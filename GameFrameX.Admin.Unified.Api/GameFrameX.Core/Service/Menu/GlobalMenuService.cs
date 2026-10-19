using GameFrameX.Core.Authorization;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Menu.Dto;
using GameFrameX.Core.Service.Role;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Service.Menu;

/// <summary>
/// 系统菜单服务
/// </summary>
[ApiDescriptionSettings(Order = 450)]
public class GlobalMenuService : IAdminApiController
{
    private readonly CurrentUserContext _currentUserContext;
    private readonly SqlSugarRepository<GlobalMenuEntity> _systemMenuRepository;
    private readonly SystemCacheService _systemCacheService;
    private readonly IServiceProvider _serviceProvider;
    private readonly TenantDbConnectionProvider _tenantDbConnectionProvider;
    private readonly ISqlSugarClient _sqlSugarClient;

    static GlobalMenuService()
    {
        // 注册本程序集中的 Mapster IRegister（如 SystemMenuMapper），使 Adapt<MenuResponse>() 填充 Meta（标题/图标/缓存标志）。
        // 缺失该扫描会导致登录菜单树 Meta 为 null，前端侧边栏无标题、isKeepAlive 取值崩溃。CLR 静态构造保证只执行一次。
        TypeAdapterConfig.GlobalSettings.Scan(typeof(SystemMenuMapper).Assembly);
    }

    public GlobalMenuService(CurrentUserContext currentUserContext,
        SqlSugarRepository<GlobalMenuEntity> systemMenuRepository,
        SystemCacheService systemCacheService,
        IServiceProvider serviceProvider,
        TenantDbConnectionProvider tenantDbConnectionProvider,
        ISqlSugarClient sqlSugarClient)
    {
        _currentUserContext = currentUserContext;
        _systemMenuRepository = systemMenuRepository;
        _systemCacheService = systemCacheService;
        _serviceProvider = serviceProvider;
        _tenantDbConnectionProvider = tenantDbConnectionProvider;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 获取登录菜单树
    /// </summary>
    /// <returns></returns>
    [AuthenticatedOnly]
    [DisplayName("获取登录菜单树")]
    [ApiDescriptionSettings(Name = "GetLoginMenuTree")]
    public async Task<List<MenuResponse>> GetLoginMenuTreeAsync()
    {
        if (_currentUserContext.SuperAdmin)
        {
            var visibleMenuIds = MenuSeedCatalog.SuperAdminVisibleMenuIds.ToList();
            var menuList = await _systemMenuRepository.AsQueryable()
                                               .Where(u => u.Type != MenuTypeEnum.Button && u.IsEnabled == true)
                                               .Where(u => visibleMenuIds.Contains(u.Id))
                                               .OrderBy(u => new { u.Type, u.OrderNo, u.Id, }).ToTreeAsync(u => u.Children, u => u.Pid, 0);
            return menuList.Adapt<List<MenuResponse>>();
        }
        else
        {
            var menuIdList = await GetMenuIdListAsync();
            // B5: 过滤整棵 SuperAdmin-only 子树（而非仅 AllowTenant 子集）。
            // 旧逻辑只剔除 TenantReadableSuperAdminOnlyMenuIds，当某菜单由"租户可见"改为"超管专属"(SuperAdminOnly=true, AllowTenant=false)后，
            // 既有租户的 tenant_role_menu 仍残留旧授权，且不在 AllowTenant 集合内，导致菜单仍出现在租户导航中（点击后端 403）。改为剔除整棵超管子树可防御性修复。
            var menuTreeIds = menuIdList
                              .Where(u => !MenuSeedCatalog.SuperAdminOnlySubtreeMenuIds.Contains(u))
                              .ToArray();
            var menuTree = await _systemMenuRepository.AsQueryable()
                                               .Where(u => u.IsEnabled == true)
                                               .OrderBy(u => new { u.Type, u.OrderNo, u.Id, }).ToTreeAsync(u => u.Children, u => u.Pid, 0, menuTreeIds.Select(d => (object)d).ToArray());
            DeleteButtonFromMenuTree(menuTree);
            return menuTree.Adapt<List<MenuResponse>>();
        }
    }

    /// <summary>
    /// 删除登录菜单树里面的按钮
    /// </summary>
    private void DeleteButtonFromMenuTree(List<GlobalMenuEntity> menuList)
    {
        if (menuList == null)
        {
            return;
        }

        for (var i = menuList.Count - 1; i >= 0; i--)
        {
            var menu = menuList[i];
            if (menu.Type == MenuTypeEnum.Button)
            {
                menuList.Remove(menu);
            }
            else if (menu.Children.Count > 0)
            {
                DeleteButtonFromMenuTree(menu.Children);
            }
        }
    }

    /// <summary>
    /// 获取菜单列表
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("获取菜单列表")]
    [ApiDescriptionSettings(Name = "GetList")]
    public async Task<List<GlobalMenuResponse>> GetListAsync([FromQuery] MenuRequest input)
    {
        var menuIdList = _currentUserContext.SuperAdmin ? new List<long>() : await GetMenuIdListAsync();

        // 有筛选条件时返回list列表（防止构造不出树）
        if (!string.IsNullOrWhiteSpace(input.Title) || input.Type is > 0)
        {
            return (await _systemMenuRepository.AsQueryable()
                                        .Where(u => u.IsDeleted == false)
                                        .WhereIF(!string.IsNullOrWhiteSpace(input.Title), u => u.Title.Contains(input.Title))
                                        .WhereIF(input.Type is > 0, u => u.Type == input.Type)
                                        .WhereIF(menuIdList.Count > 1, u => menuIdList.Contains(u.Id))
                                        .OrderBy(u => u.OrderNo).ToListAsync()).Adapt<List<GlobalMenuResponse>>();
        }

        var menuTree = _currentUserContext.SuperAdmin
                           ? await _systemMenuRepository.AsQueryable()
                                                 .Where(u => u.IsDeleted == false)
                                                 .OrderBy(u => u.OrderNo).ToTreeAsync(u => u.Children, u => u.Pid, 0)
                           : // 超级管理员
                           await _systemMenuRepository.AsQueryable()
                                               .Where(u => u.IsDeleted == false)
                                               .OrderBy(u => u.OrderNo).ToTreeAsync(u => u.Children, u => u.Pid, 0, menuIdList.Select(d => (object)d).ToArray()); // 角色菜单授权时
        return menuTree.Adapt<List<GlobalMenuResponse>>();
    }

    /// <summary>
    /// 增加菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加菜单")]
    public async Task AddMenuAsync(AddMenuRequest input)
    {
        var isExist = input.Type != MenuTypeEnum.Button
                          ? await _systemMenuRepository.IsAnyAsync(u => u.Title == input.Title)
                          : await _systemMenuRepository.IsAnyAsync(u => u.Key == input.Key);
        if (isExist)
        {
            throw BusinessError.Oh(RoleMenuErrorCode.MenuAlreadyExists);
        }

        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            if (await _systemMenuRepository.IsAnyAsync(u => u.Name == input.Name))
            {
                throw BusinessError.Oh(RoleMenuErrorCode.RouteNameDuplicate);
            }
        }

        // 校验菜单参数
        var systemMenu = input.Adapt<GlobalMenuEntity>();
        CheckMenuParam(systemMenu);

        // AddRequest 的 long? RowVersion 为 null 时 Adapt 会把实体默认 0 覆盖为 null，
        // 插入后 row_version 为 NULL，首次乐观锁更新必然冲突（同 InnerAddAsync 兜底）
        systemMenu.RowVersion ??= 0;

        // 管理端新增允许 Key 为空，但启动校验 ValidateDbMenus 要求有效菜单必须带 Key，
        // 缺失会让服务重启直接失败。按 seed 惯例兜底生成：目录/菜单 route:{Name}，按钮 manual:{Permission|Title}
        if (string.IsNullOrWhiteSpace(systemMenu.Key))
        {
            systemMenu.Key = systemMenu.Type == MenuTypeEnum.Button
                ? $"manual:{(string.IsNullOrWhiteSpace(systemMenu.Permission) ? systemMenu.Title : systemMenu.Permission)}"
                : $"route:{systemMenu.Name}";
        }

        await _systemMenuRepository.InsertAsync(systemMenu);

        // 清除缓存
        DeleteMenuCache();
    }

    /// <summary>
    /// 更新菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新菜单")]
    public async Task UpdateMenuAsync(UpdateMenuRequest input)
    {
        if (input.Id == input.Pid)
        {
            throw BusinessError.Oh(RoleMenuErrorCode.NodeEqualsParentForbidden);
        }

        var isExist = input.Type != MenuTypeEnum.Button
                          ? await _systemMenuRepository.IsAnyAsync(u => u.Title == input.Title && u.Type == input.Type && u.Id != input.Id)
                          : await _systemMenuRepository.IsAnyAsync(u => u.Key == input.Key && u.Type == input.Type && u.Id != input.Id);
        if (isExist)
        {
            throw BusinessError.Oh(RoleMenuErrorCode.MenuAlreadyExists);
        }

        // 校验菜单参数
        var systemMenu = input.Adapt<GlobalMenuEntity>();
        CheckMenuParam(systemMenu);

        await _systemMenuRepository.AsUpdateable(systemMenu).ExecuteCommandWithOptLockAsync(true);

        // 清除缓存
        DeleteMenuCache();
    }

    /// <summary>
    /// 删除菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [SqlSugarTransaction]
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除菜单")]
    public async Task DeleteMenuAsync(DeleteMenuRequest input)
    {
        var menuTreeList = await _systemMenuRepository.AsQueryable().ToChildListAsync(u => u.Pid, input.Id, true);
        var menuIdList = menuTreeList.Select(u => u.Id).ToList();

        await _systemMenuRepository.DeleteAsync(u => menuIdList.Contains(u.Id));

        // 级联删除角色菜单数据：授权分散在各租户库，平台侧删除时无租户上下文，
        // 经 TenantRoleMenuService 解析租户仓储会在构造期因缺 TenantId 抛 NoPermission；改为直接遍历租户库清理
        await CascadeDeleteRoleMenusAsync(menuIdList);

        // 清除缓存
        DeleteMenuCache();
    }

    /// <summary>
    /// 遍历所有库隔离租户，删除指定菜单的角色授权（tenant_role_menu）。
    /// </summary>
    private async Task CascadeDeleteRoleMenusAsync(List<long> menuIdList)
    {
        var mainDb = _sqlSugarClient.AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);
        var tenants = mainDb.Queryable<SystemTenantEntity>()
                            .Filter(null, true)
                            .Where(u => u.TenantType == TenantTypeEnum.Db)
                            .ToList();
        foreach (var tenantEntity in tenants)
        {
            var tenantDb = _tenantDbConnectionProvider.GetTenantDbConnectionScope(tenantEntity.Id);
            if (tenantDb == null)
            {
                continue;
            }

            await tenantDb.Deleteable<TenantRoleMenuEntity>().Where(u => menuIdList.Contains(u.MenuId)).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 增加和编辑时检查菜单数据
    /// </summary>
    /// <param name="menu"></param>
    private static void CheckMenuParam(GlobalMenuEntity menu)
    {
        var permission = menu.Permission;
        if (menu.Type == MenuTypeEnum.Button)
        {
            menu.Name = null;
            menu.Path = null;
            menu.Component = null;
            menu.Icon = null;
            menu.Redirect = null;
            menu.OutLink = null;
            menu.IsHide = false;
            menu.IsKeepAlive = true;
            menu.IsAffix = false;
            menu.IsIframe = false;

            if (string.IsNullOrEmpty(permission))
            {
                throw BusinessError.Oh(RoleMenuErrorCode.PermissionCodeEmpty);
            }
        }
        else
        {
            menu.Permission = null;
        }
    }

    /// <summary>
    /// 获取用户拥有按钮权限集合（缓存）
    /// </summary>
    /// <returns></returns>
    [AuthenticatedOnly]
    [DisplayName("获取按钮权限集合")]
    [ApiDescriptionSettings(Name = "GetOwnButtonPermList")]
    public async Task<List<string>> GetOwnButtonPermListAsync()
    {
        var userId = _currentUserContext.UserId;
        var permissions = _systemCacheService.Get<List<string>>(CacheConst.KeyUserButton + userId);
        if (permissions == null || permissions.Count == 0)
        {
            var menuIdList = _currentUserContext.SuperAdmin ? new List<long>() : await GetMenuIdListAsync();
            permissions = await _systemMenuRepository.AsQueryable()
                                              .Where(u => u.Type == MenuTypeEnum.Button)
                                              .Where(u => u.IsEnabled == true)
                                              .WhereIF(menuIdList.Count > 0, u => menuIdList.Contains(u.Id))
                                              .Select(u => u.Key ?? string.Empty).ToListAsync();
            _systemCacheService.Set(CacheConst.KeyUserButton + userId, permissions);
        }

        return permissions;
    }

    /// <summary>
    /// 获取当前用户已授权的菜单Id集合（缓存，含目录/菜单/按钮，来自 TenantRoleMenuEntity）。
    /// <para>供基于稳定身份（Key）的鉴权使用：<see cref="GameFrameX.Core.Const.MenuSeed.StableSeedId.Menu(string)"/> 由 Key 派生的 Id 与本集合比对。</para>
    /// </summary>
    /// <returns>非超管返回其角色已授权的 MenuId 集合；超管本不应被调用（JwtHandler 提前放行），此处仍返回空集以保证安全。</returns>
    [DisplayName("获取已授权菜单Id集合")]
    [ApiDescriptionSettings(Name = "GetOwnMenuIdList")]
    public async Task<List<long>> GetOwnMenuIdListAsync()
    {
        var userId = _currentUserContext.UserId;
        var menuIds = _systemCacheService.Get<List<long>>(CacheConst.KeyUserMenu + userId);
        if (menuIds == null || menuIds.Count == 0)
        {
            menuIds = await GetMenuIdListAsync();
            menuIds = await AppendAutoMenuIdsAsync(menuIds);
            _systemCacheService.Set(CacheConst.KeyUserMenu + userId, menuIds);
        }

        return menuIds;
    }

    /// <summary>
    /// 按「跟随所属页面」并入隐藏自动权限按钮（ApiEndpointCatalogSync 为未显式绑定端点兜底创建）。
    /// <para>这些按钮 IsHide 且 Key 为 permission:auto:* 前缀，角色授权树不展示、无法通过角色授予；
    /// 用户已授权其所属页面（pid 命中 ownMenuIds）即视为拥有该页面的隐式端点（detail/list 等）。</para>
    /// </summary>
    private async Task<List<long>> AppendAutoMenuIdsAsync(List<long> menuIds)
    {
        if (menuIds.Count == 0)
        {
            return menuIds;
        }

        var ownIdSet = menuIds.ToHashSet();
        var autoButtons = await _systemMenuRepository.AsQueryable()
                                              .Where(u => u.Type == MenuTypeEnum.Button && u.IsHide == true && u.Pid > 0)
                                              .Where(u => u.Key != null && u.Key.StartsWith("permission:auto:"))
                                              .Select(u => new { u.Id, u.Pid })
                                              .ToListAsync();
        foreach (var button in autoButtons)
        {
            if (ownIdSet.Contains(button.Pid))
            {
                menuIds.Add(button.Id);
            }
        }

        return menuIds;
    }

    /// <summary>
    /// 获取系统所有按钮权限集合（缓存）
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<List<string>> GetAllButtonPermListAsync()
    {
        var permissions = _systemCacheService.Get<List<string>>(CacheConst.KeyUserButton + 0);
        if (permissions == null || permissions.Count == 0)
        {
            permissions = await _systemMenuRepository.AsQueryable()
                                              .Where(u => u.Type == MenuTypeEnum.Button)
                                              .Where(u => u.IsEnabled == true)
                                              .Select(u => u.Key ?? string.Empty).ToListAsync();
            _systemCacheService.Set(CacheConst.KeyUserButton + 0, permissions);
        }

        return permissions;
    }

    /// <summary>
    /// 获取端点权限目录（U6/C42）：(HttpMethod, RoutePath) → <see cref="EndpointPermissionEntry"/>。
    /// <para>由 <c>ApiEndpointCatalogSync</c> 启动时构建并预热；此处请求期按需返回缓存，缓存缺失则即时重建。</para>
    /// </summary>
    [DisplayName("获取端点权限目录")]
    public async Task<Dictionary<string, EndpointPermissionEntry>> GetEndpointPermissionMapAsync()
    {
        var map = _systemCacheService.Get<Dictionary<string, EndpointPermissionEntry>>(CacheConst.KeyEndpointByRoute);
        if (map != null && map.Count > 0)
        {
            return map;
        }

        map = await BuildEndpointPermissionMapAsync();
        _systemCacheService.Set(CacheConst.KeyEndpointByRoute, map);
        return map;
    }

    /// <summary>
    /// 失效端点权限目录缓存（绑定/端点变更后调用）。
    /// <para>进程内缓存失效入口，非 HTTP 端点。</para>
    /// </summary>
    [NonAction]
    public void InvalidateEndpointPermissionMap()
    {
        _systemCacheService.Remove(CacheConst.KeyEndpointByRoute);
    }

    private async Task<Dictionary<string, EndpointPermissionEntry>> BuildEndpointPermissionMapAsync()
    {
        var endpointRepository = _serviceProvider.GetRequiredService<SqlSugarRepository<GlobalApiEndpointEntity>>();
        var bindingRepository = _serviceProvider.GetRequiredService<SqlSugarRepository<GlobalMenuEndpointEntity>>();
        var endpoints = await endpointRepository.AsQueryable().Where(u => u.IsEnabled == true).ToListAsync();
        var bindings = await bindingRepository.AsQueryable().ToListAsync();
        var byEndpoint = bindings.GroupBy(b => b.EndpointId).ToDictionary(g => g.Key, g => g.Select(b => b.MenuId).ToHashSet());

        var map = new Dictionary<string, EndpointPermissionEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var ep in endpoints)
        {
            byEndpoint.TryGetValue(ep.Id, out var bound);
            map[ApiEndpointIdentity.ToEndpointKey(ep.HttpMethod, ep.RoutePath)] = new EndpointPermissionEntry
            {
                EndpointId = ep.Id,
                HttpMethod = ApiEndpointIdentity.NormalizeHttpMethod(ep.HttpMethod),
                RoutePath = ep.RoutePath,
                IsPublic = ep.IsPublic,
                IsAuthenticatedOnly = ep.IsAuthenticatedOnly,
                BoundMenuIds = bound ?? new HashSet<long>(),
            };
        }

        return map;
    }

    /// <summary>
    /// 清除菜单和按钮缓存
    /// </summary>
    private void DeleteMenuCache()
    {
        _systemCacheService.RemoveByPrefixKey(CacheConst.KeyUserMenu);
        _systemCacheService.RemoveByPrefixKey(CacheConst.KeyUserButton);
    }

    /// <summary>
    /// 获取当前用户菜单Id集合
    /// </summary>
    /// <returns></returns>
    private async Task<List<long>> GetMenuIdListAsync()
    {
        var roleIdList = await _serviceProvider.GetRequiredService<TenantUserRoleService>().GetUserRoleIdListAsync(_currentUserContext.UserId);
        return await _serviceProvider.GetRequiredService<TenantRoleMenuService>().GetRoleMenuIdListAsync(roleIdList);
    }
}