using GameFrameX.Core.Service.User.Dto;

namespace GameFrameX.Core.Service.User;

/// <summary>
/// 系统用户扩展机构服务
/// </summary>
public class TenantUserExtensionOrganizationService
{
    private readonly SqlSugarRepository<TenantUserExtensionOrganizationEntity> _systemUserExtOrganizationRepository;
    private readonly SqlSugarTransactionRunner _transactionRunner;

    public TenantUserExtensionOrganizationService(SqlSugarRepository<TenantUserExtensionOrganizationEntity> systemUserExtOrganizationRepository, SqlSugarTransactionRunner transactionRunner)
    {
        _systemUserExtOrganizationRepository = systemUserExtOrganizationRepository;
        _transactionRunner = transactionRunner;
    }

    /// <summary>
    /// 获取用户扩展机构集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<TenantUserExtensionOrganizationResponse>> GetUserExtOrganizationListAsync(long userId)
    {
        return (await _systemUserExtOrganizationRepository.GetListAsync(u => u.UserId == userId)).Adapt<List<TenantUserExtensionOrganizationResponse>>();
    }

    /// <summary>
    /// 更新用户扩展机构
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="extOrganizationList"></param>
    /// <returns></returns>
    public async Task UpdateUserExtOrganizationAsync(long userId, List<TenantUserExtensionOrganizationEntity> extOrganizationList)
    {
        await _transactionRunner.ExecuteAsync(async () =>
        {
            await _systemUserExtOrganizationRepository.DeleteAsync(u => u.UserId == userId);

            if (extOrganizationList == null || extOrganizationList.Count < 1)
            {
                return;
            }

            extOrganizationList.ForEach(u => { u.UserId = userId; });
            await _systemUserExtOrganizationRepository.InsertRangeAsync(extOrganizationList);
        });
    }

    /// <summary>
    /// 根据机构Id集合删除扩展机构
    /// </summary>
    /// <param name="orgIdList"></param>
    /// <returns></returns>
    public async Task DeleteUserExtOrganizationByOrganizationIdListAsync(List<long> orgIdList)
    {
        await _systemUserExtOrganizationRepository.DeleteAsync(u => orgIdList.Contains(u.OrganizationId));
    }

    /// <summary>
    /// 根据用户Id删除扩展机构
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task DeleteUserExtOrganizationByUserIdAsync(long userId)
    {
        await _systemUserExtOrganizationRepository.DeleteAsync(u => u.UserId == userId);
    }

    /// <summary>
    /// 根据机构Id判断是否有用户
    /// </summary>
    /// <param name="orgId"></param>
    /// <returns></returns>
    public async Task<bool> HasUserOrganizationAsync(long orgId)
    {
        return await _systemUserExtOrganizationRepository.IsAnyAsync(u => u.OrganizationId == orgId);
    }

    /// <summary>
    /// 根据职位Id判断是否有用户
    /// </summary>
    /// <param name="posId"></param>
    /// <returns></returns>
    public async Task<bool> HasUserPosAsync(long posId)
    {
        return await _systemUserExtOrganizationRepository.IsAnyAsync(u => u.PosId == posId);
    }
}