using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏玩家奖励记录（C8 奖励域：BLGetUserRewards / BLReceiveRewards1 自建）
/// <para>一玩家一 key 一行；Content 为奖励内容 JSON（客户端 json.decode 后取 .rewards），
/// 最小自建模型下领取时创建的记录 Content 为空（客户端发奖用本地任务配置，不从服务端 content 取）。</para>
/// <para>token 幂等：同 token 重放必须返回相同结果（客户端响应丢失重试时补发本地奖励，防丢奖）。</para>
/// </summary>
[SugarTable(null, "游戏玩家奖励记录")]
[TenantTable]
[IncrementTable]
[SugarIndex("ux_game_user_reward_user_key", nameof(UserId), OrderByType.Asc, nameof(RewardKey), OrderByType.Asc, true)]
public class TenantGameUserRewardEntity : EntityBasicSelect
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    [SugarColumn(ColumnDescription = "玩家 id", DefaultValue = "0")]
    public long UserId { get; set; }

    /// <summary>
    /// 奖励 key（可逗号分隔复合 key，客户端逐段标记；列名避开 SQL 关键字）
    /// </summary>
    [SugarColumn(ColumnDescription = "奖励 key", Length = EntityStringLength.Length512)]
    public string RewardKey { get; set; } = string.Empty;

    /// <summary>
    /// 奖励内容 JSON（旧协议 RewardInfo.content；空 = 无下发来源）
    /// </summary>
    [SugarColumn(ColumnDescription = "奖励内容 JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 领取状态（0 = 未领 / 1 = 已领）
    /// </summary>
    [SugarColumn(ColumnDescription = "领取状态", DefaultValue = "0")]
    public long Status { get; set; }

    /// <summary>
    /// 最近一次领取使用的防重放 token
    /// </summary>
    [SugarColumn(ColumnDescription = "最近领取 token", Length = EntityStringLength.Length512)]
    public string LastToken { get; set; } = string.Empty;

    /// <summary>
    /// 领取时刻（Unix 秒）
    /// </summary>
    [SugarColumn(ColumnDescription = "领取时刻", DefaultValue = "0")]
    public long ReceivedAt { get; set; }
}