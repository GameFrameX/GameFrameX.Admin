using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Role.Dto;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Service.Role;

/// <summary>
/// 系统角色菜单服务
/// </summary>
public class TenantRoleMenuService
{
    private readonly SqlSugarRepository<TenantRoleMenuEntity> _systemRoleMenuRepository;
    private readonly SqlSugarRepository<GlobalMenuEntity> _systemMenuRepository;
    private readonly SystemCacheService _systemCacheService;
    private readonly SqlSugarTransactionRunner _transactionRunner;

    public TenantRoleMenuService(SqlSugarRepository<TenantRoleMenuEntity> systemRoleMenuRepository,
        SqlSugarRepository<GlobalMenuEntity> systemMenuRepository,
        SystemCacheService systemCacheService,
        SqlSugarTransactionRunner transactionRunner)
    {
        _systemRoleMenuRepository = systemRoleMenuRepository;
        _systemMenuRepository = systemMenuRepository;
        _systemCacheService = systemCacheService;
        _transactionRunner = transactionRunner;
    }

    /// <summary>
    /// 根据角色Id集合获取菜单Id集合
    /// </summary>
    /// <param name="roleIdList"></param>
    /// <returns></returns>
    public async Task<List<long>> GetRoleMenuIdListAsync(List<long> roleIdList)
    {
        var menuIdList = await _systemRoleMenuRepository.AsQueryable()
                                                 .Where(u => roleIdList.Contains(u.RoleId))
                                                 .Select(u => u.MenuId).ToListAsync();
        return await ExcludeSuperAdminOnlyMenusAsync(menuIdList);
    }

    /// <summary>
    /// 授权角色菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task GrantRoleMenuAsync(RoleMenuRequest input)
    {
        await _transactionRunner.ExecuteAsync(async () =>
        {
            if (input.MenuIdList == null || input.MenuIdList.Count == 0)
            {
                return;
            }

            input.MenuIdList = await ExcludeSuperAdminOnlyMenusAsync(input.MenuIdList);

            await _systemRoleMenuRepository.DeleteAsync(u => u.RoleId == input.Id);
            var menus = input.MenuIdList.Select(u => new TenantRoleMenuEntity
            {
                RoleId = input.Id,
                MenuId = u,
            }).ToList();
            await _systemRoleMenuRepository.InsertRangeAsync(menus);

            // 清除缓存
            // _systemCacheService.RemoveByPrefixKey(CacheConst.KeyUserMenu);
            _systemCacheService.RemoveByPrefixKey(CacheConst.KeyUserButton);
        });
    }

    /// <summary>
    /// 根据角色Id删除角色菜单
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public async Task DeleteRoleMenuByRoleIdAsync(long roleId)
    {
        await _systemRoleMenuRepository.DeleteAsync(u => u.RoleId == roleId);
    }

    /// <summary>
    /// 排除仅超级管理员可见的菜单及其子菜单，防止权限下放给普通角色。
    /// </summary>
    private async Task<List<long>> ExcludeSuperAdminOnlyMenusAsync(List<long> menuIdList)
    {
        if (menuIdList == null || menuIdList.Count == 0)
        {
            return menuIdList ?? new List<long>();
        }

        var reservedMenuIds = await GetSuperAdminOnlyMenuIdsAsync();
        var tenantReadableMenuIds = MenuSeedCatalog.TenantReadableSuperAdminOnlyMenuIds;
        return menuIdList.Where(u => !reservedMenuIds.Contains(u) || tenantReadableMenuIds.Contains(u)).Distinct().ToList();
    }

    /// <summary>
    /// 获取仅超级管理员可见的菜单整棵子树。
    /// </summary>
    private async Task<HashSet<long>> GetSuperAdminOnlyMenuIdsAsync()
    {
        var reservedMenuIds = CommonConst.SuperAdminOnlyMenuRootIds.ToHashSet();

        foreach (var rootId in CommonConst.SuperAdminOnlyMenuRootIds)
        {
            var menuTreeList = await _systemMenuRepository.AsQueryable().ToChildListAsync(u => u.Pid, rootId, true);
            reservedMenuIds.UnionWith(menuTreeList.Select(u => u.Id));
        }

        return reservedMenuIds;
    }
}