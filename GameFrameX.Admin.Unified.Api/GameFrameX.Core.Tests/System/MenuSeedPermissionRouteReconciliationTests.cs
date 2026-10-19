using System.Reflection;
using GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty;
using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Api;
using GameFrameX.Core.Authorization;
using GameFrameX.Core.Service.Organization;
using GameFrameX.Core.Service.TenantPackage;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// 种子按钮 Permission 与活端点目录对账守护（C368）。
/// <para>
/// Permission 现由 <see cref="MenuSeedRouteName.Of{TService}(string)"/> 按路由命名规则唯一事实源推导，
/// 本测试以反射重建全量活端点路由（与 ApiEndpointCatalogSync 同口径），对 BuildData() 全部按钮 Permission
/// 逐条对账：未命中即种子/端点漂移，红灯并给出可定位清单——GFX-710 式服务改名、端点下线、
/// ApiDescriptionSettings(Name) 改动都逃不过本测试。
/// </para>
/// </summary>
[Collection("MenuSeedCatalog")]
public class MenuSeedPermissionRouteReconciliationTests
{
    /// <summary>
    /// 豁免清单：端点已下线或动作语义被替代的历史按钮，保留字面量（前端 v-auth 仍按 Key 消费）。
    /// 新增条目必须附原因；端点恢复后条目会因「死条目」断言被清出。
    /// </summary>
    private static readonly string[] LegacyOrphanAllowlist =
    {
        "tenantGameAppAssetPackageVersion:history", // 版本历史改由 tenantGameAppAssetPackageVersionHistory 域承载
        "tenantGameAppAssetPackageVersion:restore", // 动作语义变更为 return
        "tenantGameAppEntity:package", // 分包管理独立成 gameAppPackage 页
        "tenantGamePaymentConfigEntity:copyNotifyUrl", // 端点下线，前端改从详情复制
        "tenantGameLoginTicketKeyEntity:update", // 动作被 rotate 取代
        "tenantAppSignatureKeyEntity:update", // 动作被 renew/rotate 取代
        "tenantSessionKeyEntity:update", // 动作被 enable/disable 体系取代
        "tenantUser:detail", // 端点下线
        "tenantNotice:cancel", // 端点下线（publish/setRead 体系）
    };

    [Fact]
    public void EverySeedPermissionResolvesToLiveEndpointOrAllowlisted()
    {
        var liveRoutes = BuildLiveRoutes();

        var permissions = GlobalMenuSeedData.BuildData()
                                   .Where(u => u.Type == MenuTypeEnum.Button && !string.IsNullOrWhiteSpace(u.Permission))
                                   .Select(u => u.Permission!)
                                   .Distinct()
                                   .ToList();

        var missing = permissions
                      .Where(p => !LegacyOrphanAllowlist.Contains(p))
                      .Where(p => !liveRoutes.Contains(ApiEndpointIdentity.RouteNameToRoutePath(p).ToLowerInvariant()))
                      .ToList();

        Assert.True(missing.Count == 0,
            $"种子 Permission 漂移 {missing.Count} 条（无对应活端点且不在豁免清单）——服务改名请改用 MenuSeedRouteName.Of&lt;T&gt; 拼接，端点下线请清理按钮或登记豁免：\n{string.Join("\n", missing)}");
    }

    [Fact]
    public void LegacyOrphanAllowlistHasNoDeadEntries()
    {
        var permissions = GlobalMenuSeedData.BuildData()
                                   .Where(u => !string.IsNullOrWhiteSpace(u.Permission))
                                   .Select(u => u.Permission!)
                                   .ToHashSet();

        var dead = LegacyOrphanAllowlist.Where(a => !permissions.Contains(a)).ToList();
        Assert.True(dead.Count == 0,
            $"豁免清单存在死条目（种子已不再使用，请移除）：{string.Join(", ", dead)}");
    }

    /// <summary>
    /// 契约样例：Of&lt;T&gt; 输出与手写规范形态逐字符一致（含 CRUD 模板路由与参数化路由两类代表）。
    /// </summary>
    [Fact]
    public void MenuSeedRouteNameOfProducesCanonicalRouteNames()
    {
        Assert.Equal("tenantOrganization:delete", MenuSeedRouteName.Of<TenantOrganizationService>(nameof(TenantOrganizationService.DeleteOrganizationAsync)));
        Assert.Equal("tenantGamePenaltyEntity:detail", MenuSeedRouteName.Of<TenantGamePenaltyEntityService>(nameof(TenantGamePenaltyEntityService.GetAsync)));
        Assert.Equal("globalPackage:detail:{id}", MenuSeedRouteName.Of<GlobalPackageService>(nameof(GlobalPackageService.GetDetailAsync)));
        Assert.Equal("tenantUser:ownRoleList:{userId}", MenuSeedRouteName.Of<TenantUserService>(nameof(TenantUserService.GetOwnRoleListAsync)));
    }

    /// <summary>
    /// 反射重建活端点路由集：IAdminApiController 全部公共动作 → api/... 全路径（lowercase）。
    /// 与 ApiEndpointCatalogSync.CollectLiveEndpoints / 本地 seedcatalog 工具同口径。
    /// </summary>
    private static HashSet<string> BuildLiveRoutes()
    {
        var routes = new HashSet<string>(StringComparer.Ordinal);
        var marker = typeof(IAdminApiController);
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()
                                     .Where(u => u.GetName().Name!.StartsWith("GameFrameX.", StringComparison.Ordinal)))
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (!type.IsClass || !type.IsPublic || type.IsAbstract || type.IsGenericTypeDefinition || !marker.IsAssignableFrom(type))
                {
                    continue;
                }

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (method.DeclaringType == typeof(object) || method.IsSpecialName)
                    {
                        continue;
                    }

                    if (method.GetCustomAttributes(true).Any(a => a.GetType().Name == "NonActionAttribute"))
                    {
                        continue;
                    }

                    var route = AdminApiRouteNaming.BuildRoutePath(type, method);
                    if (route.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
                    {
                        routes.Add(route.ToLowerInvariant());
                    }
                }
            }
        }

        return routes;
    }
}
