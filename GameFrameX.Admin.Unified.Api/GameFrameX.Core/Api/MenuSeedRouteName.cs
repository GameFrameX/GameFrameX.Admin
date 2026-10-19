using System.Reflection;
using GameFrameX.Core.Api;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GameFrameX.Core.Api;

/// <summary>
/// 菜单种子按钮 Permission（routeName，<c>controller:action</c> 形态）的类型安全拼接入口（C368）。
/// <para>
/// 种子写法：<c>Permission = MenuSeedRouteName.Of&lt;TenantOrganizationService&gt;(nameof(TenantOrganizationService.DeleteAsync))</c>。
/// 控制器段与动作段均按 <see cref="AdminApiRouteNaming"/> 唯一事实源推导——服务改名/删除后种子随编译自动跟随或报错，
/// 不再出现手写字符串停留在旧路由名的漂移。仅推导 Permission 绑定链；按钮 Key 是前端权限码契约
/// （StableSeedId 由 Key 派生 Id，牵动 role-menu），必须保持字面量。
/// </para>
/// </summary>
public static class MenuSeedRouteName
{
    /// <summary>
    /// 由服务类型与方法名推导 routeName（<c>tenantOrganization:delete</c> 形态）。
    /// </summary>
    /// <typeparam name="TService">Admin 动态 API 服务类型（如 <c>TenantOrganizationService</c>）。</typeparam>
    /// <param name="methodName">方法名（建议 <c>nameof(TService.XxxAsync)</c> 传入，保编译期存在性检查）。</param>
    public static string Of<TService>(string methodName) where TService : class
    {
        var methods = typeof(TService).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                      .Where(u => u.Name == methodName)
                                      .ToArray();
        if (methods.Length == 0)
        {
            throw new ArgumentException(
                $"MenuSeedRouteName：{typeof(TService).Name} 上不存在公共实例方法 {methodName}，种子 Permission 推导失败。", nameof(methodName));
        }

        // 同名重载可能挂不同路由（如基类 CRUD update 与派生回退 return）；路由集合不唯一即拒绝推导，
        // 避免静默取首个命中导致 Permission 指向错误端点。消歧手段：按语义改名派生方法（C368 先例）。
        var routePaths = methods.Select(u => AdminApiRouteNaming.BuildRoutePath(typeof(TService), u))
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToArray();
        if (routePaths.Length > 1)
        {
            throw new InvalidOperationException(
                $"MenuSeedRouteName：{typeof(TService).Name}.{methodName} 存在 {routePaths.Length} 个不同路由的重载（{string.Join("、", routePaths)}），无法按方法名消歧，请重命名方法或拆分种子按钮。");
        }

        var routePath = routePaths[0];

        // routePath 形如 api/tenantOrganization/delete → routeName（tenantOrganization:delete）。
        // 多段模板（如 dbtypelist/{configid}）与无特性 GET 动作参数段（如 ownRoleList/{userId}）
        // 统一转冒号形态，与存量字面量及 RouteNameToRoutePath 反向变换兼容。
        const string apiPrefix = "api/";
        if (!routePath.StartsWith(apiPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"MenuSeedRouteName：{typeof(TService).Name}.{methodName} 推导路由 {routePath} 不含 api/ 前缀，无法生成 Permission。");
        }

        return routePath[apiPrefix.Length..].Replace("/", ":");
    }
}
