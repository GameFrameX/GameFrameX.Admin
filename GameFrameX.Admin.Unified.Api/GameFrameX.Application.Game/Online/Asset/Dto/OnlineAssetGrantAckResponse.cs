namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// Online 资产命令（发放/撤销）确认响应：透传 Online 首次执行结果（vault:C4 幂等语义——同幂等键重试返回首次结果，不产生第二笔变更）。
/// </summary>
public class OnlineAssetGrantAckResponse
{
    /// <summary>
    /// 交易 Id（本次命令产生或首次命中的交易）。
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 幂等键（服务端按业务单号规则生成的回执）。
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// 交易状态（字符串透传：Succeeded / Failed / Reverted 等）。
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 执行时间（Unix 秒；幂等重试时为首次执行时间）。
    /// </summary>
    public long OccurredAt { get; set; }
}
