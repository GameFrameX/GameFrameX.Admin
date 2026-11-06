using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Role.Dto;
using GameFrameX.Core.Service.User.Dto;

namespace GameFrameX.Core.Service.User;

/// <summary>
/// 系统用户角色服务
/// </summary>
public class UserRoleService
{
    private readonly SqlSugarRepository<UserRoleEntity> _systemUserRoleRepository;
    private readonly SystemCacheService _systemCacheService;
    private readonly SqlSugarTransactionRunner _transactionRunner;

    public UserRoleService(SqlSugarRepository<UserRoleEntity> systemUserRoleRepository,
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

            if (input.RoleIdList != null && input.RoleIdList.Count > 0)
            {
                var roles = input.RoleIdList.Select(u => new UserRoleEntity
                {
                    UserId = input.UserId,
                    RoleId = u,
                }).ToList();
                await _systemUserRoleRepository.InsertRangeAsync(roles);
            }
            _systemCacheService.Remove(CacheConst.KeyUserMenu + input.UserId);
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
                                .ForEachAsync(userId =>
                                {
                                    _systemCacheService.Remove(CacheConst.KeyUserMenu + userId);
                                    _systemCacheService.Remove(CacheConst.KeyUserButton + userId);
                                });

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
        _systemCacheService.Remove(CacheConst.KeyUserMenu + userId);
        _systemCacheService.Remove(CacheConst.KeyUserButton + userId);
    }

    /// <summary>
    /// 根据用户Id获取角色集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<RoleResponse>> GetUserRoleListAsync(long userId)
    {
        var systemUserRoleList = await _systemUserRoleRepository.AsQueryable()
                                                         .Includes(u => u.RoleEntity)
                                                         .Where(u => u.UserId == userId).ToListAsync();
        return systemUserRoleList.Where(u => u.RoleEntity != null).Select(u => u.RoleEntity.Adapt<RoleResponse>()).ToList();
    }

    /// <summary>
    /// 根据用户Id获取角色Id集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<long>> GetUserRoleIdListAsync(long userId)
    {
        return await _systemUserRoleRepository.AsQueryable()
                                       .InnerJoin<RoleEntity>((userRole, role) => userRole.RoleId == role.Id)
                                       .Where((userRole, role) => userRole.UserId == userId)
                                       .Where((userRole, role) => (role.IsEnabled == null || role.IsEnabled == true)
                                           && (role.IsDeleted == null || role.IsDeleted == false))
                                       .Select((userRole, role) => userRole.RoleId).ToListAsync();
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