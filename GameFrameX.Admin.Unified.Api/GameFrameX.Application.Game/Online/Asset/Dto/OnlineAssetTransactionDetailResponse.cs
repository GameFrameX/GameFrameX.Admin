namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// 资产交易详情响应（Transaction/Grant 全字段：TransactionId、IdempotencyKey、状态、来源、原因、业务单号、操作者、Grant 明细、时间；
/// vault:C4 交易不变量——每笔变更可追溯，VC-3.14 Admin 展示半边）。
/// </summary>
public class OnlineAssetTransactionDetailResponse
{
    /// <summary>
    /// 交易 Id。
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 幂等键（业务单号承载；同键重试返回首次结果）。
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// 交易状态（字符串透传 Online 状态机：Succeeded / Failed / Reverted 等，Admin 不解释状态机只展示）。
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 来源（vault:C4 六类）。
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// 原因（进审计的操作理由原文）。
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 业务单号。
    /// </summary>
    public string BusinessOrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// 操作者 Id。
    /// </summary>
    public string OperatorId { get; set; } = string.Empty;

    /// <summary>
    /// 目标玩家 Id。
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 发生时间（Unix 秒）。
    /// </summary>
    public long OccurredAt { get; set; }

    /// <summary>
    /// Grant 明细（本次交易的资产维度变更列表）。
    /// </summary>
    public List<OnlineAssetTransactionGrantItemResponse> GrantItems { get; set; } = new();
}
