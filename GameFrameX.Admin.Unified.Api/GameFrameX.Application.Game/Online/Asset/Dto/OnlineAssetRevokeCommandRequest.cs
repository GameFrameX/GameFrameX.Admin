namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// Online 资产撤销命令请求体（Admin → Online 线缆契约；mock 先行，联调以 vault 详表回写）。
/// </summary>
public class OnlineAssetRevokeCommandRequest
{
    /// <summary>
    /// 目标玩家 Id。
    /// </summary>
    public string PlayerId { get; set; } = null!;

    /// <summary>
    /// 被撤销的交易 Id。
    /// </summary>
    public string TransactionId { get; set; } = null!;

    /// <summary>
    /// 撤销原因（进 Online 审计）。
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 业务单号（幂等键的语义载体）。
    /// </summary>
    public string BusinessOrderNumber { get; set; } = null!;

    /// <summary>
    /// 幂等键（= BusinessOrderNumber；同键重试只生效一次，返回首次结果）。
    /// </summary>
    public string IdempotencyKey { get; set; } = null!;
}
