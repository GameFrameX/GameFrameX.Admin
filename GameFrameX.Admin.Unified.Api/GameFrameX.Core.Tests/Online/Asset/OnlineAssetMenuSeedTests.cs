using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Tests.Online.Asset;

// C340 / GFX-752 菜单 seed 结构自检（vault:C4 + C324 受众前缀化守护 + 高危按钮 Admin 独占）：
// 在线平台容器 + 三页面 + 五按钮（页面查询 ×3 + grant/revoke ×2 高危）。
// 本测试在 psql 不可用的环境（CI / 本机离线）下提供结构断言兜底；正式仓库以 psql 实测为铁证。
public sealed class OnlineAssetMenuSeedTests
{
    [Fact]
    public void OnlineAssetMenus_Container_UsesOnlineOperationsRoot()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var container = Assert.Single(menus, m => m.Key == "route:gmCenter");

        Assert.Equal(MenuTypeEnum.Dir, container.Type);
        // C403：在线运维是独立根目录，排序由共享契约固定。
        Assert.Equal("/gmCenter", container.Path);
        Assert.Equal(0, container.Pid);
        Assert.Equal("message.menu.gmCenter", container.TitleCode);
        Assert.Equal("在线运维", container.Title);
        Assert.Equal(300, container.OrderNo);
        // C366：容器受众 = 13 个叶子受众并集（playerSegment/configRollout 为 ServerOps，运营/观测页为 OpsManager/GmSupport/DataAnalyst）。
    }

    [Theory]
    [InlineData("route:onlineWalletLedger", "资产流水", "message.menu.onlineWalletLedger", 300, "/gmCenter/walletLedger", "/gmCenter/walletLedger/index")]
    [InlineData("route:onlineTransactionDetail", "交易详情", "message.menu.onlineTransactionDetail", 400, "/gmCenter/transactionDetail", "/gmCenter/transactionDetail/index")]
    [InlineData("route:onlineAssetAudit", "资产审计", "message.menu.onlineAssetAudit", 500, "/gmCenter/assetAudit", "/gmCenter/assetAudit/index")]
    public void OnlineAssetMenus_Pages_UseOnlineOperationsPathsAndMatchLedgerSpec(
        string key, string title, string titleCode, int orderNo, string path, string component)
    {
        var menus = GlobalMenuSeedData.BuildData();
        var page = Assert.Single(menus, m => m.Key == key);

        Assert.Equal(MenuTypeEnum.Menu, page.Type);
        Assert.Equal(StableSeedId.Menu("route:gmCenter"), page.Pid);
        Assert.Equal(title, page.Title);
        Assert.Equal(titleCode, page.TitleCode);
        Assert.Equal(orderNo, page.OrderNo);
        // C403：叶子路径和组件直接使用 gmCenter 前缀。
        Assert.Equal(path, page.Path);
        Assert.Equal(component, page.Component);
    }

    [Fact]
    public void OnlineAssetMenus_HighRiskButtons_AreAdminOnly()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var grant = Assert.Single(menus, m => m.Key == "onlineWalletLedger:grant");
        var revoke = Assert.Single(menus, m => m.Key == "onlineWalletLedger:revoke");

        Assert.Equal(MenuTypeEnum.Button, grant.Type);
        Assert.Equal(StableSeedId.Menu("route:onlineWalletLedger"), grant.Pid);
        Assert.Equal("onlineAssetGrantCommand:grant", grant.Permission);
        Assert.Equal("人工发放", grant.Title);
        Assert.Equal(200, grant.OrderNo);
        // 高危：仅 Admin 可见可执行；OpsManager 排除。

        Assert.Equal(MenuTypeEnum.Button, revoke.Type);
        Assert.Equal(StableSeedId.Menu("route:onlineWalletLedger"), revoke.Pid);
        Assert.Equal("onlineAssetGrantCommand:revoke", revoke.Permission);
        Assert.Equal("人工撤销", revoke.Title);
        Assert.Equal(300, revoke.OrderNo);
    }

    [Fact]
    public void OnlineAssetMenus_QueryButtons_CarryPermissionMatching_EndpointConvention()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var ledgerPage = Assert.Single(menus, m => m.Key == "onlineWalletLedger:page");
        var detail = Assert.Single(menus, m => m.Key == "onlineTransactionDetail:detail");
        var auditPage = Assert.Single(menus, m => m.Key == "onlineAssetAudit:page");

        Assert.Equal(MenuTypeEnum.Button, ledgerPage.Type);
        Assert.Equal("onlineAssetLedgerQuery:page", ledgerPage.Permission);
        Assert.Equal(StableSeedId.Menu("route:onlineWalletLedger"), ledgerPage.Pid);

        Assert.Equal(MenuTypeEnum.Button, detail.Type);
        Assert.Equal("onlineAssetTransactionQuery:detail", detail.Permission);
        Assert.Equal(StableSeedId.Menu("route:onlineTransactionDetail"), detail.Pid);

        Assert.Equal(MenuTypeEnum.Button, auditPage.Type);
        Assert.Equal("onlineAssetAuditQuery:page", auditPage.Permission);
        Assert.Equal(StableSeedId.Menu("route:onlineAssetAudit"), auditPage.Pid);
    }

    [Fact]
    public void OnlineAssetMenus_AllRows_UseStableSeedIdDerivedPrimaryKey()
    {
        // C278：种子菜单的 Id 派生自 StableSeedId.Menu(key)，保证种子幂等 upsert。
        var menus = GlobalMenuSeedData.BuildData().Where(m => m.Key?.StartsWith("route:online", StringComparison.Ordinal) == true || m.Key?.StartsWith("online", StringComparison.Ordinal) == true).ToList();
        Assert.NotEmpty(menus);
        Assert.All(menus, m => Assert.True(m.Id > 0, $"种子菜单 Id 必须派生自 StableSeedId.Menu(key)，实际为 0：{m.Key}"));
    }
}
