using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Tests.Online.Asset;

// C340 / GFX-752 菜单 seed 结构自检（vault:C4 + C324 受众前缀化守护 + 高危按钮 TenantAdmin 独占）：
// 在线平台容器 + 三页面 + 五按钮（页面查询 ×3 + grant/revoke ×2 高危）。
// 本测试在 psql 不可用的环境（CI / 本机离线）下提供结构断言兜底；正式仓库以 psql 实测为铁证。
public sealed class OnlineAssetMenuSeedTests
{
    [Fact]
    public void OnlineAssetMenus_Container_PathMatches_C324_DirForm_And_Dedup_WithC350()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var container = Assert.Single(menus, m => m.Key == "route:onlineCenter");

        Assert.Equal(MenuTypeEnum.Dir, container.Type);
        // C324：Dir 无 /app 受众段；C366：提升为顶层根（Pid=0），OrderNo=550 紧邻区服运维 500。
        Assert.Equal("/onlineCenter", container.Path);
        Assert.Equal(0, container.Pid);
        Assert.Equal(MenuAppScopeEnum.App, container.AppScope);
        Assert.Equal("message.menu.onlineCenter", container.TitleCode);
        Assert.Equal("在线平台", container.Title);
        Assert.Equal(550, container.OrderNo);
        // C366：容器受众 = 13 个叶子受众并集（playerSegment/configRollout 为 ServerOps，运营/观测页为 OpsManager/GmSupport/DataAnalyst）。
        Assert.Equal((int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.ServerOps | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.GmSupport | TenantMenuSyncRole.DataAnalyst), container.SyncRoles);
    }

    [Theory]
    [InlineData("route:onlineWalletLedger", "资产流水", "message.menu.onlineWalletLedger", 300, "/app/onlineCenter/walletLedger", "/app/onlineCenter/walletLedger/index")]
    [InlineData("route:onlineTransactionDetail", "交易详情", "message.menu.onlineTransactionDetail", 400, "/app/onlineCenter/transactionDetail", "/app/onlineCenter/transactionDetail/index")]
    [InlineData("route:onlineAssetAudit", "资产审计", "message.menu.onlineAssetAudit", 500, "/app/onlineCenter/assetAudit", "/app/onlineCenter/assetAudit/index")]
    public void OnlineAssetMenus_Pages_Use_C324_AppAudienceSegment_AndMatchLedgerSpec(
        string key, string title, string titleCode, int orderNo, string path, string component)
    {
        var menus = GlobalMenuSeedData.BuildData();
        var page = Assert.Single(menus, m => m.Key == key);

        Assert.Equal(MenuTypeEnum.Menu, page.Type);
        Assert.Equal(MenuAppScopeEnum.App, page.AppScope);
        Assert.Equal(StableSeedId.Menu("route:onlineCenter"), page.Pid);
        Assert.Equal(title, page.Title);
        Assert.Equal(titleCode, page.TitleCode);
        Assert.Equal(orderNo, page.OrderNo);
        // C324：叶子页面带 /app 受众段；Dir 不带。
        Assert.Equal(path, page.Path);
        Assert.Equal(component, page.Component);
        Assert.Equal((int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager), page.SyncRoles);
    }

    [Fact]
    public void OnlineAssetMenus_HighRiskButtons_AreTenantAdminOnly()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var grant = Assert.Single(menus, m => m.Key == "onlineWalletLedger:grant");
        var revoke = Assert.Single(menus, m => m.Key == "onlineWalletLedger:revoke");

        Assert.Equal(MenuTypeEnum.Button, grant.Type);
        Assert.Equal(StableSeedId.Menu("route:onlineWalletLedger"), grant.Pid);
        Assert.Equal("onlineAssetGrantCommand:grant", grant.Permission);
        Assert.Equal("人工发放", grant.Title);
        Assert.Equal(200, grant.OrderNo);
        // 高危：仅 TenantAdmin 可见可执行；OpsManager 排除。
        Assert.Equal((int)TenantMenuSyncRole.TenantAdmin, grant.SyncRoles);

        Assert.Equal(MenuTypeEnum.Button, revoke.Type);
        Assert.Equal(StableSeedId.Menu("route:onlineWalletLedger"), revoke.Pid);
        Assert.Equal("onlineAssetGrantCommand:revoke", revoke.Permission);
        Assert.Equal("人工撤销", revoke.Title);
        Assert.Equal(300, revoke.OrderNo);
        Assert.Equal((int)TenantMenuSyncRole.TenantAdmin, revoke.SyncRoles);
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
        Assert.Equal((int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager), ledgerPage.SyncRoles);

        Assert.Equal(MenuTypeEnum.Button, detail.Type);
        Assert.Equal("onlineAssetTransactionQuery:detail", detail.Permission);
        Assert.Equal(StableSeedId.Menu("route:onlineTransactionDetail"), detail.Pid);
        Assert.Equal((int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager), detail.SyncRoles);

        Assert.Equal(MenuTypeEnum.Button, auditPage.Type);
        Assert.Equal("onlineAssetAuditQuery:page", auditPage.Permission);
        Assert.Equal(StableSeedId.Menu("route:onlineAssetAudit"), auditPage.Pid);
        Assert.Equal((int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager), auditPage.SyncRoles);
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
