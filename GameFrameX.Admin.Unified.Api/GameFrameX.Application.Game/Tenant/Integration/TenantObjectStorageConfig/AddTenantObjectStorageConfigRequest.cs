namespace GameFrameX.Application.Game.Tenant.Integration.TenantObjectStorageConfig;

/// <summary>
/// 新增租户对象存储配置入参。
/// </summary>
public class AddTenantObjectStorageConfigRequest : BaseAddRequest
{
    /// <summary>存储提供商名称（local / aliyun / tencent-cos / qiniu）。</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>对象键路径模板。</summary>
    public string PathTemplate { get; set; } = "Upload/{yyyy}/{MM}/{dd}";

    /// <summary>单文件最大字节数。</summary>
    public long MaxFileSizeBytes { get; set; } = 20971520;

    /// <summary>允许的内容类型 JSON 数组。</summary>
    public string AllowedContentTypesJson { get; set; } = string.Empty;

    /// <summary>是否启用 MD5 去重。</summary>
    public bool EnableMd5 { get; set; } = true;

    /// <summary>提供商配置 JSON，敏感字段可用 ****** 表示沿用旧值（更新场景）。</summary>
    public string ConfigJson { get; set; } = string.Empty;

    /// <summary>是否启用。</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注。</summary>
    public string? Remark { get; set; }
}