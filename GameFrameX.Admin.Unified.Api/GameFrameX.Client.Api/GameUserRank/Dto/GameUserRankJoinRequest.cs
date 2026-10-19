namespace GameFrameX.Client.Api.GameUserRank.Dto;

/// <summary>
/// 排行报名请求（原 RSGetBakeOutRank/GetMultiRank/GetWeeklyRank1/GetWeeklyRank2 入参，四 op 同构）
/// </summary>
public class GameUserRankJoinRequest
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
    /// 活动配置 key
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// 活动配置 id
    /// </summary>
    public long ConfigId { get; set; }

    /// <summary>
    /// 组类型（旧 op 透传；BakeOut 无此字段。自建分组键不含它，仅接收不参与匹配）
    /// </summary>
    public long? GroupType { get; set; }

    /// <summary>
    /// 展示名（进榜自报存档）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 头像 id（进榜自报存档）
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 头像框 id（进榜自报存档）
    /// </summary>
    public string IconFrame { get; set; } = string.Empty;

    /// <summary>
    /// 徽章 id（进榜自报存档）
    /// </summary>
    public string Badge { get; set; } = string.Empty;

    /// <summary>
    /// 榜单轮次令牌（= 玩家当前活动代币数即初始分；同 token 玩家入同组）
    /// </summary>
    public long Token { get; set; }
}