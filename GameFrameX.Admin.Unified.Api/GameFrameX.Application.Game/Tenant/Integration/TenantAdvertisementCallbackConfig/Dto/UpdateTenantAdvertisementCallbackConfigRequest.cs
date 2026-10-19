namespace GameFrameX.Application.Game.Tenant.Integration.TenantAdvertisementCallbackConfig.Dto;

/// <summary>
/// 租户广告回调配置更新输入参数。
/// <para>敏感字段提交 <c>******</c> 时表示保留原值。</para>
/// </summary>
public class UpdateTenantAdvertisementCallbackConfigRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用 ID。0 表示租户默认配置；大于 0 表示应用级覆盖。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 广告平台 Provider。当前首批配置支持 <c>TopOn</c>、<c>GoogleAdMob</c>、<c>Pangle</c>、<c>UnityAds</c>。
    /// </summary>
    [Required(ErrorMessage = "广告平台不能为空")]
    public string Provider { get; set; } = null!;

    /// <summary>
    /// 广告平台后台配置的回调 URL。
    /// </summary>
    public string CallbackUrl { get; set; } = null!;

    /// <summary>
    /// 签名密钥或共享密钥。传 <c>******</c> 时保留原值。
    /// </summary>
    public string SigningSecret { get; set; } = null!;

    /// <summary>
    /// 验签公钥或验证材料。传 <c>******</c> 时保留原值。
    /// </summary>
    public string VerificationKeys { get; set; } = null!;

    /// <summary>
    /// 广告回调动态配置 JSON。字段由 Provider schema 决定；敏感字段传 <c>******</c> 时保留原值。
    /// </summary>
    [Required(ErrorMessage = "广告回调配置不能为空")]
    public string ConfigJson { get; set; } = null!;

    /// <summary>
    /// 参数作用说明 JSON。
    /// </summary>
    public string ParameterPurposeJson { get; set; } = null!;

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 备注。
    /// </summary>
    public string Remark { get; set; } = null!;
}