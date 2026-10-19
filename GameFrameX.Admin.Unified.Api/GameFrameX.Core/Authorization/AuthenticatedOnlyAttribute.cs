namespace GameFrameX.Core.Authorization;

/// <summary>
/// 「仅需登录」档权限语义的标记特性：标注端点只要持有有效 JWT 即可访问，不按按钮权限授权。
/// <para>三档权限语义均在方法定义处以特性声明、启动时自动登记：匿名档 <c>[AllowAnonymous]</c>、
/// 仅登录档 <c>[AuthenticatedOnly]</c>、逃生通道档 <c>[BypassTenantExpiration]</c>。
/// 方法删除或改名时登记随之自动消失/更新，不再依赖集中手写路由清单（GFX-684 / C277，
/// 取代 ApiEndpointCatalogSync.AuthenticatedOnlyRouteNames 手写数组 + global_authenticated_only_route 中转表）。</para>
/// <para>成员必须是「登录即可访问」的登录态基础数据端点：不按角色授权、不暴露越租户数据；
/// 标注后 <c>ApiEndpointCatalogSync</c> 会把活端点登记为 <c>IsAuthenticatedOnly=true</c>，
/// 避免被自动隐藏按钮挡成 403（自动按钮默认不授权任何租户角色）。</para>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class AuthenticatedOnlyAttribute : Attribute, IFilterMetadata
{
}