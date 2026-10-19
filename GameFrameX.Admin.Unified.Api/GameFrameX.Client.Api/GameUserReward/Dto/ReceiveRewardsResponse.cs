namespace GameFrameX.Client.Api.GameUserReward.Dto;

/// <summary>
/// 领取奖励响应（原 BLReceiveRewards1 出参；Rcode 仅允许 0/1/2——其他值客户端当成功发奖，
/// 服务端出现未知错误码等于白送奖励）
/// </summary>
public class ReceiveRewardsResponse
{
    /// <summary>
    /// 业务结果码（0 = 成功；1 = 已领过；2 = 过期）
    /// </summary>
    public long Rcode { get; set; }
}