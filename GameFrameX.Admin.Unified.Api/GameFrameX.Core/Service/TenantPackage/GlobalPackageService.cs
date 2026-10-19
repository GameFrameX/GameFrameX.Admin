using GameFrameX.Core.Authorization;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.TenantMenuSync;
using GameFrameX.Core.Service.TenantPackage.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.Filter;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Core.Service.TenantPackage;

/// <summary>
/// 系统租户套餐管理服务
/// </summary>
[ApiDescriptionSettings(Order = 391)]
public class GlobalPackageService : IAdminApiController
{
    private readonly SqlSugarRepository<GlobalPackageEntity> _globalPackageRepository;
    private readonly SqlSugarRepository<GlobalPackageMenuEntity> _systemTenantPackageMenuRepository;
    private readonly SqlSugarRepository<SystemTenantEntity> _systemTenantRepository;
    private readonly SqlSugarRepository<GlobalMenuEntity> _systemMenuRepository;
    private readonly CurrentUserContext _currentUserContext;
    private readonly TenantExpirationService _tenantExpirationService;
    private readonly TenantMenuSyncQueue _tenantMenuSyncQueue;

    public GlobalPackageService(
        SqlSugarRepository<GlobalPackageEntity> globalPackageRepository,
        SqlSugarRepository<GlobalPackageMenuEntity> systemTenantPackageMenuRepository,
        SqlSugarRepository<SystemTenantEntity> systemTenantRepository,
        SqlSugarRepository<GlobalMenuEntity> systemMenuRepository,
        CurrentUserContext currentUserContext,
        TenantExpirationService tenantExpirationService,
        TenantMenuSyncQueue tenantMenuSyncQueue)
    {
        _globalPackageRepository = globalPackageRepository;
        _systemTenantPackageMenuRepository = systemTenantPackageMenuRepository;
        _systemTenantRepository = systemTenantRepository;
        _systemMenuRepository = systemMenuRepository;
        _currentUserContext = currentUserContext;
        _tenantExpirationService = tenantExpirationService;
        _tenantMenuSyncQueue = tenantMenuSyncQueue;
    }

    /// <summary>
    /// 获取套餐分页列表
    /// </summary>
    [DisplayName("获取套餐分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantPackageResponse>> PageAsync(PageTenantPackageRequest input)
    {
        return await _globalPackageRepository.AsQueryable()
                                            .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains((input.Name ?? string.Empty).Trim()))
                                            .OrderBy(u => u.OrderNo)
                                            .Select(u => new TenantPackageResponse
                                            {
                                                Id = u.Id,
                                                Name = u.Name,
                                                Code = u.Code,
                                                MaxUserCount = u.MaxUserCount,
                                                MaxOrganizationCount = u.MaxOrganizationCount,
                                                MaxOrganizationLevel = u.MaxOrganizationLevel,
                                                MaxRoleCount = u.MaxRoleCount,
                                                MaxGameAppCount = u.MaxGameAppCount,
                                                MaxPackageCountPerApp = u.MaxPackageCountPerApp,
                                                GracePeriodDays = u.GracePeriodDays,
                                                OrderNo = u.OrderNo,
                                                Remark = u.Remark,
                                                IsEnabled = u.IsEnabled,
                                            })
                                            .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加套餐
    /// </summary>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加套餐")]
    public async Task AddAsync(AddTenantPackageRequest input)
    {
        var isExist = await _globalPackageRepository.IsAnyAsync(u => u.Code == input.Code);
        if (isExist)
        {
            throw BusinessError.Oh("套餐编码已存在");
        }

        var package = input.Adapt<GlobalPackageEntity>();
        await _globalPackageRepository.InsertAsync(package);

        await UpdatePackageMenusAsync(package.Id, input.MenuIdList);
    }

    /// <summary>
    /// 更新套餐
    /// </summary>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新套餐")]
    public async Task UpdateAsync(UpdateTenantPackageRequest input)
    {
        var isExist = await _globalPackageRepository.IsAnyAsync(u => u.Code == input.Code && u.Id != input.Id);
        if (isExist)
        {
            throw BusinessError.Oh("套餐编码已存在");
        }

        var package = input.Adapt<GlobalPackageEntity>();
        await _globalPackageRepository.AsUpdateable(package).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);

        await UpdatePackageMenusAsync(input.Id, input.MenuIdList);
    }

    /// <summary>
    /// 删除套餐
    /// </summary>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除套餐")]
    public async Task DeleteAsync(DeleteTenantPackageRequest input)
    {
        var hasTenants = await _systemTenantRepository.IsAnyAsync(u => u.PackageId == input.Id);
        if (hasTenants)
        {
            throw BusinessError.Oh("该套餐下存在关联租户，无法删除");
        }

        await _systemTenantPackageMenuRepository.DeleteAsync(u => u.PackageId == input.Id);
        await _globalPackageRepository.DeleteAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取套餐详情（含关联菜单）
    /// </summary>
    [DisplayName("获取套餐详情")]
    [ApiDescriptionSettings(Name = "GetDetail")]
    public async Task<TenantPackageResponse> GetDetailAsync([FromQuery] long id)
    {
        var package = await _globalPackageRepository.GetByIdAsync(id);
        if (package == null)
        {
            throw BusinessError.Oh("套餐不存在");
        }

        var menuIdList = await _systemTenantPackageMenuRepository.AsQueryable()
                                                          .Where(u => u.PackageId == id)
                                                          .Select(u => u.MenuId)
                                                          .ToListAsync();

        var output = package.Adapt<TenantPackageResponse>();
        output.MenuIdList = menuIdList;
        return output;
    }

    /// <summary>
    /// 获取套餐关联的菜单Id列表
    /// </summary>
    [NonAction]
    public async Task<List<long>> GetPackageMenuIdListAsync(long packageId)
    {
        return await _systemTenantPackageMenuRepository.AsQueryable()
                                                .Where(u => u.PackageId == packageId)
                                                .Select(u => u.MenuId)
                                                .ToListAsync();
    }

    /// <summary>
    /// 更新套餐关联菜单
    /// </summary>
    private async Task UpdatePackageMenusAsync(long packageId, List<long> menuIdList)
    {
        // C354 T4：Enterprise 为默认租户保留套餐，菜单清单由启动幂等补全收敛（种子演化跟随），
        // 编辑端只读——与库中现值一致的无变化提交放行（基础信息编辑回传场景），实际变更拒绝
        if (packageId == TenantDefaultDataTemplates.EnterprisePackageId)
        {
            var currentMenuIds = await _systemTenantPackageMenuRepository.AsQueryable()
                                                           .Where(u => u.PackageId == packageId)
                                                           .Select(u => u.MenuId)
                                                           .ToListAsync();
            if (!new HashSet<long>(currentMenuIds).SetEquals(menuIdList))
            {
                throw BusinessError.Oh(TenantErrorCode.EnterprisePackageMenuReadOnly);
            }

            return;
        }

        await _systemTenantPackageMenuRepository.DeleteAsync(u => u.PackageId == packageId);

        if (menuIdList is { Count: > 0, })
        {
            menuIdList = await ExcludeSuperAdminOnlyMenusAsync(menuIdList);
            // C354 T3：服务端补祖先目录闭包后落库（编辑期所见即所得；幂等——已含祖先的输入无变化）
            menuIdList = TenantPackageMenuClosure.CompleteAncestors(menuIdList, MenuSeedCatalog.AllDefinitions).ToList();
            var menus = menuIdList.Select(menuId => new GlobalPackageMenuEntity
            {
                PackageId = packageId,
                MenuId = menuId,
            }).ToList();
            await _systemTenantPackageMenuRepository.InsertRangeAsync(menus);
        }

        // C353 触发①：套餐菜单保存后，将该套餐下的同步范围租户入队（异步收敛，幂等去重）；
        // C354 T6：默认租户纳入同步范围，枚举口径与 GetTenantDbListAsync 一致（不再排除 MainTenantId）
        var packageTenantIds = await _systemTenantRepository.AsQueryable()
                                               .Where(u => u.PackageId == packageId
                                                           && u.TenantType == TenantTypeEnum.Db
                                                           && u.IsEnabled == true
                                                           && u.ConfigId != SqlSugarConst.MainConfigId)
                                               .Select(u => u.Id)
                                               .ToListAsync();
        foreach (var tenantId in packageTenantIds)
        {
            _tenantMenuSyncQueue.TryEnqueue(tenantId, "package-saved");
        }
    }

    /// <summary>
    /// 排除仅超级管理员可见的菜单及其子菜单，防止套餐权限下放给租户。
    /// </summary>
    private async Task<List<long>> ExcludeSuperAdminOnlyMenusAsync(List<long> menuIdList)
    {
        var reservedMenuIds = CommonConst.SuperAdminOnlyMenuRootIds.ToHashSet();
        foreach (var rootId in CommonConst.SuperAdminOnlyMenuRootIds)
        {
            var menuTreeList = await _systemMenuRepository.AsQueryable().ToChildListAsync(u => u.Pid, rootId, true);
            reservedMenuIds.UnionWith(menuTreeList.Select(u => u.Id));
        }

        var tenantReadableMenuIds = MenuSeedCatalog.TenantReadableSuperAdminOnlyMenuIds;
        return menuIdList.Where(u => !reservedMenuIds.Contains(u) || tenantReadableMenuIds.Contains(u)).Distinct().ToList();
    }

    /// <summary>
    /// 获取当前租户的到期状态（前端横幅用）
    /// </summary>
    [AuthenticatedOnly]
    [BypassTenantExpiration]
    [DisplayName("获取租户到期状态")]
    [ApiDescriptionSettings(Name = "GetTenantExpirationStatus")]
    public async Task<HttpJsonResultData<TenantExpirationStatusResponse>> GetTenantExpirationStatusAsync()
    {
        if (_currentUserContext.SuperAdmin)
        {
            return HttpJsonResultData<TenantExpirationStatusResponse>.Success(new TenantExpirationStatusResponse
            {
                Status = "active",
                DaysLeft = null,
            });
        }

        var tenantId = _currentUserContext.TenantId;
        var status = await _tenantExpirationService.GetExpirationStatusAsync(tenantId);

        var tenant = await _systemTenantRepository.GetFirstAsync(u => u.Id == tenantId);
        var daysLeft = tenant?.ExpireTime != null
                           ? (int)((tenant.ExpireTime.Value - TimerHelper.UnixTimeSeconds()) / (24 * 60 * 60))
                           : (int?)null;

        return HttpJsonResultData<TenantExpirationStatusResponse>.Success(new TenantExpirationStatusResponse
        {
            Status = status.ToString().ToLowerInvariant(),
            DaysLeft = daysLeft,
        });
    }

    /// <summary>
    /// 获取当前租户的套餐信息（只读，租户管理员可查看）
    /// </summary>
    [AuthenticatedOnly]
    [DisplayName("获取当前租户套餐信息")]
    [ApiDescriptionSettings(Name = "GetCurrentTenantPackage")]
    public async Task<TenantPackageResponse?> GetCurrentTenantPackageAsync()
    {
        if (_currentUserContext.SuperAdmin)
        {
            return null;
        }

        var tenant = await _systemTenantRepository.GetFirstAsync(u => u.Id == _currentUserContext.TenantId);
        if (tenant?.PackageId == null)
        {
            return null;
        }

        return await GetDetailAsync(tenant.PackageId.Value);
    }
}