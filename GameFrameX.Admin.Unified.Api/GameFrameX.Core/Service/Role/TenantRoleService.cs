using GameFrameX.Core.Service.Base;
using GameFrameX.Core.Service.Organization;
using GameFrameX.Core.Service.Role.Dto;
using GameFrameX.Core.Service.TenantPackage;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Service.Role;

/// <summary>
/// 系统角色服务
/// </summary>
[ApiDescriptionSettings(Order = 480)]
public class TenantRoleService : BaseSystemService
{
    private readonly SqlSugarRepository<TenantRoleEntity> _systemRoleRepository;
    private readonly TenantRoleOrganizationService _systemRoleOrganizationService;
    private readonly TenantRoleMenuService _systemRoleMenuService;
    private readonly TenantOrganizationService _systemOrganizationService;
    private readonly TenantUserRoleService _systemUserRoleService;
    private readonly IServiceProvider _serviceProvider;

    public TenantRoleService(
        CurrentUserContext currentUserContext,
        TenantQuotaHelper tenantQuotaHelper,
        SqlSugarRepository<TenantRoleEntity> systemRoleRepository,
        TenantRoleOrganizationService systemRoleOrganizationService,
        TenantRoleMenuService systemRoleMenuService,
        TenantOrganizationService systemOrganizationService,
        TenantUserRoleService systemUserRoleService,
        IServiceProvider serviceProvider)
        : base(currentUserContext, tenantQuotaHelper)
    {
        _systemRoleRepository = systemRoleRepository;
        _systemRoleOrganizationService = systemRoleOrganizationService;
        _systemRoleMenuService = systemRoleMenuService;
        _systemOrganizationService = systemOrganizationService;
        _systemUserRoleService = systemUserRoleService;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 获取角色分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取角色分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantRoleResponse>> PageAsync(PageRoleRequest input)
    {
        return await _systemRoleRepository.AsQueryable()
                                   .WhereIF(!CurrentUserContext.SuperAdmin, u => u.CreatedId == CurrentUserContext.UserId) // 若非超管，则只能操作自己创建的角色
                                   .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name))
                                   .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => (u.Code ?? string.Empty).Contains(input.Code ?? string.Empty))
                                   .OrderBy(u => u.OrderNo)
                                   .Select<TenantRoleResponse>()
                                   .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取角色列表")]
    [ApiDescriptionSettings(Name = "GetList")]
    public async Task<List<RoleResponse>> GetListAsync()
    {
        // 当前用户已拥有的角色集合
        var roleIdList = CurrentUserContext.SuperAdmin ? null : await _systemUserRoleService.GetUserRoleIdListAsync(CurrentUserContext.UserId);

        return await _systemRoleRepository.AsQueryable()
                                   .WhereIF(roleIdList != null, u => u.CreatedId == CurrentUserContext.UserId || roleIdList!.Contains(u.Id)) // 若非超管，则只显示自己创建和已拥有的角色
                                   .OrderBy(u => u.OrderNo).Select<RoleResponse>().ToListAsync();
    }

    /// <summary>
    /// 增加角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加角色")]
    public async Task AddRoleAsync(AddRoleRequest input)
    {
        if (await _systemRoleRepository.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code))
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }

        // 角色数量限额检查
        if (!CurrentUserContext.SuperAdmin)
        {
            var package = await TenantQuotaHelper.GetPackageAsync(CurrentUserContext.TenantId);
            if (package != null)
            {
                var currentRoleCount = await _systemRoleRepository.AsQueryable().CountAsync();
                if (currentRoleCount >= package.MaxRoleCount)
                {
                    throw BusinessError.Oh($"角色数已达上限（{package.MaxRoleCount}），无法新增");
                }
            }
        }

        var newRole = await _systemRoleRepository.AsInsertable(input.Adapt<TenantRoleEntity>()).ExecuteReturnEntityAsync();
        input.Id = newRole.Id;
        await UpdateRoleMenuAsync(input);
    }

    /// <summary>
    /// 更新角色菜单权限
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task UpdateRoleMenuAsync(AddRoleRequest input)
    {
        if (input.MenuIdList == null || input.MenuIdList.Count < 1)
        {
            return;
        }

        // 将父节点为0的菜单排除，防止前端全选异常
        var pMenuIds = await _serviceProvider.GetRequiredService<SqlSugarRepository<GlobalMenuEntity>>().AsQueryable().Where(u => input.MenuIdList.Contains(u.Id) && u.Pid == 0).ToListAsync(u => u.Id);
        var menuIds = input.MenuIdList.Except(pMenuIds); // 差集
        await GrantMenuAsync(new RoleMenuRequest
        {
            Id = input.Id,
            MenuIdList = menuIds.ToList(),
        });
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新角色")]
    public async Task UpdateRoleAsync(UpdateRoleRequest input)
    {
        if (await _systemRoleRepository.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code && u.Id != input.Id))
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }

        await _systemRoleRepository.AsUpdateable(input.Adapt<TenantRoleEntity>()).IgnoreColumns(true)
                            .IgnoreColumns(u => new { u.DataScope, }).ExecuteCommandWithOptLockAsync(true);

        await UpdateRoleMenuAsync(input);
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [SqlSugarTransaction]
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除角色")]
    public async Task DeleteRoleAsync(DeleteRoleRequest input)
    {
        var systemRole = await _systemRoleRepository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        if (systemRole.Code == CommonConst.SysAdminRole)
        {
            throw BusinessError.Oh(RoleMenuErrorCode.DeleteSystemAdminRoleForbidden);
        }

        await _systemRoleRepository.DeleteAsync(systemRole);

        // 级联删除角色机构数据
        await _systemRoleOrganizationService.DeleteRoleOrganizationByRoleIdAsync(systemRole.Id);

        // 级联删除用户角色数据
        await _systemUserRoleService.DeleteUserRoleByRoleIdAsync(systemRole.Id);

        // 级联删除角色菜单数据
        await _systemRoleMenuService.DeleteRoleMenuByRoleIdAsync(systemRole.Id);
    }

    /// <summary>
    /// 授权角色菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("授权角色菜单")]
    [ApiDescriptionSettings(Name = "GrantMenu")]
    public async Task GrantMenuAsync(RoleMenuRequest input)
    {
        await _systemRoleMenuService.GrantRoleMenuAsync(input);
    }

    /// <summary>
    /// 授权角色数据范围
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("授权角色数据范围")]
    [ApiDescriptionSettings(Name = "GrantDataScope")]
    public async Task GrantDataScopeAsync(RoleOrganizationRequest input)
    {
        // 删除与该角色相关的用户机构缓存
        var userIdList = await _systemUserRoleService.GetUserIdListAsync(input.Id);
        foreach (var userId in userIdList)
        {
            SqlSugarFilter.DeleteUserOrganizationCache(userId, _systemRoleRepository.Context.CurrentConnectionConfig.ConfigId?.ToString() ?? string.Empty);
        }

        var role = await _systemRoleRepository.GetFirstAsync(u => u.Id == input.Id);
        var dataScope = input.DataScope;
        if (!CurrentUserContext.SuperAdmin)
        {
            // 非超级管理员没有全部数据范围权限
            if (dataScope == (int)DataScopeEnum.All)
            {
                throw BusinessError.Oh(SystemUserErrorCode.NoPermission);
            }

            // 若数据范围自定义，则判断授权数据范围是否有权限
            if (dataScope == (int)DataScopeEnum.Define)
            {
                var grantOrganizationIdList = input.OrganizationIdList ?? new List<long>();
                if (grantOrganizationIdList.Count > 0)
                {
                    var orgIdList = await _systemOrganizationService.GetUserOrganizationIdListAsync();
                    if (orgIdList.Count < 1)
                    {
                        throw BusinessError.Oh(SystemUserErrorCode.NoPermission);
                    }
                    else if (!grantOrganizationIdList.All(u => orgIdList.Any(c => c == u)))
                    {
                        throw BusinessError.Oh(SystemUserErrorCode.NoPermission);
                    }
                }
            }
        }

        role.DataScope = (DataScopeEnum)dataScope;
        await _systemRoleRepository.AsUpdateable(role).UpdateColumns(u => new { u.DataScope, }).ExecuteCommandWithOptLockAsync(true);
        await _systemRoleOrganizationService.GrantRoleOrganizationAsync(input);
    }

    /// <summary>
    /// 根据角色Id获取菜单Id集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("根据角色Id获取菜单Id集合")]
    [ApiDescriptionSettings(Name = "GetOwnMenuList")]
    public async Task<List<long>> GetOwnMenuListAsync([FromQuery] RoleRequest input)
    {
        return await _systemRoleMenuService.GetRoleMenuIdListAsync(new List<long> { input.Id, });
    }

    /// <summary>
    /// 根据角色Id获取机构Id集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("根据角色Id获取机构Id集合")]
    // Org 为路由锁定字符串（前端 /api/tenantRole/ownOrgList 实际调用），GFX-710 禁简写豁免
    [ApiDescriptionSettings(Name = "GetOwnOrgList")]
    public async Task<List<long>> GetOwnOrganizationListAsync([FromQuery] RoleRequest input)
    {
        return await _systemRoleOrganizationService.GetRoleOrganizationIdListAsync(new List<long> { input.Id, });
    }

    /// <summary>
    /// 设置角色状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("设置角色状态")]
    [ApiDescriptionSettings(Name = "SetStatus")]
    public async Task<int> SetStatusAsync(RoleRequest input)
    {
        return await _systemRoleRepository.AsUpdateable()
                                   .SetColumns(u => u.IsEnabled == input.IsEnabled)
                                   .Where(u => u.Id == input.Id)
                                   .ExecuteCommandAsync();
    }
}