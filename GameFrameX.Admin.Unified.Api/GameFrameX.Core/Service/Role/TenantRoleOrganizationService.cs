using GameFrameX.Core.Service.Role.Dto;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.Role;

/// <summary>
/// 系统角色机构服务
/// </summary>
public class TenantRoleOrganizationService
{
    private readonly SqlSugarRepository<TenantRoleOrganizationEntity> _systemRoleOrganizationRepository;
    private readonly SqlSugarTransactionRunner _transactionRunner;

    public TenantRoleOrganizationService(SqlSugarRepository<TenantRoleOrganizationEntity> systemRoleOrganizationRepository, SqlSugarTransactionRunner transactionRunner)
    {
        _systemRoleOrganizationRepository = systemRoleOrganizationRepository;
        _transactionRunner = transactionRunner;
    }

    /// <summary>
    /// 授权角色机构
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task GrantRoleOrganizationAsync(RoleOrganizationRequest input)
    {
        await _transactionRunner.ExecuteAsync(async () =>
        {
            await _systemRoleOrganizationRepository.DeleteAsync(u => u.RoleId == input.Id);
            if (input.DataScope == (int)DataScopeEnum.Define)
            {
                var roleOrgs = input.OrganizationIdList.Select(u => new TenantRoleOrganizationEntity
                {
                    RoleId = input.Id,
                    OrganizationId = u,
                }).ToList();
                await _systemRoleOrganizationRepository.InsertRangeAsync(roleOrgs);
            }
        });
    }

    /// <summary>
    /// 根据角色Id集合获取角色机构Id集合
    /// </summary>
    /// <param name="roleIdList"></param>
    /// <returns></returns>
    public async Task<List<long>> GetRoleOrganizationIdListAsync(List<long> roleIdList)
    {
        return await _systemRoleOrganizationRepository.AsQueryable()
                                      .Where(u => roleIdList.Contains(u.RoleId))
                                      .Select(u => u.OrganizationId).ToListAsync();
    }

    /// <summary>
    /// 根据机构Id集合删除角色机构
    /// </summary>
    /// <param name="orgIdList"></param>
    /// <returns></returns>
    public async Task DeleteRoleOrganizationByOrganizationIdListAsync(List<long> orgIdList)
    {
        await _systemRoleOrganizationRepository.DeleteAsync(u => orgIdList.Contains(u.OrganizationId));
    }

    /// <summary>
    /// 根据角色Id删除角色机构
    /// </summary>
    /// <param name="roleId"></param>
    /// <returns></returns>
    public async Task DeleteRoleOrganizationByRoleIdAsync(long roleId)
    {
        await _systemRoleOrganizationRepository.DeleteAsync(u => u.RoleId == roleId);
    }
}