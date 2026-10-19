namespace GameFrameX.Client.Api.GameUserRank.Dto;

/// <summary>
/// 排行心跳响应（原 RSHeartBeat 出参：四榜当前名次，无行回 0）
/// </summary>
public class GameUserRankHeartBeatResponse
{
    /// <summary>
    /// 业务结果码（0 = 成功）
    /// </summary>
    public long Rcode { get; set; }

    /// <summary>
    /// BakeOut 榜名次（无行 = 0）
    /// </summary>
    public long BakeoutRank { get; set; }

    /// <summary>
    /// Multi 榜名次（无行 = 0）
    /// </summary>
    public long MultiRank { get; set; }

    /// <summary>
    /// Weekly1 榜名次（无行 = 0）
    /// </summary>
    public long Weekly1Rank { get; set; }

    /// <summary>
    /// Weekly2 榜名次（无行 = 0）
    /// </summary>
    public long Weekly2Rank { get; set; }
}