using GameFrameX.Core.Authorization;
using GameFrameX.Core.Service.Base;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Organization.Dto;
using GameFrameX.Core.Service.Role;
using GameFrameX.Core.Service.Role.Dto;
using GameFrameX.Core.Service.TenantPackage;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.Organization;

/// <summary>
/// 系统机构服务
/// </summary>
[ApiDescriptionSettings(Order = 470)]
public class TenantOrganizationService : BaseSystemService
{
    private readonly SqlSugarRepository<TenantOrganizationEntity> _systemOrganizationRepository;
    private readonly SystemCacheService _systemCacheService;
    private readonly TenantUserExtensionOrganizationService _systemUserExtOrganizationService;
    private readonly TenantUserRoleService _systemUserRoleService;
    private readonly TenantRoleOrganizationService _systemRoleOrganizationService;
    private readonly IServiceProvider _serviceProvider;

    public TenantOrganizationService(
        CurrentUserContext currentUserContext,
        TenantQuotaHelper tenantQuotaHelper,
        SqlSugarRepository<TenantOrganizationEntity> systemOrganizationRepository,
        SystemCacheService systemCacheService,
        TenantUserExtensionOrganizationService systemUserExtOrganizationService,
        TenantUserRoleService systemUserRoleService,
        TenantRoleOrganizationService systemRoleOrganizationService,
        IServiceProvider serviceProvider)
        : base(currentUserContext, tenantQuotaHelper)
    {
        _systemOrganizationRepository = systemOrganizationRepository;
        _systemCacheService = systemCacheService;
        _systemUserExtOrganizationService = systemUserExtOrganizationService;
        _systemUserRoleService = systemUserRoleService;
        _systemRoleOrganizationService = systemRoleOrganizationService;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 获取运营组织列表
    /// </summary>
    /// <returns></returns>
    [AuthenticatedOnly]
    [DisplayName("获取运营组织列表")]
    [ApiDescriptionSettings(Name = "GetList")]
    public async Task<List<TenantOrganizationResponse>> GetListAsync([FromQuery] OrganizationRequest input)
    {
        // 获取拥有的机构Id集合
        var userOrganizationIdList = await GetUserOrganizationIdListAsync();

        var iSugarQueryable = _systemOrganizationRepository.AsQueryable().OrderBy(u => u.OrderNo);

        // 带条件筛选时返回列表数据
        if (!string.IsNullOrWhiteSpace(input.Name) || !string.IsNullOrWhiteSpace(input.Code) || !string.IsNullOrWhiteSpace(input.Type))
        {
            return (await iSugarQueryable.WhereIF(userOrganizationIdList.Count > 0, u => userOrganizationIdList.Contains(u.Id))
                                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name))
                                         .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => u.Code == input.Code)
                                         .WhereIF(!string.IsNullOrWhiteSpace(input.Type), u => u.Type == input.Type)
                                         .ToListAsync()).Adapt<List<TenantOrganizationResponse>>();
        }

        var systemOrg = await _systemOrganizationRepository.GetSingleAsync(u => u.Id == input.Id);
        var orgTree = new List<TenantOrganizationEntity>();
        if (CurrentUserContext?.SuperAdmin == true)
        {
            orgTree = await iSugarQueryable.ToTreeAsync(u => u.Children, u => u.Pid, input.Id);
        }
        else
        {
            orgTree = await iSugarQueryable.ToTreeAsync(u => u.Children, u => u.Pid, input.Id, userOrganizationIdList.Select(d => (object)d).ToArray());
            // 递归禁用没权限的机构（防止用户修改或创建无权的机构和用户）
            HandlerOrganizationTree(orgTree, userOrganizationIdList);
        }

        if (systemOrg != null)
        {
            systemOrg.Children = orgTree;
            orgTree = new List<TenantOrganizationEntity> { systemOrg, };
        }

        return orgTree.Adapt<List<TenantOrganizationResponse>>();
    }

    /// <summary>
    /// 递归禁用没权限的机构
    /// </summary>
    /// <param name="orgTree"></param>
    /// <param name="userOrganizationIdList"></param>
    private void HandlerOrganizationTree(List<TenantOrganizationEntity> orgTree, List<long> userOrganizationIdList)
    {
        foreach (var org in orgTree)
        {
            org.Disabled = !userOrganizationIdList.Contains(org.Id); // 设置禁用/不可选择
            if (org.Children != null)
            {
                HandlerOrganizationTree(org.Children, userOrganizationIdList);
            }
        }
    }

    /// <summary>
    /// 增加运营组织
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加运营组织")]
    public async Task<long> AddOrganizationAsync(AddOrganizationRequest input)
    {
        if (CurrentUserContext?.SuperAdmin != true && input.Pid == 0)
        {
            throw BusinessError.Oh(OrganizationErrorCode.RootOrganizationCreateForbidden);
        }

        if (await _systemOrganizationRepository.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code))
        {
            throw BusinessError.Oh(OrganizationErrorCode.OrganizationAlreadyExists);
        }

        if (CurrentUserContext?.SuperAdmin != true && input.Pid != 0)
        {
            // 新增组织父Id不是0，则进行权限校验
            var orgIdList = await GetUserOrganizationIdListAsync();
            // 新增组织的上级组织不在自己的数据范围内
            if (orgIdList.Count < 1 || !orgIdList.Contains(input.Pid))
            {
                throw BusinessError.Oh(OrganizationErrorCode.NoPermissionForOrganizationOperation);
            }
        }

        // 组织数量和层级深度限额检查
        if (CurrentUserContext?.SuperAdmin != true)
        {
            var package = await TenantQuotaHelper.GetPackageAsync(CurrentUserContext?.TenantId ?? 0);
            if (package != null)
            {
                var currentOrganizationCount = await _systemOrganizationRepository.AsQueryable().CountAsync();
                if (currentOrganizationCount >= package.MaxOrganizationCount)
                {
                    throw BusinessError.Oh($"运营组织数已达上限（{package.MaxOrganizationCount}），无法新增");
                }

                if (input.Pid != 0)
                {
                    var parentOrg = await _systemOrganizationRepository.GetFirstAsync(u => u.Id == input.Pid);
                    var newLevel = (parentOrg?.Level ?? 0) + 1;
                    if (newLevel > package.MaxOrganizationLevel)
                    {
                        throw BusinessError.Oh($"运营组织层级已达上限（{package.MaxOrganizationLevel}层），无法新增");
                    }
                }
            }
        }

        // 删除与此父机构有关的用户机构缓存
        var pOrg = await _systemOrganizationRepository.GetFirstAsync(u => u.Id == input.Pid);
        if (pOrg != null)
        {
            DeleteUserOrganizationCache(pOrg.Id, pOrg.Pid);
        }

        var newOrg = await _systemOrganizationRepository.AsInsertable(input.Adapt<TenantOrganizationEntity>()).ExecuteReturnEntityAsync();
        return newOrg.Id;
    }

    /// <summary>
    /// 更新运营组织
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [SqlSugarTransaction]
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新运营组织")]
    public async Task UpdateOrganizationAsync(UpdateOrganizationRequest input)
    {
        if (CurrentUserContext?.SuperAdmin != true && input.Pid == 0)
        {
            throw BusinessError.Oh(OrganizationErrorCode.RootOrganizationCreateForbidden);
        }

        if (input.Pid != 0)
        {
            //var pOrg = await _systemOrganizationRepository.GetFirstAsync(u => u.Id == input.Pid);
            //_ = pOrg ?? throw BusinessError.Oh(OrganizationErrorCode.ParentOrganizationNotFound);

            // 若上级组织发生变化则清空用户机构缓存
            var systemOrg = await _systemOrganizationRepository.GetFirstAsync(u => u.Id == input.Id);
            if (systemOrg != null && systemOrg.Pid != input.Pid)
            {
                // 删除与此机构、新父机构有关的用户机构缓存
                DeleteUserOrganizationCache(systemOrg.Id, input.Pid);
            }
        }

        if (input.Id == input.Pid)
        {
            throw BusinessError.Oh(OrganizationErrorCode.OrganizationIdEqualsParentForbidden);
        }

        if (await _systemOrganizationRepository.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code && u.Id != input.Id))
        {
            throw BusinessError.Oh(OrganizationErrorCode.OrganizationAlreadyExists);
        }

        // 父Id不能为自己的子节点
        var childIdList = await GetChildIdListWithSelfByIdAsync(input.Id);
        if (childIdList.Contains(input.Pid))
        {
            throw BusinessError.Oh(OrganizationErrorCode.OrganizationIdEqualsParentForbidden);
        }

        // 是否有权限操作此机构
        if (CurrentUserContext?.SuperAdmin != true)
        {
            var orgIdList = await GetUserOrganizationIdListAsync();
            if (orgIdList.Count < 1 || !orgIdList.Contains(input.Id))
            {
                throw BusinessError.Oh(OrganizationErrorCode.NoPermissionForOrganizationOperation);
            }
        }

        await _systemOrganizationRepository.AsUpdateable(input.Adapt<TenantOrganizationEntity>()).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 删除运营组织
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [SqlSugarTransaction]
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除运营组织")]
    public async Task DeleteOrganizationAsync(DeleteOrganizationRequest input)
    {
        var systemOrg = await _systemOrganizationRepository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);

        // 是否有权限操作此机构
        if (CurrentUserContext?.SuperAdmin != true)
        {
            var orgIdList = await GetUserOrganizationIdListAsync();
            if (orgIdList.Count < 1 || !orgIdList.Contains(systemOrg.Id))
            {
                throw BusinessError.Oh(OrganizationErrorCode.NoPermissionForOrganizationOperation);
            }
        }

        // 若机构为租户默认机构禁止删除
        var isTenantOrganization = await _serviceProvider.GetRequiredService<SqlSugarRepository<SystemTenantEntity>>()
                                                         .IsAnyAsync(u => u.OrganizationId == input.Id);
        if (isTenantOrganization)
        {
            throw BusinessError.Oh(OrganizationErrorCode.TenantDefaultOrganizationDeleteForbidden);
        }

        // 若机构有用户则禁止删除
        var orgHasEmp = await _serviceProvider.GetRequiredService<SqlSugarRepository<TenantUserEntity>>()
                                              .IsAnyAsync(u => u.OrganizationId == input.Id);
        if (orgHasEmp)
        {
            throw BusinessError.Oh(OrganizationErrorCode.OrganizationHasUsersDeleteForbidden);
        }

        // 若扩展机构有用户则禁止删除
        var hasExtOrganizationEmp = await _systemUserExtOrganizationService.HasUserOrganizationAsync(systemOrg.Id);
        if (hasExtOrganizationEmp)
        {
            throw BusinessError.Oh(OrganizationErrorCode.AffiliatedOrganizationHasUsersDeleteForbidden);
        }

        // 若子机构有用户则禁止删除
        var childOrganizationTreeList = await _systemOrganizationRepository.AsQueryable().ToChildListAsync(u => u.Pid, input.Id, true);
        var childOrganizationIdList = childOrganizationTreeList.Select(u => u.Id).ToList();

        // 若子机构有用户则禁止删除
        var cOrganizationHasEmp = await _serviceProvider.GetRequiredService<SqlSugarRepository<TenantUserEntity>>()
                                               .IsAnyAsync(u => childOrganizationIdList.Contains(u.OrganizationId));
        if (cOrganizationHasEmp)
        {
            throw BusinessError.Oh(OrganizationErrorCode.ChildOrganizationHasUsersDeleteForbidden);
        }

        // 删除与此机构、父机构有关的用户机构缓存
        DeleteUserOrganizationCache(systemOrg.Id, systemOrg.Pid);

        // 级联删除机构子节点
        await _systemOrganizationRepository.DeleteAsync(u => childOrganizationIdList.Contains(u.Id));

        // 级联删除角色机构数据
        await _systemRoleOrganizationService.DeleteRoleOrganizationByOrganizationIdListAsync(childOrganizationIdList);

        // 级联删除用户机构数据
        await _systemUserExtOrganizationService.DeleteUserExtOrganizationByOrganizationIdListAsync(childOrganizationIdList);
    }

    /// <summary>
    /// 删除与此机构、父机构有关的用户机构缓存
    /// </summary>
    /// <param name="orgId"></param>
    /// <param name="orgPid"></param>
    private void DeleteUserOrganizationCache(long orgId, long orgPid)
    {
        var userOrganizationKeyList = _systemCacheService.GetKeysByPrefixKey(CacheConst.KeyUserOrganization);
        if (userOrganizationKeyList != null && userOrganizationKeyList.Count > 0)
        {
            foreach (var userOrganizationKey in userOrganizationKeyList)
            {
                var userOrgs = _systemCacheService.Get<List<long>>(userOrganizationKey);
                if (userOrgs.Contains(orgId) || userOrgs.Contains(orgPid))
                {
                    var userId = long.Parse(userOrganizationKey[CacheConst.KeyUserOrganization.Length..]);
                    SqlSugarFilter.DeleteUserOrganizationCache(userId, _systemOrganizationRepository.Context.CurrentConnectionConfig.ConfigId?.ToString() ?? string.Empty);
                }
            }
        }
    }

    /// <summary>
    /// 根据用户Id获取机构Id集合
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<long>> GetUserOrganizationIdListAsync()
    {
        if (CurrentUserContext?.SuperAdmin == true)
        {
            return new List<long>();
        }

        var userId = CurrentUserContext.UserId;
        var orgIdList = _systemCacheService.Get<List<long>>($"{CacheConst.KeyUserOrganization}{userId}"); // 取缓存
        if (orgIdList == null || orgIdList.Count < 1)
        {
            // 本人新建机构集合
            var orgList0 = await _systemOrganizationRepository.AsQueryable().Where(u => u.CreatedId == userId).Select(u => u.Id).ToListAsync();
            // 扩展机构集合
            var orgList1 = await _systemUserExtOrganizationService.GetUserExtOrganizationListAsync(userId);
            // 角色机构集合
            var orgList2 = await GetUserRoleOrganizationIdListAsync(userId);
            // 机构并集
            orgIdList = orgList1.Select(u => u.OrganizationId).Union(orgList2).Union(orgList0).ToList();
            // 当前所属机构
            if (!orgIdList.Contains(CurrentUserContext.OrganizationId))
            {
                orgIdList.Add(CurrentUserContext.OrganizationId);
            }

            _systemCacheService.Set($"{CacheConst.KeyUserOrganization}{userId}", orgIdList); // 存缓存
        }

        return orgIdList;
    }

    /// <summary>
    /// 获取用户角色机构Id集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    private async Task<List<long>> GetUserRoleOrganizationIdListAsync(long userId)
    {
        var roleList = await _systemUserRoleService.GetUserRoleListAsync(userId);
        if (roleList.Count < 1)
        {
            return new List<long>(); // 空机构Id集合
        }

        return await GetUserOrganizationIdListAsync(roleList);
    }

    /// <summary>
    /// 根据角色Id集合获取机构Id集合
    /// </summary>
    /// <param name="roleList"></param>
    /// <returns></returns>
    private async Task<List<long>> GetUserOrganizationIdListAsync(List<TenantRoleResponse> roleList)
    {
        // 按最大范围策略设定(若同时拥有ALL和SELF权限，则结果ALL)
        var strongerDataScopeType = (int)DataScopeEnum.Self;

        // 角色集合拥有的数据范围
        var customDataScopeRoleIdList = new List<long>();
        if (roleList != null && roleList.Count > 0)
        {
            roleList.ForEach(u =>
            {
                if (u.DataScope == DataScopeEnum.Define)
                {
                    customDataScopeRoleIdList.Add(u.Id);
                    strongerDataScopeType = (int)u.DataScope; // 自定义数据权限时也要更新最大范围
                }
                else if ((int)u.DataScope <= strongerDataScopeType)
                {
                    strongerDataScopeType = (int)u.DataScope;
                }
            });
        }

        // 根据角色集合获取机构集合
        var orgIdList1 = await _systemRoleOrganizationService.GetRoleOrganizationIdListAsync(customDataScopeRoleIdList);
        // 根据数据范围获取机构集合
        var orgIdList2 = await GetOrganizationIdListByDataScopeAsync(strongerDataScopeType);

        // 缓存当前用户最大角色数据范围
        _systemCacheService.Set(CacheConst.KeyRoleMaxDataScope + CurrentUserContext.UserId, strongerDataScopeType);

        // 并集机构集合
        return orgIdList1.Union(orgIdList2).ToList();
    }

    /// <summary>
    /// 根据数据范围获取机构Id集合
    /// </summary>
    /// <param name="dataScope"></param>
    /// <returns></returns>
    private async Task<List<long>> GetOrganizationIdListByDataScopeAsync(int dataScope)
    {
        var orgId = CurrentUserContext.OrganizationId;
        var orgIdList = new List<long>();
        // 若数据范围是全部，则获取所有机构Id集合
        if (dataScope == (int)DataScopeEnum.All)
        {
            orgIdList = await _systemOrganizationRepository.AsQueryable().Select(u => u.Id).ToListAsync();
        }
        // 若数据范围是本部门及以下，则获取本节点和子节点集合
        else if (dataScope == (int)DataScopeEnum.DeptChild)
        {
            orgIdList = await GetChildIdListWithSelfByIdAsync(orgId);
        }
        // 若数据范围是本部门不含子节点，则直接返回本部门
        else if (dataScope == (int)DataScopeEnum.Dept)
        {
            orgIdList.Add(orgId);
        }

        return orgIdList;
    }

    /// <summary>
    /// 根据节点Id获取子节点Id集合(包含自己)
    /// </summary>
    /// <param name="pid"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<List<long>> GetChildIdListWithSelfByIdAsync(long pid)
    {
        var orgTreeList = await _systemOrganizationRepository.AsQueryable().ToChildListAsync(u => u.Pid, pid, true);
        return orgTreeList.Select(u => u.Id).ToList();
    }
}