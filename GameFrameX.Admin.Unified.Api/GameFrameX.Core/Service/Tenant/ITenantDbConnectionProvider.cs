namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 租户数据库连接提供器接口（C217 可测性重构：SystemAuthService 注入此接口，便于测试 fake）。
/// </summary>
public interface ITenantDbConnectionProvider
{
    /// <summary>
    /// 按租户 ID 获取租户库连接作用域；租户未注册返回 null。
    /// </summary>
    SqlSugarScopeProvider GetTenantDbConnectionScope(long tenantId);
}