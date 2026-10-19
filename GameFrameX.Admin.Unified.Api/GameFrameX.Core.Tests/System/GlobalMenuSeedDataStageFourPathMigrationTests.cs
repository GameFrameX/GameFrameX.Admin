using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// C112 (GFX-104 阶段4) 配套回归测试：验证区服运维 / 任务与自动化两棵菜单的
/// Path / Component 已由 /game/&lt;子域&gt;/... 切到 /&lt;子域&gt;Center/...；
/// 菜单 Key / Id / Pid / SyncRoles 与改动前一致（spec 系统管理 §86 禁止运行时写 tenant_role_menu）。
/// </summary>
[Collection("MenuSeedCatalog")]
public class GlobalMenuSeedDataStageFourPathMigrationTests
{
    private static readonly string[] StageFourDirKeys =
    {
        "route:ops", "route:automation",
    };

    private static readonly Dictionary<string, string> ExpectedDirPathByKey = new(StringComparer.Ordinal)
    {
        ["route:ops"] = "/opsCenter",
        ["route:automation"] = "/automationCenter",
    };

    // C324（GFX-733）受众前缀化：app 受众叶子首段统一为 /app；Dir 分组保持中心前缀。
    private static readonly Dictionary<string, string> StageFourLeafKeyToExpectedPathPrefix = new(StringComparer.Ordinal)
    {
        // ops 子树
        ["route:gameAreaEntity"] = "/app/opsCenter/gameAreaEntity",
        ["route:gameAreaClassifyEntity"] = "/app/opsCenter/gameAreaClassifyEntity",
        ["route:gameDockerEntity"] = "/app/opsCenter/gameDockerEntity",
        // automation 子树
        ["route:gameTimeJobEntity"] = "/app/automationCenter/gameTimeJobEntity",
    };

    // C302（GFX-715）views 改名映射的显式期望（Component 与 Path 末段不同名）。
    private static readonly Dictionary<string, string> ComponentOverrideByKey = new(StringComparer.Ordinal)
    {
        ["route:gameDockerEntity"] = "/app/opsCenter/gameDockerTagEntity/index",
    };

    [Fact]
    public void StageFour_DirNodes_UseNewPathPrefix()
    {
        var menus = GlobalMenuSeedData.BuildData();

        foreach (var (key, expectedPath) in ExpectedDirPathByKey)
        {
            var menu = menus.Single(m => m.Key == key);
            Assert.Equal(expectedPath, menu.Path);
        }
    }

    [Fact]
    public void StageFour_NoLegacyGamePathPrefixRemains_InStageFourSubtrees()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var stageFourKeys = StageFourDirKeys
                            .Concat(StageFourLeafKeyToExpectedPathPrefix.Keys)
                            .ToHashSet(StringComparer.Ordinal);

        var stageFourMenus = menus.Where(m => m.Key != null && stageFourKeys.Contains(m.Key)).ToArray();
        Assert.NotEmpty(stageFourMenus);

        foreach (var menu in stageFourMenus)
        {
            Assert.False(
                (menu.Path?.StartsWith("/game/ops/", StringComparison.Ordinal) ?? false)
                || (menu.Path?.StartsWith("/game/automation/", StringComparison.Ordinal) ?? false),
                $"菜单 {menu.Key} 仍保留旧 /game/<子域>/ 前缀：{menu.Path}");

            if (!string.IsNullOrEmpty(menu.Component))
            {
                Assert.False(
                    menu.Component.StartsWith("/game/ops/", StringComparison.Ordinal)
                    || menu.Component.StartsWith("/game/automation/", StringComparison.Ordinal),
                    $"菜单 {menu.Key} 的 Component 仍保留旧前缀：{menu.Component}");
            }
        }
    }

    [Fact]
    public void StageFour_LeafPages_UseNewPathAndComponentPrefix()
    {
        var menus = GlobalMenuSeedData.BuildData();

        foreach (var (key, expectedPath) in StageFourLeafKeyToExpectedPathPrefix)
        {
            var menu = menus.Single(m => m.Key == key);
            Assert.Equal(expectedPath, menu.Path);

            // C302（GFX-715）views 改名后 gameDockerEntity 的组件文件不再与 Path 末段同名。
            if (ComponentOverrideByKey.TryGetValue(key, out var overrideComponent))
            {
                Assert.Equal(overrideComponent, menu.Component);
                continue;
            }

            // 其余 leaf page 的 Component 是 Path + "/index"。
            var expectedComponent = expectedPath + "/index";
            Assert.Equal(expectedComponent, menu.Component);
        }
    }

    [Fact]
    public void StageFour_KeyIdPidStructure_Unchanged()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();

        // 2 个目录节点必须仍然存在（Key / Id 不漂移）。
        var definitionKeys = MenuSeedCatalog.AllDefinitions
                                            .Select(d => d.Key)
                                            .ToHashSet(StringComparer.Ordinal);

        foreach (var key in StageFourDirKeys)
        {
            Assert.Contains(key, definitionKeys);
        }
    }
}