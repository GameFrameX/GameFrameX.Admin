namespace GameFrameX.Client.Api.GameUserReward.Dto;

/// <summary>
/// 查询奖励记录状态请求（原 BLGetUserRewards 入参 GetUserRewardsReq）
/// </summary>
public class GetUserRewardsRequest
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 待查 key 列表（如 op_cp_&lt;eventId&gt;_&lt;missionId&gt;；查不到的 key 不返回元素）
    /// </summary>
    public List<string> RewardKeys { get; set; } = new();
}