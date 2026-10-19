namespace GameFrameX.Client.Api.GameUserReward.Models;

/// <summary>
/// 奖励记录条目（原 BL RewardInfo；嵌套行结构，非端点 IO）
/// </summary>
public class GameUserRewardInfo
{
    /// <summary>
    /// 奖励 key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 奖励内容 JSON（客户端 json.decode 后取 .rewards；空 = 无下发来源）
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 领取状态（0 = 未领 / 1 = 已领）
    /// </summary>
    public long Status { get; set; }
}