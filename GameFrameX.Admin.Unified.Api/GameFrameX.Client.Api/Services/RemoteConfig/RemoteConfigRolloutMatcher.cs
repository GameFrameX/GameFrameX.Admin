using GameFrameX.Application.Game.App.Rollout;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;

namespace GameFrameX.Client.Api.Services.RemoteConfig;

/// <summary>
/// 远程配置记录级投放规则。
/// <para>灰度判定委托 <see cref="StableDeviceRolloutMatcher"/>（Application.Game 唯一实现），
/// 与活动/功能开关投放共用同一套取模 + 白名单旁路语义。</para>
/// </summary>
public static class RemoteConfigRolloutMatcher
{
    /// <summary>
    /// 判断远程配置记录是否对当前设备生效。
    /// </summary>
    /// <param name="record">候选远程配置记录。</param>
    /// <param name="tenantId">当前租户 ID。</param>
    /// <param name="deviceId">客户端稳定设备标识。</param>
    /// <returns>命中返回 true，否则返回 false。</returns>
    public static bool IsMatch(TenantGameAppRemoteConfigEntity record, long tenantId, string? deviceId)
    {
        if (record.IsEnabled == false)
        {
            return false;
        }

        return StableDeviceRolloutMatcher.IsMatch(
            tenantId: tenantId,
            appId: record.AppId,
            recordId: record.Id,
            deviceId: deviceId,
            rolloutPercent: record.RolloutPercent,
            deviceWhitelistJson: record.DeviceWhitelistJson);
    }

    /// <summary>
    /// 从候选远程配置记录中选择最终投放记录。
    /// </summary>
    /// <param name="candidates">候选记录和作用域精确度。</param>
    /// <param name="tenantId">当前租户 ID。</param>
    /// <param name="deviceId">客户端稳定设备标识。</param>
    /// <returns>命中的最高优先级记录；没有命中时返回 null。</returns>
    public static TenantGameAppRemoteConfigEntity? SelectWinner(
        IEnumerable<RemoteConfigScoredCandidate> candidates,
        long tenantId,
        string? deviceId)
    {
        return candidates.Where(item => IsMatch(item.Record, tenantId, deviceId))
                         .OrderByDescending(item => item.Record.Priority)
                         .ThenByDescending(item => item.Score)
                         .ThenByDescending(item => item.Record.AppId.HasValue && item.Record.AppId.Value > 0)
                         .ThenByDescending(item => item.Record.Id)
                         .Select(item => item.Record)
                         .FirstOrDefault();
    }
}
