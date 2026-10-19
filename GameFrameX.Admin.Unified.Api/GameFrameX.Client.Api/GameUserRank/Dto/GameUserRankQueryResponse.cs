
using GameFrameX.Client.Api.GameUserRank.Models;
namespace GameFrameX.Client.Api.GameUserRank.Dto;

/// <summary>
/// 排行查询响应（原 RS Query* 出参：按 GroupId 回组内全部行）
/// </summary>
public class GameUserRankQueryResponse
{
    /// <summary>
    /// 业务结果码（0 = 成功）
    /// </summary>
    public long Rcode { get; set; }

    /// <summary>
    /// 组内全部行（rank 实时重算，按名次排序）
    /// </summary>
    public List<GameRankInfo> Ranks { get; set; } = new();
}