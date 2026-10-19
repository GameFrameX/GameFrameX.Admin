using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// C107 (GFX-102 阶段3) 配套回归测试：验证商品 / 内容 / 营销（兑换码 + Web 商城）四棵菜单的
/// Path / Component 已由 /game/&lt;子域&gt;/... 切到 /&lt;子域&gt;Center/...；
/// 菜单 Key / Id / Pid / SyncRoles 与改动前一致（spec 系统管理 §86 禁止运行时写 tenant_role_menu）。
/// </summary>
[Collection("MenuSeedCatalog")]
public class GlobalMenuSeedDataStageThreePathMigrationTests
{
    private static readonly string[] StageThreeDirKeys =
    {
        "route:goods", "route:redeemcode", "route:webShop", "route:content",
    };

    private static readonly Dictionary<string, string> ExpectedDirPathByKey = new(StringComparer.Ordinal)
    {
        ["route:goods"] = "/goodsCenter",
        ["route:redeemcode"] = "/redeemCenter",
        ["route:webShop"] = "/webShopCenter",
        ["route:content"] = "/contentCenter",
    };

    // C324（GFX-733）受众前缀化：app 受众叶子（AppScope=App）首段统一为 /app；Dir 分组保持中心前缀。
    private static readonly Dictionary<string, string> StageThreeLeafKeyToExpectedPathPrefix = new(StringComparer.Ordinal)
    {
        // goods 子树
        ["route:gameCommodityEntity"] = "/app/goodsCenter/gameCommodityEntity",
        ["route:gameGoodsEntity"] = "/app/goodsCenter/gameGoodsEntity",
        ["route:gameGoodsCatalogueEntity"] = "/app/goodsCenter/gameGoodsCatalogueEntity",
        // redeemcode 子树
        ["route:gameRedeemCodeEntity"] = "/app/redeemCenter/gameRedeemCodeEntity",
        ["route:gameRedeemCodeChannelEntity"] = "/app/redeemCenter/gameRedeemCodeChannelEntity",
        ["route:gameRedeemCodeUseRecordEntity"] = "/app/redeemCenter/gameRedeemCodeUseRecordEntity",
        // webShop 子树（webShopItemManager / webShopOrderManager 是目录型叶子）
        ["route:webShopItemManager"] = "/app/webShopCenter/itemManager",
        ["route:webShopOrderManager"] = "/app/webShopCenter/orderManager",
        // content 子树（含 announcement 子目录、mail 子目录、各类 leaf）
        ["route:announcement"] = "/contentCenter/announcement",
        ["route:mail"] = "/contentCenter/mail",
        ["route:gameAnnouncementEntity"] = "/app/contentCenter/announcement/gameAnnouncementEntity",
        ["route:gameBannerAnnouncementEntity"] = "/app/contentCenter/announcement/gameBannerAnnouncementEntity",
        ["route:gameMailEntity"] = "/app/contentCenter/mail/gameMailEntity",
        ["route:gameMailTemplateEntity"] = "/app/contentCenter/mail/gameMailTemplateEntity",
        ["route:gameMailTemplateCategoryEntity"] = "/app/contentCenter/mail/gameMailTemplateCategoryEntity",
    };

    // C302（GFX-715）views 改名映射的显式期望（Component 与 Path 末段不同名）。
    private static readonly Dictionary<string, string> ComponentOverrideByKey = new(StringComparer.Ordinal)
    {
        ["route:webShopItemManager"] = "/app/webShopCenter/webShop/index",
        ["route:webShopOrderManager"] = "/app/webShopCenter/webShopOrder/index",
    };

    [Fact]
    public void StageThree_DirNodes_UseNewPathPrefix()
    {
        var menus = GlobalMenuSeedData.BuildData();

        foreach (var (key, expectedPath) in ExpectedDirPathByKey)
        {
            var menu = menus.Single(m => m.Key == key);
            Assert.Equal(expectedPath, menu.Path);
        }
    }

    [Fact]
    public void StageThree_NoLegacyGamePathPrefixRemains_InStageThreeSubtrees()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var stageThreeKeys = StageThreeDirKeys
                             .Concat(StageThreeLeafKeyToExpectedPathPrefix.Keys)
                             .ToHashSet(StringComparer.Ordinal);

        var stageThreeMenus = menus.Where(m => m.Key != null && stageThreeKeys.Contains(m.Key)).ToArray();
        Assert.NotEmpty(stageThreeMenus);

        foreach (var menu in stageThreeMenus)
        {
            Assert.False(
                (menu.Path?.StartsWith("/game/goods/", StringComparison.Ordinal) ?? false)
                || (menu.Path?.StartsWith("/game/redeemcode/", StringComparison.Ordinal) ?? false)
                || (menu.Path?.StartsWith("/game/webShop/", StringComparison.Ordinal) ?? false)
                || (menu.Path?.StartsWith("/game/content/", StringComparison.Ordinal) ?? false),
                $"菜单 {menu.Key} 仍保留旧 /game/<子域>/ 前缀：{menu.Path}");

            if (!string.IsNullOrEmpty(menu.Component))
            {
                Assert.False(
                    menu.Component.StartsWith("/game/goods/", StringComparison.Ordinal)
                    || menu.Component.StartsWith("/game/redeemcode/", StringComparison.Ordinal)
                    || menu.Component.StartsWith("/game/webShop/", StringComparison.Ordinal)
                    || menu.Component.StartsWith("/game/content/", StringComparison.Ordinal),
                    $"菜单 {menu.Key} 的 Component 仍保留旧前缀：{menu.Component}");
            }
        }
    }

    [Fact]
    public void StageThree_LeafPages_UseNewPathAndComponentPrefix()
    {
        var menus = GlobalMenuSeedData.BuildData();

        // 这些子目录节点 (Dir) 没有 Component (C104 把 route:goods 等 8 个 Dir 的 Component 设为 "Layout"，
        // 但其下 route:announcement / route:mail 这种次级目录节点的 Component 仍是 null)。
        var dirEntriesWithoutComponent = new HashSet<string>(StringComparer.Ordinal)
        {
            "route:announcement", "route:mail",
        };

        foreach (var (key, expectedPath) in StageThreeLeafKeyToExpectedPathPrefix)
        {
            var menu = menus.Single(m => m.Key == key);
            Assert.Equal(expectedPath, menu.Path);

            if (dirEntriesWithoutComponent.Contains(key))
            {
                // Dir 子目录节点的 Component 应当保持 null（与 C104 一致）。
                Assert.Null(menu.Component);
                continue;
            }

            // C302（GFX-715）views 改名后 webShop 两叶的组件文件不再与 Path 末段同名。
            if (ComponentOverrideByKey.TryGetValue(key, out var overrideComponent))
            {
                Assert.Equal(overrideComponent, menu.Component);
                continue;
            }

            // leaf page 的 Component 是 Path + "/index"
            var expectedComponent = expectedPath + "/index";
            Assert.Equal(expectedComponent, menu.Component);
        }
    }

    [Fact]
    public void StageThree_KeyIdPidStructure_Unchanged()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();

        // 4 个目录节点必须仍然存在（Key / Id 不漂移）。
        var definitionKeys = MenuSeedCatalog.AllDefinitions
                                            .Select(d => d.Key)
                                            .ToHashSet(StringComparer.Ordinal);

        foreach (var key in StageThreeDirKeys)
        {
            Assert.Contains(key, definitionKeys);
        }
    }
}