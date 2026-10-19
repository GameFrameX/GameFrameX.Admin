using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.ServerManager;

// C150 §3.3 / C175：GameServerEndpointUrlBuilder 契约自检。
// 覆盖五类 kind、root/prefix/action 的斜杠归一化、空 prefix 拒绝（含 ClientDispatch/Fulfillment）、非法 root。
public class GameServerEndpointUrlBuilderTests
{
    [Fact]
    public void Build_ClientDispatch_UsesConfiguredPrefix()
    {
        var area = NewArea();
        area.ClientDispatchApiPrefix = "game/api";

        var url = GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.ClientDispatch, "req_player_create");

        Assert.Equal("http://game.example/game/api/req_player_create", url);
    }

    [Fact]
    public void Build_Fulfillment_UsesConfiguredPrefix()
    {
        var area = NewArea();
        area.FulfillmentApiPrefix = "game/api";

        var url = GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Fulfillment, "post_mail");

        Assert.Equal("http://game.example/game/api/post_mail", url);
    }

    [Fact]
    public void Build_Gm_UsesGmPrefix()
    {
        var area = NewArea();
        area.GmApiPrefix = "gm/api";

        var url = GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Gm, "run_cmd");

        Assert.Equal("http://game.example/gm/api/run_cmd", url);
    }

    [Fact]
    public void Build_Control_UsesControlPrefix()
    {
        var area = NewArea();
        area.ControlApiPrefix = "ctl";

        var url = GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Control, "reload");

        Assert.Equal("http://game.example/ctl/reload", url);
    }

    [Fact]
    public void Build_NormalizesTrailingSlashOnRoot()
    {
        var area = NewArea("http://game.example/");

        var url = GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Fulfillment, "post_mail");

        Assert.Equal("http://game.example/game/api/post_mail", url);
    }

    [Fact]
    public void Build_NormalizesLeadingSlashOnAction()
    {
        var area = NewArea();

        var url = GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Fulfillment, "/post_mail");

        Assert.Equal("http://game.example/game/api/post_mail", url);
    }

    [Fact]
    public void Build_NormalizesSurroundingSlashesOnPrefix()
    {
        var area = NewArea();
        area.FulfillmentApiPrefix = "/game/api/";

        var url = GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Fulfillment, "post_mail");

        Assert.Equal("http://game.example/game/api/post_mail", url);
    }

    [Fact]
    public void Build_ClientDispatch_ThrowsWhenPrefixEmpty()
    {
        var area = NewArea();
        area.ClientDispatchApiPrefix = string.Empty;

        Assert.Throws<InvalidOperationException>(() =>
                                                     GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.ClientDispatch, "req_player_create"));
    }

    [Fact]
    public void Build_Fulfillment_ThrowsWhenPrefixEmpty()
    {
        var area = NewArea();
#pragma warning disable CS8625 // 测试意图：赋 null 模拟未配置 FulfillmentApiPrefix，验证守卫抛异常
        area.FulfillmentApiPrefix = null;
#pragma warning restore CS8625

        Assert.Throws<InvalidOperationException>(() =>
                                                     GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Fulfillment, "req_player_send_item"));
    }

    [Fact]
    public void Build_Gm_ThrowsWhenPrefixEmpty()
    {
        var area = NewArea();
        area.GmApiPrefix = string.Empty;

        Assert.Throws<InvalidOperationException>(() =>
                                                     GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Gm, "run_cmd"));
    }

    [Fact]
    public void Build_Control_ThrowsWhenPrefixEmpty()
    {
        var area = NewArea();
#pragma warning disable CS8625 // 测试意图：赋 null 模拟未配置 ControlApiPrefix，验证守卫抛异常
        area.ControlApiPrefix = null;
#pragma warning restore CS8625

        Assert.Throws<InvalidOperationException>(() =>
                                                     GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Control, "reload"));
    }

    // C339 / GFX-750：OnlineAdmin 第五档（Online 平台管理，vault:C2 S1.7）——默认前缀 / 自定义前缀 / 空值拒绝（同 Gm/Control 守卫）。
    [Fact]
    public void Build_OnlineAdmin_UsesDefaultPrefix()
    {
        var area = NewArea();

        var url = GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.OnlineAdmin, "kick_player");

        Assert.Equal("http://game.example/online/admin/kick_player", url);
    }

    [Fact]
    public void Build_OnlineAdmin_UsesConfiguredPrefix()
    {
        var area = NewArea();
        area.OnlineAdminApiPrefix = "online/v2";

        var url = GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.OnlineAdmin, "kick_player");

        Assert.Equal("http://game.example/online/v2/kick_player", url);
    }

    [Fact]
    public void Build_OnlineAdmin_ThrowsWhenPrefixEmpty()
    {
        var area = NewArea();
        area.OnlineAdminApiPrefix = string.Empty;

        Assert.Throws<InvalidOperationException>(() =>
                                                     GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.OnlineAdmin, "kick_player"));
    }

    [Fact]
    public void Build_ThrowsWhenRootMissing()
    {
        var area = NewArea(string.Empty);

        Assert.Throws<InvalidOperationException>(() =>
                                                     GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Fulfillment, "post_mail"));
    }

    [Fact]
    public void Build_ThrowsWhenRootNotAbsoluteHttp()
    {
        var area = NewArea("not-a-url");
        area.FulfillmentApiPrefix = "game/api";

        Assert.Throws<InvalidOperationException>(() =>
                                                     GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Fulfillment, "post_mail"));
    }

    [Fact]
    public void Build_ThrowsWhenAreaNull()
    {
#pragma warning disable CS8625 // 测试意图：传入 null area 验证守卫抛 ArgumentNullException
        Assert.Throws<ArgumentNullException>(() =>
                                                 GameServerEndpointUrlBuilder.Build(null, GameServerEndpointKindEnum.Fulfillment, "post_mail"));
#pragma warning restore CS8625
    }

    private static TenantGameAreaEntity NewArea(string root = "http://game.example")
    {
        return new TenantGameAreaEntity { HttpManageUrl = root, };
    }
}