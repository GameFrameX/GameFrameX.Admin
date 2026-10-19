namespace GameFrameX.Client.Api.GameUserReward.Dto;

/// <summary>
/// 领取奖励请求（原 BLReceiveRewards1 入参 ReceiveRewards1Req）
/// </summary>
public class ReceiveRewardsRequest
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 奖励 key（可逗号分隔复合 key）
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 防重领 token（客户端 installUuid+serverTime+key 本地拼接；同 token 重放须同结果）
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 1 = 记录应已存在（op_cp 任务/客服补偿）；0 = 允许领取时创建（问卷/mail 通知）
    /// </summary>
    public long CheckExist { get; set; }
}