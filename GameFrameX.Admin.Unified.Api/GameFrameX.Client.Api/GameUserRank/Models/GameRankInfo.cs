namespace GameFrameX.Client.Api.GameUserRank.Models;

/// <summary>
/// 排行榜行（原 RS RankInfo；嵌套行结构）
/// </summary>
public class GameRankInfo
{
    /// <summary>
    /// 玩家 id（机器人行为合成 userid）
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 展示名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 头像 id
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 头像框 id
    /// </summary>
    public string IconFrame { get; set; } = string.Empty;

    /// <summary>
    /// 当前分数
    /// </summary>
    public long Score { get; set; }

    /// <summary>
    /// 组内名次
    /// </summary>
    public long Rank { get; set; }

    /// <summary>
    /// 徽章 id
    /// </summary>
    public string Badge { get; set; } = string.Empty;
}