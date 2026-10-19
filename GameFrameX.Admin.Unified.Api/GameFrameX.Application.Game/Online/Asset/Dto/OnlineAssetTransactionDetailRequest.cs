namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// 资产交易详情查询输入参数（GET 复杂查询对象，显式 <c>[FromQuery]</c> 绑定，C257 §4）。
/// <para>作用域必填：详情查询同样经 OnlineScopeContextFactory 校验后路由到目标区服的 Online 管理 API。</para>
/// </summary>
public class OnlineAssetTransactionDetailRequest
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
    /// 交易 Id（必填；流水行反查交易详情的唯一键）。
    /// </summary>
    [Required(ErrorMessage = "交易Id不能为空")]
    [MaxLength(EntityStringLength.Length256)]
    public string TransactionId { get; set; } = null!;
}
