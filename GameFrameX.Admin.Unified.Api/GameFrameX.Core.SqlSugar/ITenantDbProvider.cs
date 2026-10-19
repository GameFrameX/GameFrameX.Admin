using SqlSugar;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// 租户库连接提供器抽象：当前请求 TenantId -&gt; 租户业务库连接（C8 T4a 中间件共享层收敛引入）。
/// <para>各 host 自行注册实现：Client.Api / Game.Api 为同构的连接缓存提供器（ClientApiTenantDbProvider / GameApiTenantDbProvider，
/// 返回 AddConnection 后的 SqlSugarScopeProvider）；Hub.Api 为实时查库提供器（HubTenantDbProvider，
/// 每次解析创建独立租户库连接）。共享层密钥环 Resolver（SignatureKeyResolver / SessionKeyResolver）与统一仓储
/// Repository&lt;T&gt; 只依赖本接口，不感知具体 host 的提供器类型与连接策略。</para>
/// </summary>
public interface ITenantDbProvider
{
    /// <summary>
    /// 取指定租户的租户库连接。连接缓存型实现返回 SqlSugarScopeProvider，实时查库型实现返回独立 client。
    /// </summary>
    ISqlSugarClient GetTenantDb(long tenantId);
}