using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Role.Dto;
using GameFrameX.Core.Service.User.Dto;

namespace GameFrameX.Core.Service.User;

/// <summary>
/// 系统用户角色服务
/// </summary>
public class TenantUserRoleService
{
    private readonly SqlSugarRepository<TenantUserRoleEntity> _systemUserRoleRepository;
    private readonly SystemCacheService _systemCacheService;
    private readonly SqlSugarTransactionRunner _transactionRunner;

    public TenantUserRoleService(SqlSugarRepository<TenantUserRoleEntity> systemUserRoleRepository,
        SystemCacheService systemCacheService,
        SqlSugarTransactionRunner transactionRunner)
    {
        _systemUserRoleRepository = systemUserRoleRepository;
        _systemCacheService = systemCacheService;
        _transactionRunner = transactionRunner;
    }

    /// <summary>
    /// 授权用户角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task GrantUserRoleAsync(UserRoleRequest input)
    {
        await _transactionRunner.ExecuteAsync(async () =>
        {
            await _systemUserRoleRepository.DeleteAsync(u => u.UserId == input.UserId);

            if (input.RoleIdList == null || input.RoleIdList.Count < 1)
            {
                return;
            }

            var roles = input.RoleIdList.Select(u => new TenantUserRoleEntity
            {
                UserId = input.UserId,
                RoleId = u,
            }).ToList();
            await _systemUserRoleRepository.InsertRangeAsync(roles);
            _systemCacheService.Remove(CacheConst.KeyUserButton + input.UserId);
        });
    }

    /// <summary>
    /// 根据角色Id删除用户角色
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public async Task DeleteUserRoleByRoleIdAsync(long roleId)
    {
        await _systemUserRoleRepository.AsQueryable()
                                .Where(u => u.RoleId == roleId)
                                .Select(u => u.UserId)
                                .ForEachAsync(userId => { _systemCacheService.Remove(CacheConst.KeyUserButton + userId); });

        await _systemUserRoleRepository.DeleteAsync(u => u.RoleId == roleId);
    }

    /// <summary>
    /// 根据用户Id删除用户角色
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task DeleteUserRoleByUserIdAsync(long userId)
    {
        await _systemUserRoleRepository.DeleteAsync(u => u.UserId == userId);
        _systemCacheService.Remove(CacheConst.KeyUserButton + userId);
    }

    /// <summary>
    /// 根据用户Id获取角色集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<TenantRoleResponse>> GetUserRoleListAsync(long userId)
    {
        var systemUserRoleList = await _systemUserRoleRepository.AsQueryable()
                                                         .Includes(u => u.TenantRoleEntity)
                                                         .Where(u => u.UserId == userId).ToListAsync();
        return systemUserRoleList.Where(u => u.TenantRoleEntity != null).Select(u => u.TenantRoleEntity.Adapt<TenantRoleResponse>()).ToList();
    }

    /// <summary>
    /// 根据用户Id获取角色Id集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<long>> GetUserRoleIdListAsync(long userId)
    {
        return await _systemUserRoleRepository.AsQueryable()
                                       .Where(u => u.UserId == userId).Select(u => u.RoleId).ToListAsync();
    }

    /// <summary>
    /// 根据角色Id获取用户Id集合
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public async Task<List<long>> GetUserIdListAsync(long roleId)
    {
        return await _systemUserRoleRepository.AsQueryable()
                                       .Where(u => u.RoleId == roleId).Select(u => u.UserId).ToListAsync();
    }
}