using GameFrameX.Entity.Base;
using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.System;

/// <summary>
/// 平台系统邮件 Provider 配置。
/// <para>存储边界：主库 / SuperAdmin-only；与租户业务邮件（<c>TenantEmailConfigEntity</c>）完全独立，互不 fallback。</para>
/// <para>典型用途：租户生命周期、账单、平台安全、平台运维、异常事件通知；调用方不依赖当前请求租户上下文。</para>
/// <para>同一 <see>
///     <cref>SystemEmailPurpose</cref>
/// </see>
/// 下允许多条启用配置；选择策略见 <c>SystemEmailProviderSelector</c>。</para>
/// </summary>
[SugarTable(null, "平台系统邮件Provider配置")]
[PlatformTable]
[IncrementTable]
public class SystemEmailProviderEntity : EntityBasic
{
    /// <summary>
    /// 平台系统邮件用途（<see cref="Core.Service.Message.SystemEmailPurpose"/>）。0 表示默认配置。
    /// </summary>
    [SugarColumn(ColumnDescription = "平台系统邮件用途", IndexGroupNameList = new[] { "idx_purpose", }, DefaultValue = "0")]
    public int Purpose { get; set; }

    /// <summary>
    /// Provider 类型。当前支持 <c>TencentCloudSes</c>（腾讯云 SES）和 <c>Smtp</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "Provider类型", Length = EntityStringLength.Length256)]
    public string Provider { get; set; } = "TencentCloudSes";

    /// <summary>
    /// SMTP 主机。Provider=Smtp 时必填。
    /// </summary>
    [SugarColumn(ColumnDescription = "SMTP主机", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? Host { get; set; }

    /// <summary>
    /// SMTP 端口。Provider=Smtp 时必填。
    /// </summary>
    [SugarColumn(ColumnDescription = "SMTP端口", IsNullable = true)]
    public int? Port { get; set; }

    /// <summary>
    /// SMTP 启用 SSL。Provider=Smtp 时使用。
    /// </summary>
    [SugarColumn(ColumnDescription = "SMTP启用SSL", IsNullable = true)]
    public bool? EnableSsl { get; set; }

    /// <summary>
    /// SMTP 使用默认凭据。Provider=Smtp 时使用。
    /// </summary>
    [SugarColumn(ColumnDescription = "SMTP使用默认凭据", IsNullable = true)]
    public bool? UseDefaultCredentials { get; set; }

    /// <summary>
    /// SMTP 用户名（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    [SugarColumn(ColumnDescription = "SMTP用户名", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? UserName { get; set; }

    /// <summary>
    /// SMTP 密码（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    [SugarColumn(ColumnDescription = "SMTP密码", Length = EntityStringLength.Length512, IsNullable = true)]
    public string? Password { get; set; }

    /// <summary>
    /// Provider 区域（如腾讯云 SES 区域 <c>ap-hongkong</c>）。
    /// </summary>
    [SugarColumn(ColumnDescription = "Provider区域", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? Region { get; set; }

    /// <summary>
    /// 腾讯云 SES SecretId（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    [SugarColumn(ColumnDescription = "腾讯云SecretId", Length = EntityStringLength.Length512, IsNullable = true)]
    public string? SecretId { get; set; }

    /// <summary>
    /// 腾讯云 SES SecretKey（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    [SugarColumn(ColumnDescription = "腾讯云SecretKey", Length = EntityStringLength.Length512, IsNullable = true)]
    public string? SecretKey { get; set; }

    /// <summary>
    /// 平台默认发件人邮箱地址。
    /// </summary>
    [SugarColumn(ColumnDescription = "默认发件人邮箱", Length = EntityStringLength.Length512, IsNullable = true)]
    public string? DefaultFromEmail { get; set; }

    /// <summary>
    /// 平台默认发件人显示名称。
    /// </summary>
    [SugarColumn(ColumnDescription = "默认发件人显示名", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? DefaultFromName { get; set; }

    /// <summary>
    /// 平台默认收件人邮箱；缺省时使用业务调用方传入的 receiver。
    /// </summary>
    [SugarColumn(ColumnDescription = "默认收件人邮箱", Length = EntityStringLength.Length512, IsNullable = true)]
    public string? DefaultToEmail { get; set; }

    /// <summary>
    /// 腾讯云 SES 模板 ID。Provider=<c>TencentCloudSes</c> 时必填；SMTP 发送不使用。
    /// </summary>
    [SugarColumn(ColumnDescription = "腾讯云模板ID", IsNullable = true)]
    public ulong? TemplateId { get; set; }

    /// <summary>
    /// 腾讯云 SES 模板变量 JSON。Provider=<c>TencentCloudSes</c> 时写入 TemplateData；SMTP 发送不使用。
    /// </summary>
    [SugarColumn(ColumnDescription = "腾讯云模板变量JSON", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? TemplateDataJson { get; set; }

    /// <summary>
    /// 配置选择优先级。同一 <see cref="Purpose"/> 匹配层中按此值降序选择；数值越大越优先。
    /// <para>本字段仅用于配置选择，不代表邮件投递队列优先级；当前 change 不承诺投递顺序变化。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "配置选择优先级", DefaultValue = "0")]
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 备注。
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string? Remark { get; set; }
}