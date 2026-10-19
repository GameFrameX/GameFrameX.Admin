using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏排行榜条目（C9 排行域：RSGet*/RSQuery*/RSHeartBeat 自建）
/// <para>一玩家一组一行（唯一键 RankType+GroupId+UserId）；组按 RankType+ConfigKey+ConfigId+RoundToken
/// 匹配（RoundToken = 榜单轮次令牌 = 玩家当前活动代币数），组容量满自动开新组。</para>
/// <para>机器人补位行同表存储（合成 userid），保证 Get/Query/HeartBeat 三视图榜单一致。</para>
/// </summary>
[SugarTable(null, "游戏排行榜条目")]
[TenantTable]
[IncrementTable]
[SugarIndex("ux_game_rank_entry", nameof(RankType), OrderByType.Asc, nameof(GroupId), OrderByType.Asc, nameof(UserId), OrderByType.Asc, true)]
public class TenantGameRankEntryEntity : EntityBasicSelect
{
    /// <summary>
    /// 榜单类型（1=BakeOut 烘焙 / 2=Multi 组队 / 3=Weekly1 周榜一 / 4=Weekly2 周榜二）
    /// </summary>
    [SugarColumn(ColumnDescription = "榜单类型", DefaultValue = "0")]
    public int RankType { get; set; }

    /// <summary>
    /// 组 id（服务端生成 "g" + 时间戳 + 序号）
    /// </summary>
    [SugarColumn(ColumnDescription = "组 id", Length = EntityStringLength.Length256)]
    public string GroupId { get; set; } = string.Empty;

    /// <summary>
    /// 活动配置 key
    /// </summary>
    [SugarColumn(ColumnDescription = "活动配置 key", Length = EntityStringLength.Length512)]
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// 活动配置 id
    /// </summary>
    [SugarColumn(ColumnDescription = "活动配置 id", DefaultValue = "0")]
    public long ConfigId { get; set; }

    /// <summary>
    /// 玩家 id（机器人行为合成 userid）
    /// </summary>
    [SugarColumn(ColumnDescription = "玩家 id", DefaultValue = "0")]
    public long UserId { get; set; }

    /// <summary>
    /// 头像 id（进榜自报）
    /// </summary>
    [SugarColumn(ColumnDescription = "头像 id", Length = EntityStringLength.Length256)]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 头像框 id（进榜自报）
    /// </summary>
    [SugarColumn(ColumnDescription = "头像框 id", Length = EntityStringLength.Length256)]
    public string IconFrame { get; set; } = string.Empty;

    /// <summary>
    /// 徽章 id（进榜自报）
    /// </summary>
    [SugarColumn(ColumnDescription = "徽章 id", Length = EntityStringLength.Length256)]
    public string Badge { get; set; } = string.Empty;

    /// <summary>
    /// 当前分数（= 玩家当前活动代币数；心跳更新）
    /// </summary>
    [SugarColumn(ColumnDescription = "当前分数", DefaultValue = "0")]
    public long Score { get; set; }

    /// <summary>
    /// 组内名次缓存（组内按 Score 降序计算；查询时实时重算）
    /// </summary>
    [SugarColumn(ColumnDescription = "组内名次缓存", DefaultValue = "0")]
    public int Rank { get; set; }

    /// <summary>
    /// 榜单轮次令牌（报名时玩家活动代币数 = 初始分；token 变化视为新一轮开新组）
    /// </summary>
    [SugarColumn(ColumnDescription = "榜单轮次令牌", DefaultValue = "0")]
    public long RoundToken { get; set; }
}