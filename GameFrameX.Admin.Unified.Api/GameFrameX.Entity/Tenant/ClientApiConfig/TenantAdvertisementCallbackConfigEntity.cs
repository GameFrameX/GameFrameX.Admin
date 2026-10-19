using GameFrameX.Entity.Base;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Tenant.ClientApiConfig;

/// <summary>
/// 租户广告回调配置。
/// <para>承载广告服务端回调用于验签和参数识别的最小配置材料，Client API 公开回调入口按 Provider 读取该配置。</para>
/// <para>作用域：<see cref="EntityTenantBase.TenantId"/> + <see cref="AppId"/>。</para>
/// </summary>
[SugarTable(null, "租户广告回调配置")]
[TenantTable]
[IncrementTable]
public class TenantAdvertisementCallbackConfigEntity : EntityBasic, IAppScoped, ITenantAppConfigScope
{
    /// <summary>
    /// 应用 ID。0 表示租户默认配置；大于 0 表示应用级覆盖。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用ID", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 广告平台 Provider。当前首批配置支持 <c>TopOn</c>、<c>GoogleAdMob</c>、<c>Pangle</c>、<c>UnityAds</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "广告平台Provider", Length = EntityStringLength.Length256)]
    public string Provider { get; set; } = "TopOn";

    /// <summary>
    /// 广告平台后台配置的回调 URL。
    /// </summary>
    [SugarColumn(ColumnDescription = "回调URL", Length = EntityStringLength.Length2048)]
    public string CallbackUrl { get; set; } = string.Empty;

    /// <summary>
    /// 签名密钥或共享密钥。TopOn 对应 <c>sec_key</c>，Pangle 对应 security key，Unity Ads 对应 Support 下发的 secret。
    /// <para>敏感字段，API 输出需脱敏，日志禁止明文。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "签名密钥", Length = EntityStringLength.Length1024)]
    public string SigningSecret { get; set; } = string.Empty;

    /// <summary>
    /// 验签公钥或验证材料。Google AdMob 可记录 <c>key_id=base64key</c> 形式的验证密钥集合。
    /// <para>敏感字段，API 输出需脱敏，日志禁止明文。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "验证密钥集合", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string VerificationKeys { get; set; } = string.Empty;

    /// <summary>
    /// 广告回调动态配置 JSON。字段由 Provider schema 决定，类似支付配置的 ConfigJson。
    /// </summary>
    [SugarColumn(ColumnDescription = "广告回调配置JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string ConfigJson { get; set; } = string.Empty;

    /// <summary>
    /// 参数作用说明 JSON。记录该 Provider 的核心参数如何映射到用户、交易、奖励、签名、审计等用途。
    /// <para>保留为兼容摘要；新的动态页面以 Provider schema 的字段 help 作为主要来源。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "参数作用说明", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string ParameterPurposeJson { get; set; } = string.Empty;

    /// <summary>
    /// 备注。
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Remark { get; set; } = null!;
}