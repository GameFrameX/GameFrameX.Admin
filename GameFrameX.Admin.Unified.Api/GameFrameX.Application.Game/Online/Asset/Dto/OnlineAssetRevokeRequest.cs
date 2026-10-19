namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// Online 资产人工撤销输入参数（高危；与 <see cref="OnlineAssetGrantRequest"/> 同构 + 目标 TransactionId）。
/// <para>撤销的是一笔已生效交易的资产变更，经 Online Grant/Transaction API 执行，Admin 侧零直写。</para>
/// </summary>
public class OnlineAssetRevokeRequest
{
    /// <summary>
    /// 应用作用域（必填，经 OnlineScopeContextFactory 校验租户归属）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id必须大于0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服作用域（必填，经 OnlineScopeContextFactory 校验租户与应用归属）。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long ServerId { get; set; }

    /// <summary>
    /// 目标玩家 Id。
    /// </summary>
    [Required(ErrorMessage = "玩家Id不能为空")]
    [MaxLength(EntityStringLength.Length256)]
    public string PlayerId { get; set; } = null!;

    /// <summary>
    /// 被撤销的交易 Id（必填；从交易详情页发起撤销时携带）。
    /// </summary>
    [Required(ErrorMessage = "交易Id不能为空")]
    [MaxLength(EntityStringLength.Length256)]
    public string TransactionId { get; set; } = null!;

    /// <summary>
    /// 撤销原因（必填，进审计）。
    /// </summary>
    [Required(ErrorMessage = "撤销原因不能为空")]
    [MaxLength(EntityStringLength.Length512)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 业务单号（可选；语义同发放——提供时为幂等键，未提供时服务端生成）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? BusinessOrderNumber { get; set; }
}
