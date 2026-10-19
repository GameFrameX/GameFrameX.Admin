namespace GameFrameX.Application.Game.Tenant.Integration.TenantObjectStorageConfig;

/// <summary>
/// 租户对象存储配置分页查询入参。
/// </summary>
public class PageTenantObjectStorageConfigRequest : BasePageRequest
{
    /// <summary>按提供商过滤。</summary>
    public string? Provider { get; set; }

    /// <summary>按启用状态过滤。</summary>
    public bool? IsEnabled { get; set; }
}