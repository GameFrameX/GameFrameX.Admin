
using GameFrameX.Client.Api.GameUserRank.Models;
namespace GameFrameX.Client.Api.GameUserRank.Dto;

/// <summary>
/// 排行报名响应（原 RS GetAsync* 出参：返回全组）
/// </summary>
public class GameUserRankJoinResponse
{
    /// <summary>
    /// 业务结果码（0 = 成功）
    /// </summary>
    public long Rcode { get; set; }

    /// <summary>
    /// 榜期是否已结算（1 = 已结束；朴素时间窗判定）
    /// </summary>
    public long IsFinished { get; set; }

    /// <summary>
    /// 分配的组 id
    /// </summary>
    public string GroupId { get; set; } = string.Empty;

    /// <summary>
    /// 组内全部行（含机器人补位行，按名次排序）
    /// </summary>
    public List<GameRankInfo> Ranks { get; set; } = new();
}