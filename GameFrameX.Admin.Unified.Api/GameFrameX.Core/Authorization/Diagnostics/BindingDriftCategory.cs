using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Authorization.Diagnostics;

/// <summary>
/// 菜单权限/endpoint 绑定漂移类别（C128 稳定诊断契约）。
/// <para>枚举值即契约：未来对 ApiEndpointCatalogSync / JwtHandler 授权链路的改动一旦引入下列任一漂移，
/// 回归测试必须失败。新增类别只能追加到末尾，不得重排或重命名既有成员。</para>
/// </summary>
[AdminEnum]
public enum BindingDriftCategory
{
    /// <summary>
    /// 活端点（ASP.NET Core MVC 发现）未在 <c>global_api_endpoint</c> 登记。
    /// JwtHandler 未知端点 fail-closed，风险=功能静默不可达。severity=Warning。
    /// </summary>
    LiveEndpointMissingFromDb = 1,

    /// <summary>
    /// DB 行 <c>IsOrphan=true</c> 但 <c>IsEnabled=true</c>，与对账后应自动禁用矛盾。severity=Error。
    /// </summary>
    DbOrphanStillEnabled = 2,

    /// <summary>
    /// <c>IsPublic=false &amp;&amp; IsAuthenticatedOnly=false &amp;&amp; IsEnabled=true</c> 且无任何有效绑定。
    /// JwtHandler 空绑定 fail-closed，任何非超管请求必 403。severity=Error。
    /// </summary>
    NonPublicEndpointWithoutBinding = 3,

    /// <summary>
    /// 绑定 <c>MenuId</c> 指向不存在或软删菜单。severity=Error。
    /// </summary>
    BindingToMissingMenu = 4,

    /// <summary>
    /// 绑定 <c>EndpointId</c> 指向不存在或禁用端点。severity=Error。
    /// </summary>
    BindingToMissingEndpoint = 5,

    /// <summary>
    /// 按钮 <c>Type=Button</c> 且 <c>Permission</c> 非空，但 routeName 经
    /// <see cref="ApiEndpointIdentity.RouteNameToRoutePath"/> 映射后命中零活端点（孤立按钮）。severity=Warning。
    /// </summary>
    MenuButtonPermissionOrphan = 6,

    /// <summary>
    /// 同 <c>(HttpMethod, RoutePath)</c> 被多个活方法占用，采集仅保留首个。severity=Warning。
    /// <para>（历史注记：成员 7 <c>AuthenticatedOnlyRouteMissingFromLive</c> 已随登录态白名单特性化退役删除——
    /// <c>[AuthenticatedOnly]</c> 特性长在方法上，方法删除则登记自动消失，该漂移结构性不可再发生，GFX-684/C277。
    /// 显式数值不复用，已删除成员的值永久空缺。）</para>
    /// </summary>
    DuplicateEndpointKey = 8,
}