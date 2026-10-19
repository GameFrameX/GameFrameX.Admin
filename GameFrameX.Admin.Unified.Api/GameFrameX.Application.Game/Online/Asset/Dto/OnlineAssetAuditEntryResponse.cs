namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// 资产审计行（命令类型 + 操作者 + 目标玩家 + 时间；作用域 App/Server 由查询作用域隐含）。
/// 线缆与 Admin 响应共用形态（审计行无脱敏面）。
/// </summary>
public class OnlineAssetAuditEntryResponse
{
    /// <summary>
    /// 关联交易 Id（经其反查交易详情与账本行）。
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 命令类型（grant / revoke 等）。
    /// </summary>
    public string CommandType { get; set; } = string.Empty;

    /// <summary>
    /// 操作者 Id。
    /// </summary>
    public string OperatorId { get; set; } = string.Empty;

    /// <summary>
    /// 目标玩家 Id。
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 原因（进审计的操作理由原文）。
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 业务单号。
    /// </summary>
    public string BusinessOrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// 发生时间（Unix 秒）。
    /// </summary>
    public long OccurredAt { get; set; }
}
