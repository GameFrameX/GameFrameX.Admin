namespace GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto;

/// <summary>
/// 租户微信公众号 / 小程序配置新增输入参数。
/// <para>公众号凭证对（<see cref="WechatAppId"/> + <see cref="WechatAppSecret"/>）与小程序凭证对（<see cref="WxOpenAppId"/> + <see cref="WxOpenSecret"/>）至少配置一组，且每组必须成对完整。</para>
/// </summary>
public class AddTenantWechatConfigRequest : BaseAddRequest
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
    /// 微信公众号 AppSecret（敏感字段，入库明文，API 输出脱敏）。
    /// </summary>
    public string WechatAppSecret { get; set; } = null!;

    /// <summary>
    /// 微信小程序 AppId。
    /// </summary>
    public string WxOpenAppId { get; set; } = null!;

    /// <summary>
    /// 微信小程序 AppSecret（敏感字段，入库明文，API 输出脱敏）。
    /// </summary>
    public string WxOpenSecret { get; set; } = null!;

    /// <summary>
    /// 配置选择优先级（数值越大越优先）。仅用于配置选择。
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否启用。禁用配置不参与租户 Wechat 选择和测试发送。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 备注。
    /// </summary>
    public string Remark { get; set; } = null!;
}