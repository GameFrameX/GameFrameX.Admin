using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Tests.Online.Console;

// C344 / GFX-760 菜单 seed 结构自检（vault:C9 S8.1 + C324 受众前缀化守护 + 高危按钮 TenantAdmin 独占）：
// 容器重定向到在线总览首屏 + 控制台三页面 + 四按钮（查询 ×3 + 受控操作 execute 高危）。
// 本测试在 psql 不可用的环境（CI / 本机离线）下提供结构断言兜底；正式仓库以 psql 实测为铁证。
public sealed class OnlineConsoleMenuSeedTests
{
    [Fact]
    public void OnlineConsoleMenus_Container_RedirectsToDashboardPage()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var container = Assert.Single(menus, m => m.Key == "route:onlineCenter");

        // 容器点击直达首子页（与 platform 容器重定向先例同构：Redirect = 目标页面完整 Path，含 /app 受众段）。
        Assert.Equal("/app/onlineCenter/onlineDashboard", container.Redirect);
    }

    [Theory]
    [InlineData("route:onlineDashboard", "在线总览", "message.menu.onlineDashboard", 50, "/app/onlineCenter/onlineDashboard", "/app/onlineCenter/onlineDashboard/index")]
    [InlineData("route:onlinePlayerTimeline", "玩家时间线", "message.menu.onlinePlayerTimeline", 630, "/app/onlineCenter/onlinePlayerTimeline", "/app/onlineCenter/onlinePlayerTimeline/index")]
    [InlineData("route:onlineAuditSearch", "统一审计检索", "message.menu.onlineAuditSearch", 640, "/app/onlineCenter/onlineAuditSearch", "/app/onlineCenter/onlineAuditSearch/index")]
    public void OnlineConsoleMenus_Pages_Use_C324_AppAudienceSegment(
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
        // C324：叶子页面带 /app 受众段。
        Assert.Equal(path, page.Path);
        Assert.Equal(component, page.Component);
        // 控制台查询受众：租户管理员 + 运营负责人 + 客服 GM + 数据分析。
        Assert.Equal((int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.GmSupport | TenantMenuSyncRole.DataAnalyst), page.SyncRoles);
    }

    [Fact]
    public void OnlineConsoleMenus_QueryButtons_CarryPermissionMatching_EndpointConvention()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var snapshot = Assert.Single(menus, m => m.Key == "onlineDashboard:snapshot");
        var timelineQuery = Assert.Single(menus, m => m.Key == "onlinePlayerTimeline:query");
        var auditPage = Assert.Single(menus, m => m.Key == "onlineAuditSearch:page");

        Assert.Equal(MenuTypeEnum.Button, snapshot.Type);
        Assert.Equal(StableSeedId.Menu("route:onlineDashboard"), snapshot.Pid);
        Assert.Equal("onlineOverviewQuery:snapshot", snapshot.Permission);

        Assert.Equal(MenuTypeEnum.Button, timelineQuery.Type);
        Assert.Equal(StableSeedId.Menu("route:onlinePlayerTimeline"), timelineQuery.Pid);
        Assert.Equal("onlinePlayerTimelineQuery:query", timelineQuery.Permission);

        Assert.Equal(MenuTypeEnum.Button, auditPage.Type);
        Assert.Equal(StableSeedId.Menu("route:onlineAuditSearch"), auditPage.Pid);
        Assert.Equal("onlineAuditSearch:page", auditPage.Permission);

        var queryRoles = (int)(TenantMenuSyncRole.TenantAdmin | TenantMenuSyncRole.OpsManager | TenantMenuSyncRole.GmSupport | TenantMenuSyncRole.DataAnalyst);
        Assert.Equal(queryRoles, snapshot.SyncRoles);
        Assert.Equal(queryRoles, timelineQuery.SyncRoles);
        Assert.Equal(queryRoles, auditPage.SyncRoles);
    }

    [Fact]
    public void OnlineConsoleMenus_ExecuteButton_IsTenantAdminOnly()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var execute = Assert.Single(menus, m => m.Key == "onlinePlayerTimeline:execute");

        Assert.Equal(MenuTypeEnum.Button, execute.Type);
        Assert.Equal(StableSeedId.Menu("route:onlinePlayerTimeline"), execute.Pid);
        Assert.Equal("onlineControlledCommand:execute", execute.Permission);
        Assert.Equal("受控操作", execute.Title);
        // 高危：仅租户管理员可见可执行（与资产人工发放/撤销按钮口径一致）。
        Assert.Equal((int)TenantMenuSyncRole.TenantAdmin, execute.SyncRoles);
    }
}
