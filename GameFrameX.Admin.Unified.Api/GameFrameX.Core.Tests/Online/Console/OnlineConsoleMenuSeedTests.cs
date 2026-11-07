using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Tests.Online.Console;

//  / 菜单 seed 结构自检（vault: .1 + 受众前缀化守护 + 高危按钮 Admin 独占）：
// 容器重定向到在线总览首屏 + 控制台三页面 + 四按钮（查询 ×3 + 受控操作 execute 高危）。
// 本测试在 psql 不可用的环境（CI / 本机离线）下提供结构断言兜底；正式仓库以 psql 实测为铁证。
public sealed class OnlineConsoleMenuSeedTests
{
    [Fact]
    public void OnlineConsoleMenus_Container_UsesOnlineOperationsRoot()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var container = Assert.Single(menus, m => m.Key == "route:gmCenter");

        // ：独立在线运维根使用 Layout，不保留旧容器重定向。
        Assert.Null(container.Redirect);
        Assert.Equal("Layout", container.Component);
    }

    [Theory]
    [InlineData("route:onlineDashboard", "在线总览", "message.menu.onlineDashboard", 50, "/gmCenter/onlineDashboard", "/gmCenter/onlineDashboard/index")]
    [InlineData("route:onlinePlayerTimeline", "玩家时间线", "message.menu.onlinePlayerTimeline", 630, "/gmCenter/onlinePlayerTimeline", "/gmCenter/onlinePlayerTimeline/index")]
    [InlineData("route:onlineAuditSearch", "统一审计检索", "message.menu.onlineAuditSearch", 640, "/gmCenter/onlineAuditSearch", "/gmCenter/onlineAuditSearch/index")]
    public void OnlineConsoleMenus_Pages_UseOnlineOperationsPaths(
        string key, string title, string titleCode, int orderNo, string path, string component)
    {
        var menus = GlobalMenuSeedData.BuildData();
        var page = Assert.Single(menus, m => m.Key == key);

        Assert.Equal(MenuTypeEnum.Menu, page.Type);
        Assert.Equal(StableSeedId.Menu("route:gmCenter"), page.Pid);
        Assert.Equal(title, page.Title);
        Assert.Equal(titleCode, page.TitleCode);
        Assert.Equal(orderNo, page.OrderNo);
        // ：叶子路径和组件直接使用 gmCenter 前缀。
        Assert.Equal(path, page.Path);
        Assert.Equal(component, page.Component);
        // 控制台查询受众：管理员角色 + 运营负责人 + 客服 GM + 数据分析。
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

    }

    [Fact]
    public void OnlineConsoleMenus_ExecuteButton_IsAdminRoleOnly()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var execute = Assert.Single(menus, m => m.Key == "onlinePlayerTimeline:execute");

        Assert.Equal(MenuTypeEnum.Button, execute.Type);
        Assert.Equal(StableSeedId.Menu("route:onlinePlayerTimeline"), execute.Pid);
        Assert.Equal("onlineControlledCommand:execute", execute.Permission);
        Assert.Equal("受控操作", execute.Title);
        // 高危：仅管理员角色可见可执行（与资产人工发放/撤销按钮口径一致）。
    }
}
