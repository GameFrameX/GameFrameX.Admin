namespace GameFrameX.Core.Filter;

/// <summary>
/// 豁免租户到期/禁用门禁（<see cref="TenantExpirationFilter"/>）的标记特性。
/// <para>逃生通道端点专用：登录、登出、刷新 Token、验证码、租户到期状态查询——即使租户被禁用/过期也必须可用，
/// 否则该租户已登录用户将无法登出、无法获取到期提示，被锁死在系统内。</para>
/// <para>以特性绑定替代早期按路径前缀匹配的白名单：Admin 路径由动态路由推导（如 <c>SystemAuthService</c> → <c>/api/systemAuth</c>），
/// 硬编码路径会随路由调整漂移失效（原白名单 5 条全部与真实路由不符，即为此故）。</para>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class BypassTenantExpirationAttribute : Attribute, IFilterMetadata
{
}