namespace GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto;

/// <summary>
/// 租户微信公众号 / 小程序配置更新输入参数。
/// <para>敏感字段（<see cref="WechatAppSecret"/> / <see cref="WxOpenSecret"/>）提交 <c>******</c> 时表示保留原值，避免脱敏回写覆盖真实密钥。</para>
/// </summary>
public class UpdateTenantWechatConfigRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用 ID。0 表示租户默认配置；大于 0 表示应用级覆盖。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 微信公众号 AppId。
    /// </summary>
    public string WechatAppId { get; set; } = null!;

    /// <summary>
    /// 微信公众号 AppSecret。传 <c>******</c> 时保留原值。
    /// </summary>
    public string WechatAppSecret { get; set; } = null!;

    /// <summary>
    /// 微信小程序 AppId。
    /// </summary>
    public string WxOpenAppId { get; set; } = null!;

    /// <summary>
    /// 微信小程序 AppSecret。传 <c>******</c> 时保留原值。
    /// </summary>
    public string WxOpenSecret { get; set; } = null!;

    /// <summary>
    /// 配置选择优先级（数值越大越优先）。
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 备注。
    /// </summary>
    public string Remark { get; set; } = null!;
}