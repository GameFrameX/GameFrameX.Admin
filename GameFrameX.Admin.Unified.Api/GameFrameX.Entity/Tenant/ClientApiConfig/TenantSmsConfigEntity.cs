using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.ClientApiConfig;

/// <summary>
/// 租户 SMS 短信配置。
/// <para>承载租户自有阿里云短信凭证（AccessKey/SignName/TemplateCode），用于租户业务短信下发；平台不持有 SMS 配置。</para>
/// <para>作用域：<see cref="EntityTenantBase.TenantId"/> + <see cref="AppId"/>，AppId=0 表示租户默认配置。</para>
/// <para>敏感字段（<see cref="AccessKeyId"/> / <see cref="AccessKeySecret"/>）入库明文，API 输出脱敏 <c>******</c>，更新时回传 <c>******</c> 保留原值。</para>
/// </summary>
[SugarTable(null, "租户SMS短信配置")]
[TenantTable]
[IncrementTable]
public class TenantSmsConfigEntity : EntityBasic, IAppScoped, ITenantAppConfigScope
{
    /// <summary>
    /// 应用 ID。0 表示租户默认 SMS 配置；大于 0 表示应用级覆盖。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用ID", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// Provider 类型。当前仅支持 <c>Aliyun</c>（阿里云短信）。
    /// </summary>
    [SugarColumn(ColumnDescription = "Provider类型", Length = EntityStringLength.Length256)]
    public string Provider { get; set; } = "Aliyun";

    /// <summary>
    /// 阿里云 AccessKeyId（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    [SugarColumn(ColumnDescription = "阿里云AccessKeyId", Length = EntityStringLength.Length256)]
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// 阿里云 AccessKeySecret（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    [SugarColumn(ColumnDescription = "阿里云AccessKeySecret", Length = EntityStringLength.Length512)]
    public string AccessKeySecret { get; set; } = string.Empty;

    /// <summary>
    /// 短信签名（如 <c>AdminNET平台</c>）。
    /// </summary>
    [SugarColumn(ColumnDescription = "短信签名", Length = EntityStringLength.Length256)]
    public string SignName { get; set; } = string.Empty;

    /// <summary>
    /// 短信模板 Code（阿里云 SMS_ 开头）。可为空（测试发送时由调用方指定）。
    /// </summary>
    [SugarColumn(ColumnDescription = "短信模板Code", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? TemplateCode { get; set; }

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