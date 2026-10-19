using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// C109 (GFX-100 阶段2) 配套回归测试：验证应用中心 / 玩家中心菜单的
/// Path / Component 已由 /game/app、/game/player 切到 /appCenter、/playerCenter；
/// 菜单 Key / Id / Pid 结构保持稳定，避免破坏既有 tenant_role_menu 授权事实。
/// </summary>
[Collection("MenuSeedCatalog")]
public class GlobalMenuSeedDataStageTwoPathMigrationTests
{
    [Fact]
    public void StageTwo_DirNodes_UseNewPathPrefix()
    {
        var menus = GlobalMenuSeedData.BuildData();

        Assert.Equal("/appCenter", menus.Single(m => m.Key == "route:app").Path);
        Assert.Equal("/playerCenter", menus.Single(m => m.Key == "route:player").Path);
    }

    [Fact]
    public void StageTwo_NoLegacyGamePathPrefixRemains_InAppAndPlayerSubtrees()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var stageTwoMenus = CollectSubtreeMenus(menus, "route:app")
                            .Concat(CollectSubtreeMenus(menus, "route:player"))
                            .ToArray();

        Assert.NotEmpty(stageTwoMenus);

        foreach (var menu in stageTwoMenus)
        {
            Assert.False(
                menu.Path != null
                && (menu.Path.Equals("/game/app", StringComparison.Ordinal)
                    || menu.Path.Equals("/game/player", StringComparison.Ordinal)
                    || menu.Path.StartsWith("/game/app/", StringComparison.Ordinal)
                    || menu.Path.StartsWith("/game/player/", StringComparison.Ordinal)),
                $"菜单 {menu.Key} 仍保留旧 /game/app 或 /game/player Path：{menu.Path}");

            Assert.False(
                menu.Component != null
                && (menu.Component.StartsWith("/game/app/", StringComparison.Ordinal)
                    || menu.Component.StartsWith("/game/player/", StringComparison.Ordinal)),
                $"菜单 {menu.Key} 仍保留旧 /game/app 或 /game/player Component：{menu.Component}");
        }
    }

    [Fact]
    public void StageTwo_SubtreePathsAndComponents_UseExpectedNewPrefix()
    {
        var menus = GlobalMenuSeedData.BuildData();

        // C324（GFX-733）受众前缀化后：叶子首段为受众段（/app 或 /tenant），业务段仍聚合在中心前缀下。
        AssertSubtreePrefix(menus, "route:app", "/appCenter");
        AssertSubtreePrefix(menus, "route:player", "/playerCenter");
    }

    [Fact]
    public void StageTwo_KeyIdPidStructure_Unchanged()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var expectedParents = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["route:gameAppEntity"] = "route:app",
            ["route:gameAppPackage"] = "route:app",
            ["route:gameAppRelease"] = "route:app",
            ["route:gameAppResource"] = "route:app",
            ["route:appRuntimeConfig"] = "route:app",
            ["route:appIntegrationConfig"] = "route:app",
            ["route:tenantDeviceManagement"] = "route:app",
            ["route:gamePaymentConfigEntity"] = "route:appIntegrationConfig",
            ["route:tenantGameAdvertisementRewardEntity"] = "route:appIntegrationConfig",
            ["route:clientApiConfig"] = "route:app",
            ["route:tenantGameLoginTicketKeyEntity"] = "route:clientApiConfig",
            ["route:tenantClientApiPolicyEntity"] = "route:clientApiConfig",
            ["route:serverConfig"] = "route:appRuntimeConfig",
            ["route:featureSwitch"] = "route:appRuntimeConfig",
            ["route:remoteConfig"] = "route:appRuntimeConfig",
            ["route:configKey"] = "route:appRuntimeConfig",
            ["route:configMode"] = "route:appRuntimeConfig",
            ["route:gameRoleEntity"] = "route:player",
            ["route:gameUserEntity"] = "route:player",
            ["route:gameOrderEntity"] = "route:player",
        };

        Assert.Equal(StableSeedId.Menu("route:app"), menus.Single(m => m.Key == "route:app").Id);
        Assert.Equal(StableSeedId.Menu("route:player"), menus.Single(m => m.Key == "route:player").Id);

        foreach (var expectedParent in expectedParents)
        {
            var menu = menus.Single(m => m.Key == expectedParent.Key);
            Assert.Equal(StableSeedId.Menu(expectedParent.Key), menu.Id);
            Assert.Equal(StableSeedId.Menu(expectedParent.Value), menu.Pid);
        }
    }

    private static void AssertSubtreePrefix(IReadOnlyCollection<GlobalMenuEntity> menus, string rootKey, string expectedPrefix)
    {
        foreach (var menu in CollectSubtreeMenus(menus, rootKey))
        {
            // C324（GFX-733）受众前缀化后：views 叶子的 Path/Component 首段是受众段（/platform、/tenant、/app），
            // 剥离受众段后的业务段必须仍聚合在中心前缀（expectedPrefix）下；Dir 分组节点保持中心前缀不变。
            var audienceSegments = new[] { "/platform", "/tenant", "/app" };

            if (menu.Path != null)
            {
                var path = menu.Path;
                foreach (var audience in audienceSegments)
                {
                    if (path.StartsWith(audience + "/", StringComparison.Ordinal))
                    {
                        path = path.Substring(audience.Length);
                        break;
                    }
                }

                Assert.True(
                    path.Equals(expectedPrefix, StringComparison.Ordinal)
                    || path.StartsWith(expectedPrefix + "/", StringComparison.Ordinal),
                    $"菜单 {menu.Key} Path 剥离受众段后未使用 {expectedPrefix} 前缀：{menu.Path}");
            }

            if (menu.Component != null && menu.Component.StartsWith("/", StringComparison.Ordinal))
            {
                var component = menu.Component;
                foreach (var audience in audienceSegments)
                {
                    if (component.StartsWith(audience + "/", StringComparison.Ordinal))
                    {
                        component = component.Substring(audience.Length);
                        break;
                    }
                }

                Assert.True(
                    component.StartsWith(expectedPrefix + "/", StringComparison.Ordinal),
                    $"菜单 {menu.Key} Component 剥离受众段后未使用 {expectedPrefix} 前缀：{menu.Component}");
            }
        }
    }

    private static IEnumerable<GlobalMenuEntity> CollectSubtreeMenus(IReadOnlyCollection<GlobalMenuEntity> menus, string rootKey)
    {
        var root = menus.Single(m => m.Key == rootKey);
        var pending = new Queue<long>();
        pending.Enqueue(root.Id);
        yield return root;

        while (pending.Count > 0)
        {
            var parentId = pending.Dequeue();
            var children = menus.Where(m => m.Pid == parentId).ToArray();
            foreach (var child in children)
            {
                pending.Enqueue(child.Id);
                yield return child;
            }
        }
    }
}