using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏玩家存档（C8 存档三件套）
/// <para>分表透传存储：BranchesJson 原样保存客户端 24 张分表行数组，服务端不解析行内容（round-trip 字节等价的前提）。</para>
/// </summary>
[SugarTable(null, "游戏玩家存档")]
[TenantTable]
[IncrementTable]
[SugarIndex("ux_game_user_archive_user", nameof(UserId), OrderByType.Asc, true)]
public class TenantGameUserArchiveEntity : EntityBasicSelect
{
    /// <summary>
    /// 玩家 id（租户内唯一，一玩家一行）
    /// </summary>
    [SugarColumn(ColumnDescription = "玩家 id", DefaultValue = "0")]
    public long UserId { get; set; }

    /// <summary>
    /// 乐观锁版本戳（服务端下发，客户端下次上送作为期望值）
    /// </summary>
    [SugarColumn(ColumnDescription = "乐观锁版本戳", DefaultValue = "0")]
    public long SyncTime { get; set; }

    /// <summary>
    /// 24 张分表透传 JSON（键 = 分表名，值 = 行数组原样字符串）
    /// </summary>
    [SugarColumn(ColumnDescription = "分表透传 JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string BranchesJson { get; set; } = string.Empty;

    /// <summary>
    /// user 分表摘要-金币（GetOtherProgress / 排行展示免解析）
    /// </summary>
    [SugarColumn(ColumnDescription = "user 分表摘要-金币")]
    public string Gold { get; set; } = string.Empty;

    /// <summary>
    /// user 分表摘要-宝石
    /// </summary>
    [SugarColumn(ColumnDescription = "user 分表摘要-宝石")]
    public string Gem { get; set; } = string.Empty;

    /// <summary>
    /// user 分表摘要-跳过券
    /// </summary>
    [SugarColumn(ColumnDescription = "user 分表摘要-跳过券")]
    public string SkipProp { get; set; } = string.Empty;

    /// <summary>
    /// user 分表摘要-等级
    /// </summary>
    [SugarColumn(ColumnDescription = "user 分表摘要-等级")]
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// user 分表摘要-经验
    /// </summary>
    [SugarColumn(ColumnDescription = "user 分表摘要-经验")]
    public string Exp { get; set; } = string.Empty;

    /// <summary>
    /// user 分表摘要-任务进度
    /// </summary>
    [SugarColumn(ColumnDescription = "user 分表摘要-任务进度")]
    public string Progress { get; set; } = string.Empty;

    /// <summary>
    /// user 分表摘要-体力
    /// </summary>
    [SugarColumn(ColumnDescription = "user 分表摘要-体力")]
    public string Energy { get; set; } = string.Empty;

    /// <summary>
    /// user 分表摘要-订单天数
    /// </summary>
    [SugarColumn(ColumnDescription = "user 分表摘要-订单天数", DefaultValue = "0")]
    public long Day { get; set; }

    /// <summary>
    /// 当前主线任务 id
    /// </summary>
    [SugarColumn(ColumnDescription = "当前主线任务 id")]
    public string CurrentTask { get; set; } = string.Empty;

    /// <summary>
    /// 当前章节 id
    /// </summary>
    [SugarColumn(ColumnDescription = "当前章节 id", DefaultValue = "0")]
    public long CurrentChapter { get; set; }

    /// <summary>
    /// 订单所属章节 id
    /// </summary>
    [SugarColumn(ColumnDescription = "订单所属章节 id", DefaultValue = "0")]
    public long OrderChapterId { get; set; }

    /// <summary>
    /// 订单组 id
    /// </summary>
    [SugarColumn(ColumnDescription = "订单组 id", DefaultValue = "0")]
    public long OrderGroupId { get; set; }

    /// <summary>
    /// 头像 id（冗余，排行/好友直接展示）
    /// </summary>
    [SugarColumn(ColumnDescription = "头像 id")]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 昵称（冗余；基类 Name 为管理面通用字段，玩家昵称独立列）
    /// </summary>
    [SugarColumn(ColumnDescription = "昵称")]
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 当前佩戴徽章（冗余）
    /// </summary>
    [SugarColumn(ColumnDescription = "当前佩戴徽章")]
    public string Badge { get; set; } = string.Empty;

    /// <summary>
    /// 徽章数量
    /// </summary>
    [SugarColumn(ColumnDescription = "徽章数量", DefaultValue = "0")]
    public long BadgeCount { get; set; }

    /// <summary>
    /// 当前头像框（冗余）
    /// </summary>
    [SugarColumn(ColumnDescription = "当前头像框")]
    public string Frame { get; set; } = string.Empty;

    /// <summary>
    /// 头像框数量
    /// </summary>
    [SugarColumn(ColumnDescription = "头像框数量", DefaultValue = "0")]
    public long FrameCount { get; set; }

    /// <summary>
    /// 安装 uuid（换端判定）
    /// </summary>
    [SugarColumn(ColumnDescription = "安装 uuid")]
    public string InstallationId { get; set; } = string.Empty;

    /// <summary>
    /// 机型（换端判定）
    /// </summary>
    [SugarColumn(ColumnDescription = "机型")]
    public string DeviceModel { get; set; } = string.Empty;

    /// <summary>
    /// 客户端版本（基类无版本字段，命名避开歧义）
    /// </summary>
    [SugarColumn(ColumnDescription = "客户端版本")]
    public string ClientVersion { get; set; } = string.Empty;

    /// <summary>
    /// 登录会话 id（P0-2：C13 登录时生成下发，存档行单副本承载）
    /// <para>ponytail: 单副本起步（一玩家一行一列，后登录覆盖）；多端并发顶号天花板 = Redis SETNX 抢占，见 C13 change。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "登录会话 id", IsNullable = true)]
    public long? SessionId { get; set; }

    /// <summary>
    /// 会话发起机型（P0-2：与 SessionId 成对写入，顶号/换端排查）
    /// </summary>
    [SugarColumn(ColumnDescription = "会话发起机型", IsNullable = true)]
    public string? SessionDeviceModel { get; set; }
}