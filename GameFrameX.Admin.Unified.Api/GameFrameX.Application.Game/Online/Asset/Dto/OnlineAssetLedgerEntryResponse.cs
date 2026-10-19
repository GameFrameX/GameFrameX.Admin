namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// 资产流水行（vault:C4 账本不变量字段：TransactionId / Player / App / Server / 来源 / 原因 / 业务单号 / 操作者 / 前后值 / 时间；
/// App/Server 由查询作用域隐含，不在行内重复）。线缆与 Admin 响应共用形态（账本行无脱敏面）。
/// </summary>
public class OnlineAssetLedgerEntryResponse
{
    /// <summary>
    /// 交易 Id（一笔业务操作 = 一个 TransactionId，账本行经其反查交易详情）。
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 玩家 Id。
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 货币或道具类型（如 gold / diamond / item:1001）。
    /// </summary>
    public string CurrencyOrItemType { get; set; } = string.Empty;

    /// <summary>
    /// 变更前值（该资产维度的余额/库存快照）。
    /// </summary>
    public long BeforeValue { get; set; }

    /// <summary>
    /// 变更后值。
    /// </summary>
    public long AfterValue { get; set; }

    /// <summary>
    /// 来源（Admin 人工 / 支付 / 邮件 / 兑换码 / 对局 / 活动，vault:C4 六类）。
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// 原因（进审计的操作理由原文）。
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 业务单号（幂等键载体，同单号重试只生效一次）。
    /// </summary>
    public string BusinessOrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// 操作者 Id（Admin 人工来源时为 Admin 操作者；系统来源为系统标识）。
    /// </summary>
    public string OperatorId { get; set; } = string.Empty;

    /// <summary>
    /// 发生时间（Unix 秒）。
    /// </summary>
    public long OccurredAt { get; set; }
}
