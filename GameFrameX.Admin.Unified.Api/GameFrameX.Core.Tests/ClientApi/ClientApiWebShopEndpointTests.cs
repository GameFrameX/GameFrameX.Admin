using GameFrameX.Client.Api.GameWebShop;
using System.Globalization;
using System.Net;
using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Json;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiWebShopEndpointTests
{
    [Fact]
    public async Task GameWebShop_Get_ReturnsOnlyListedGoods()
    {
        using var db = new ClientApiTestDb();
        db.SeedWebShop(5001, "6.50", ItemJson(), true, 20);
        db.SeedWebShop(5002, "7.50", ItemJson(), false, 30);
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Get();

        var root = ClientApiResultReader.Read(result);
        var data = ClientApiResultReader.Data(root);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Single(data.EnumerateArray());
        Assert.Equal(5001, data[0].GetProperty("Id").GetInt64());
    }

    [Fact]
    public async Task GameWebShop_Buy_AppendsTrustedCallbackRouteToNotifyUrl()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson());
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler);
        controller.ControllerContext = new ControllerContext { HttpContext = db.CreateHttpContext(true, true, 7001), };

        var result = await controller.Buy(new TenantGameWebShopEntityBuyRequest
        {
            Id = 5001,
            AreaId = 2001,
            RoleId = 3001,
            Type = "alipay",
        });

        var root = ClientApiResultReader.Read(result);
        var body = WebUtility.UrlDecode(handler.RequestBodies.Single());
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Contains($"/{db.TenantId}/{db.AppId}/7001", body);
        Assert.StartsWith("https://zlpay.zilancreative.com/pay/ok", ClientApiResultReader.Data(root).GetProperty("Html").GetString());
    }

    [Fact]
    public async Task GameWebShop_Buy_RejectsMissingContextForCallbackUrl()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson());
        var controller = CreateController(db);
        db.AttachContext(controller, includeApp: false);

        var result = await controller.Buy(new TenantGameWebShopEntityBuyRequest
        {
            Id = 5001,
            AreaId = 2001,
            RoleId = 3001,
            Type = "alipay",
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(60300006, ClientApiResultReader.Code(root));
        Assert.Equal("回调地址缺少租户或应用上下文", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameWebShop_Buy_PreCreatesUnpaidOrderWithSnapshot()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson());
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler);
        db.AttachContext(controller);

        var result = await controller.Buy(new TenantGameWebShopEntityBuyRequest
        {
            Id = 5001,
            AreaId = 2001,
            RoleId = 3001,
            Type = "alipay",
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        var outTradeNo = ClientApiResultReader.Data(root).GetProperty("OrderId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(outTradeNo));
        Assert.True(long.TryParse(outTradeNo, out _), "out_trade_no 应为雪花 ID（可解析长整数）");

        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        Assert.NotNull(order);
        Assert.Equal(OrderStatusEnum.Unpaid, order.Status);
        Assert.Equal(650u, order.Amount);
        Assert.Equal("3001", order.RoleId);
        Assert.Equal(2001, order.AreaId);
        Assert.Equal(5001, order.Sku);
        Assert.Equal("1001", order.AccountId);
        Assert.Equal(db.AppId, order.AppId);
        Assert.Equal("网页支付", order.PayChannel);
        Assert.Equal("聚支付平台", order.Channel);
        Assert.Equal("web", order.Platform);
        Assert.Contains("RewardsItem", order.Content);
    }

    [Fact]
    public async Task GameWebShop_NotifyWebShop_CreatesShippedOrderAndSendsMail()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedGoods(11, configId: 90011);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson());
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler);
        db.AttachContext(controller);

        var outTradeNo = await BuyAndGetOrderIdAsync(controller);
        var result = await controller.NotifyWebShop(BuildSignedNotifyRequest(outTradeNo, 6.50m, "sign-secret"));

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(db.AppId, order.AppId);
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal(650, role.AllRecharge);
        Assert.Equal(1, handler.Count("/game/api/post_mail"));
    }

    [Fact]
    public async Task GameWebShop_NotifyWebShop_EnforceMode_RejectsForgedSign()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedGoods(11, configId: 90011);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson("enforce"));
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler);
        db.AttachContext(controller);

        var outTradeNo = await BuyAndGetOrderIdAsync(controller);
        var result = await controller.NotifyWebShop(BuildSignedNotifyRequest(outTradeNo, 6.50m, "sign-secret", "forged-sign"));

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        var failedOrders = await db.TenantDb.Queryable<TenantGameWebShopOrderEntity>().ToListAsync();
        Assert.Equal((int)AuthErrorCode.SignatureVerifyFailed, ClientApiResultReader.Code(root));
        Assert.Equal(OrderStatusEnum.Unpaid, order.Status);
        Assert.Equal(0, role.AllRecharge);
        Assert.Equal(0, handler.Count("/game/api/post_mail"));
        Assert.Single(failedOrders);
        Assert.Contains("验签失败", failedOrders[0].Reason);
    }

    [Fact]
    public async Task GameWebShop_NotifyWebShop_ReplaySameOutTradeNo_ShipsOnlyOnce()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedGoods(11, configId: 90011);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson());
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler);
        db.AttachContext(controller);

        var outTradeNo = await BuyAndGetOrderIdAsync(controller);
        var request = BuildSignedNotifyRequest(outTradeNo, 6.50m, "sign-secret");
        var first = ClientApiResultReader.Read(await controller.NotifyWebShop(request));
        var second = ClientApiResultReader.Read(await controller.NotifyWebShop(request));

        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(0, ClientApiResultReader.Code(first));
        Assert.Equal(0, ClientApiResultReader.Code(second));
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal(650, role.AllRecharge);
        Assert.Equal(1, handler.Count("/game/api/post_mail"));
    }

    [Fact]
    public async Task GameWebShop_NotifyWebShop_RejectsAmountMismatch()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedGoods(11, configId: 90011);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson());
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler);
        db.AttachContext(controller);

        var outTradeNo = await BuyAndGetOrderIdAsync(controller);
        var result = await controller.NotifyWebShop(BuildSignedNotifyRequest(outTradeNo, 1.00m, "sign-secret"));

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        var failedOrders = await db.TenantDb.Queryable<TenantGameWebShopOrderEntity>().ToListAsync();
        Assert.Equal((int)OrderErrorCode.NotifyAmountMismatch, ClientApiResultReader.Code(root));
        Assert.Equal(OrderStatusEnum.Unpaid, order.Status);
        Assert.Equal(0, role.AllRecharge);
        Assert.Equal(0, handler.Count("/game/api/post_mail"));
        Assert.Single(failedOrders);
        Assert.Contains("金额不符", failedOrders[0].Reason);
    }

    [Fact]
    public async Task GameWebShop_NotifyWebShop_LogMode_AllowsMismatchedSignAndShips()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedGoods(11, configId: 90011);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson());
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler);
        db.AttachContext(controller);

        var outTradeNo = await BuyAndGetOrderIdAsync(controller);
        var result = await controller.NotifyWebShop(BuildSignedNotifyRequest(outTradeNo, 6.50m, "sign-secret", "forged-sign"));

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal(1, handler.Count("/game/api/post_mail"));
    }

    [Fact]
    public async Task GameWebShop_NotifyWebShop_RejectsUnknownOrder()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedGoods(11, configId: 90011);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson());
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler);
        db.AttachContext(controller);

        var result = await controller.NotifyWebShop(BuildSignedNotifyRequest("999999999999999999", 6.50m, "sign-secret"));

        var root = ClientApiResultReader.Read(result);
        var orderCount = await db.TenantDb.Queryable<TenantGameOrderEntity>().CountAsync(u => u.OrderId == "999999999999999999");
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        var failedOrders = await db.TenantDb.Queryable<TenantGameWebShopOrderEntity>().ToListAsync();
        Assert.Equal((int)OrderErrorCode.NotifyOrderNotFound, ClientApiResultReader.Code(root));
        Assert.Equal(0, orderCount);
        Assert.Equal(0, role.AllRecharge);
        Assert.Equal(0, handler.Count("/game/api/post_mail"));
        Assert.Single(failedOrders);
    }

    [Fact]
    public async Task GameWebShop_NotifyWebShop_RejectsMissingAppContext()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);
        db.AttachContext(controller, includeApp: false);

        var result = await controller.NotifyWebShop(new TenantGameWebShopEntityBuyNotifyRequest { TradeStatus = "TRADE_SUCCESS", });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)AuthErrorCode.AuthorizeInvalid, ClientApiResultReader.Code(root));
        Assert.Equal("应用未认证", ClientApiResultReader.Message(root));
    }

    private static async Task<string> BuyAndGetOrderIdAsync(GameWebShopController controller)
    {
        var result = await controller.Buy(new TenantGameWebShopEntityBuyRequest
        {
            Id = 5001,
            AreaId = 2001,
            RoleId = 3001,
            Type = "alipay",
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        return ClientApiResultReader.Data(root).GetProperty("OrderId").GetString() ?? string.Empty;
    }

    /// <summary>
    /// 按易支付 MD5 口径构造带正确签名的回调请求，模拟 zlpay notify 合法回传；overrideSign 用于伪造签名场景。
    /// </summary>
    private static TenantGameWebShopEntityBuyNotifyRequest BuildSignedNotifyRequest(
        string outTradeNo,
        decimal money,
        string signKey,
        string? overrideSign = null)
    {
        var request = new TenantGameWebShopEntityBuyNotifyRequest
        {
            Pid = 1001,
            TradeNo = "provider-" + outTradeNo,
            OutTradeNo = outTradeNo,
            Type = "alipay",
            Name = "Shop-5001",
            Money = money,
            TradeStatus = "TRADE_SUCCESS",
            Param = "{\"RoleId\":\"3001\",\"AreaId\":\"2001\",\"ShopId\":\"5001\"}",
            SignType = "MD5",
        };
        request.Sign = overrideSign ?? GameWebShopController.CreateMd5Sign(BuildNotifySignParameters(request), signKey);
        return request;
    }

    /// <summary>
    /// 与 NotifyWebShop 验签侧同口径重建回传业务参数字典（money 用 invariant 无尾零格式，排除 sign/sign_type）。
    /// </summary>
    private static Dictionary<string, string> BuildNotifySignParameters(TenantGameWebShopEntityBuyNotifyRequest request)
    {
        return new Dictionary<string, string>
        {
            { "pid", request.Pid.ToString(CultureInfo.InvariantCulture) },
            { "trade_no", request.TradeNo },
            { "out_trade_no", request.OutTradeNo },
            { "type", request.Type },
            { "name", request.Name },
            { "money", request.Money.ToString(CultureInfo.InvariantCulture) },
            { "trade_status", request.TradeStatus },
            { "param", request.Param },
        };
    }

    private static GameWebShopController CreateController(ClientApiTestDb db, ClientApiRecordingHttpHandler? handler = null)
    {
        db.InitTenantTables(
            typeof(TenantGameWebShopEntity),
            typeof(TenantGameRoleEntity),
            typeof(TenantGameAreaEntity),
            typeof(TenantGameWebShopOrderEntity),
            typeof(TenantGameOrderEntity),
            typeof(TenantGameGoodsEntity));
        return new GameWebShopController(
            new HttpClient(handler ?? ClientApiRecordingHttpHandler.WithGameServerDefaults(), false),
            db.Repository<TenantGameWebShopEntity>(),
            db.Repository<TenantGameRoleEntity>(),
            db.Repository<TenantGameAreaEntity>(),
            db.Repository<TenantGameWebShopOrderEntity>(),
            db.Repository<TenantGameOrderEntity>(),
            db.Repository<SystemConfigEntity>());
    }

    private static string ItemJson()
    {
        return JsonHelper.Serialize(new[] { new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, }, });
    }

    private static string WebShopConfigJson(string verifySignMode = "log")
    {
        return JsonHelper.Serialize(new GameWebConfig
        {
            VerifySignMode = verifySignMode,
            Pid = "pid",
            Type = "alipay",
            Sign = "sign-secret",
            NotifyUrl = "https://client.example.test/api/GameWebShop/NotifyWebShop",
            ReturnUrl = "https://client.example.test/return",
            WebShopTitle = "Gift",
            WebShopSubTitle = "Gift sub",
            WebShopContent = "Gift content",
        });
    }
}