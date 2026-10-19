using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.ObjectStorage;

/// <summary>
/// 租户对象存储配置。每个租户维护若干存储提供商配置，其中至多一条处于启用状态。
/// 当前定位为租户默认资源能力（租户级共享），不补 AppId；若未来不同应用必须隔离桶 / 目录 / 凭证 / CDN 域名，
/// 另立 change 评估升级为应用覆盖（仿 <c>ITenantAppConfigScope</c> 范式），不在当前作用域。
/// </summary>
[SugarTable(null, "租户对象存储配置")]
[TenantTable]
[IncrementTable]
public class TenantObjectStorageConfigEntity : EntityBasic
{
    /// <summary>
    /// 存储提供商名称（稳定小写值：local / aliyun / tencent-cos / qiniu）。创建后不可变更。
    /// </summary>
    [SugarColumn(ColumnDescription = "存储提供商", Length = EntityStringLength.Length256)]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// 对象键路径模板，支持 {yyyy}/{MM}/{dd} 占位符。
    /// </summary>
    [SugarColumn(ColumnDescription = "对象键路径模板", Length = EntityStringLength.Length256)]
    public string PathTemplate { get; set; } = "Upload/{yyyy}/{MM}/{dd}";

    /// <summary>
    /// 单文件最大字节数，默认 20MB。
    /// </summary>
    [SugarColumn(ColumnDescription = "单文件最大字节数", DefaultValue = "0")]
    public long MaxFileSizeBytes { get; set; } = 20971520;

    /// <summary>
    /// 允许的内容类型 JSON 数组（空表示不限制）。
    /// </summary>
    [SugarColumn(ColumnDescription = "允许的内容类型JSON数组", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string AllowedContentTypesJson { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用 MD5 去重。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否启用MD5去重", DefaultValue = "false")]
    public bool EnableMd5 { get; set; } = true;

    /// <summary>
    /// 提供商配置 JSON（凭证等），字段集合由对应 Provider 的 Schema 决定。
    /// </summary>
    [SugarColumn(ColumnDescription = "提供商配置JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string ConfigJson { get; set; } = string.Empty;

    /// <summary>
    /// 备注。
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length512, IsNullable = true)]
    public string? Remark { get; set; }
}