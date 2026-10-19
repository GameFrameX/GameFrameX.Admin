using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.ClientApiConfig;

/// <summary>
/// 租户微信公众号 / 小程序配置。
/// <para>承载租户自有公众号 / 小程序凭证，用于租户业务模板消息、运营触达等认证后场景；平台不持有 Wechat 配置。</para>
/// <para>注意：公众号/小程序网页授权登录已从 Admin 登录移除（认证前无租户上下文），本配置仅服务租户上下文内的运营方法。</para>
/// <para>作用域：<see cref="EntityTenantBase.TenantId"/> + <see cref="AppId"/>，AppId=0 表示租户默认配置。</para>
/// <para>敏感字段（<see cref="WechatAppSecret"/> / <see cref="WxOpenSecret"/>）入库明文，API 输出脱敏 <c>******</c>，更新时回传 <c>******</c> 保留原值。</para>
/// </summary>
[SugarTable(null, "租户微信公众号/小程序配置")]
[TenantTable]
[IncrementTable]
public class TenantWechatConfigEntity : EntityBasic, IAppScoped, ITenantAppConfigScope
{
    /// <summary>
    /// 应用 ID。0 表示租户默认公众号/小程序配置；大于 0 表示应用级覆盖。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用ID", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 微信公众号 AppId。
    /// </summary>
    [SugarColumn(ColumnDescription = "微信公众号AppId", Length = EntityStringLength.Length256)]
    public string WechatAppId { get; set; } = string.Empty;

    /// <summary>
    /// 微信公众号 AppSecret（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    [SugarColumn(ColumnDescription = "微信公众号AppSecret", Length = EntityStringLength.Length256)]
    public string WechatAppSecret { get; set; } = string.Empty;

    /// <summary>
    /// 微信小程序 AppId。
    /// </summary>
    [SugarColumn(ColumnDescription = "微信小程序AppId", Length = EntityStringLength.Length256)]
    public string WxOpenAppId { get; set; } = string.Empty;

    /// <summary>
    /// 微信小程序 AppSecret（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    [SugarColumn(ColumnDescription = "微信小程序AppSecret", Length = EntityStringLength.Length256)]
    public string WxOpenSecret { get; set; } = string.Empty;

    /// <summary>
    /// 配置选择优先级。同一匹配层中按此值降序选择；数值越大越优先。
    /// </summary>
    [SugarColumn(ColumnDescription = "配置选择优先级", DefaultValue = "0")]
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 备注。
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Remark { get; set; } = null!;
}