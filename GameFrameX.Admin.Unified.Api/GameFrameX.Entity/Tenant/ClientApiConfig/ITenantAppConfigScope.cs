namespace GameFrameX.Entity.Tenant.ClientApiConfig;

/// <summary>
/// 标记按 <c>TenantId + AppId</c> 作用域的租户配置实体，供 Provider 的泛型作用域选择使用。
/// <para>TenantId / IsEnabled / Id 均继承自框架基类，本接口只显式暴露 AppId。</para>
/// </summary>
public interface ITenantAppConfigScope
{
    /// <summary>
    /// 应用 ID。0 表示租户默认配置。
    /// </summary>
    long AppId { get; set; }
}