namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// Online 资产人工发放输入参数（高危；vault:C4「Admin 人工发放」六类来源之一）。
/// <para>经 Online Grant API 执行，Admin 侧零直写；<c>Reason</c> 必填进审计；幂等键（业务单号）规则见命令服务。</para>
/// </summary>
public class OnlineAssetGrantRequest
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
    /// 发放明细（至少一条）。
    /// </summary>
    [Required(ErrorMessage = "发放明细不能为空")]
    public List<OnlineAssetGrantItemRequest> GrantItems { get; set; } = new();

    /// <summary>
    /// 发放原因（必填，进审计；对应前端弹层理由输入）。
    /// </summary>
    [Required(ErrorMessage = "发放原因不能为空")]
    [MaxLength(EntityStringLength.Length512)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 业务单号（可选；提供时以其为幂等键——同单号重试返回 Online 首次结果；未提供时服务端生成新单号，每次调用都是新业务意图）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? BusinessOrderNumber { get; set; }
}
