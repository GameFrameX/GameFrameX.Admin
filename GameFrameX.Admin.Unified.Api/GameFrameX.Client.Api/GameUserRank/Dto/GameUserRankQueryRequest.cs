namespace GameFrameX.Client.Api.GameUserRank.Dto;

/// <summary>
/// 排行查询请求（原 RSQueryBakeOutRank/QueryMultiRank/QueryWeeklyRank1/QueryWeeklyRank2 入参，四 op 完全同构）
/// </summary>
public class GameUserRankQueryRequest
{
    /// <summary>
    /// 榜单类型（1=BakeOut / 2=Multi / 3=Weekly1 / 4=Weekly2）
    /// </summary>
    public int RankType { get; set; }

    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 组 id（GetAsync 报名时返回）
    /// </summary>
    public string GroupId { get; set; } = string.Empty;
}