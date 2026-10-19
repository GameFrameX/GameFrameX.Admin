namespace GameFrameX.Application.Game.Tenant.Integration.TenantObjectStorageConfig;

/// <summary>
/// 租户对象存储配置列表项。
/// </summary>
public class TenantObjectStorageConfigResponse
{
    public long Id { get; set; }

    /// <summary>存储提供商名称。</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>对象键路径模板。</summary>
    public string PathTemplate { get; set; } = string.Empty;

    /// <summary>单文件最大字节数。</summary>
    public long MaxFileSizeBytes { get; set; }

    /// <summary>允许的内容类型 JSON 数组。</summary>
    public string AllowedContentTypesJson { get; set; } = string.Empty;

    /// <summary>是否启用 MD5 去重。</summary>
    public bool EnableMd5 { get; set; }

    /// <summary>提供商配置 JSON（列表场景整体脱敏为 ******）。</summary>
    public string ConfigJson { get; set; } = string.Empty;

    /// <summary>是否启用。</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注。</summary>
    public string? Remark { get; set; }

    /// <summary>创建时间。</summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 行版本（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}