
using GameFrameX.Client.Api.GameUserReward.Models;
namespace GameFrameX.Client.Api.GameUserReward.Dto;

/// <summary>
/// 查询奖励记录状态响应（原 BLGetUserRewards 出参；无 rcode，仅 rewards 数组，空数组合法）
/// </summary>
public class GetUserRewardsResponse
{
    /// <summary>
    /// 奖励记录列表（只含服务端已存在的 key）
    /// </summary>
    public List<GameUserRewardInfo> Rewards { get; set; } = new();
}