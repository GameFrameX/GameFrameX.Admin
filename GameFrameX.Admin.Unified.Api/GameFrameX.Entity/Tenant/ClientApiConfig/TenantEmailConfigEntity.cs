using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.ClientApiConfig;

/// <summary>
/// 租户业务邮件配置。
/// <para>承载玩家注册验证码、找回密码、玩家运营触达等租户品牌发信；与平台系统邮件（<c>SystemEmailProviderEntity</c>）完全独立，互不 fallback。</para>
/// <para>作用域：<see cref="EntityTenantBase.TenantId"/> + <see cref="AppId"/>，AppId=0 表示租户默认配置；<see cref="Purpose"/>=0 表示通配租户业务邮件。</para>
/// </summary>
[SugarTable(null, "租户业务邮件配置")]
[TenantTable]
[IncrementTable]
public class TenantEmailConfigEntity : EntityBasic, IAppScoped, ITenantAppConfigScope
{
    /// <summary>
    /// 应用 ID。0 表示租户默认邮件配置；大于 0 表示应用级覆盖。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用ID", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 租户业务邮件用途（<c>TenantEmailPurpose</c>）。0 表示通配该租户 / 应用下所有租户业务邮件。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户业务邮件用途", DefaultValue = "0")]
    public int Purpose { get; set; }

    /// <summary>
    /// Provider 类型。当前仅支持 <c>TencentCloudSes</c>（沿用现有 <c>EmailHelper</c>）；SMTP 是否支持取决于 execute 阶段复用成本，不引入新依赖。
    /// </summary>
    [SugarColumn(ColumnDescription = "Provider类型", Length = EntityStringLength.Length256)]
    public string Provider { get; set; } = "TencentCloudSes";

    /// <summary>
    /// Provider 区域（如腾讯云 SES 区域 <c>ap-hongkong</c>）。
    /// </summary>
    [SugarColumn(ColumnDescription = "Provider区域", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? Region { get; set; }

    /// <summary>
    /// 腾讯云邮件模板组 / 模板集标识；用于按业务组维护模板。
    /// </summary>
    [SugarColumn(ColumnDescription = "模板组", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? TemplateSet { get; set; }

    /// <summary>
    /// 发件人邮箱地址。
    /// </summary>
    [SugarColumn(ColumnDescription = "发件人邮箱地址", Length = EntityStringLength.Length512)]
    public string FromEmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// 默认发件人显示名称。
    /// </summary>
    [SugarColumn(ColumnDescription = "默认发件人显示名", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? DefaultFromName { get; set; }

    /// <summary>
    /// 默认邮件主题前缀（例如 <c>[GameName]</c>）。
    /// </summary>
    [SugarColumn(ColumnDescription = "默认邮件主题前缀", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? DefaultSubjectPrefix { get; set; }

    /// <summary>
    /// 邮件服务商 SecretId（敏感字段，API 输出需脱敏，日志禁止明文）。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件服务商SecretId", Length = EntityStringLength.Length512)]
    public string FromEmailSecretId { get; set; } = string.Empty;

    /// <summary>
    /// 邮件服务商 SecretKey（敏感字段，API 输出需脱敏，日志禁止明文）。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件服务商SecretKey", Length = EntityStringLength.Length512)]
    public string FromEmailSecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 配置选择优先级。同一匹配层（<c>TenantId+AppId+Purpose</c> 等）中按此值降序选择；数值越大越优先。
    /// <para>本字段仅用于配置选择，不代表邮件投递队列优先级。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "配置选择优先级", DefaultValue = "0")]
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 备注。
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Remark { get; set; } = null!;
}