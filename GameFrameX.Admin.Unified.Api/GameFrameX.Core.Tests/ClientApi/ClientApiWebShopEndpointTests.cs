using GameFrameX.Client.Api.GameWebShop;
using System.Globalization;
using System.Net;
using GameFrameX.Client.Api.GameUserEmail.Dto;
using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Http.Normalization;
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
    public async Task GameWebShop_Buy_BuildsCallbackUrlFromSiteRootWithPackageSegment()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson("https://client.example.test"));
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
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Contains("/submit.php", handler.RequestPaths);
        // 站点根地址自动补全路由段 + 当前分包段，提交给网关的 notify_url 能命中 {packageId} 路由
        Assert.Contains(handler.RequestBodies, requestBody => requestBody.Contains("notify_url=https%3A%2F%2Fclient.example.test%2Fapi%2FGameWebShop%2FNotifyWebShop%2F7001", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GameWebShop_Buy_RejectsLegacyThreeSegmentCallbackUrl()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson("https://client.example.test/api/GameWebShop/NotifyWebShop/123/456/7001"));
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
        Assert.Equal((int)AreaErrorCode.AreaCannotBuy, ClientApiResultReader.Code(root));
        // 旧三段地址不再去段重建：下单前拒绝，不提交网关
        Assert.DoesNotContain("/submit.php", handler.RequestPaths);
    }

    [Fact]
    public async Task GameWebShop_Buy_RejectsUnmappableCallbackUrl()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson("https://client.example.test/pay/notify"));
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
        // 无法映射到回调路由的地址在下单前拒绝：不提交网关、不预创建订单
        Assert.Equal((int)AreaErrorCode.AreaCannotBuy, ClientApiResultReader.Code(root));
        Assert.DoesNotContain("/submit.php", handler.RequestPaths);
        Assert.Equal(0, await db.MainDb.Queryable<TenantGameOrderEntity>().CountAsync());
    }

    [Fact]
    public async Task GameWebShop_Buy_DoesNotRequireLegacyTenantOrAppContext()
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
        Assert.Equal(0, ClientApiResultReader.Code(root));
    }

    [Fact]
    public async Task GameWebShop_Buy_PersistsOrderWithSingleKeyCallbackRoute()
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
        Assert.Equal(1, await db.MainDb.Queryable<TenantGameOrderEntity>().CountAsync());
        Assert.Contains(handler.RequestBodies, requestBody => requestBody.Contains("NotifyWebShop", StringComparison.Ordinal));
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

        var outTradeNo = await SeedUnpaidOrderAsync(db);
        var result = await controller.NotifyWebShop(BuildSignedNotifyRequest(outTradeNo, 6.50m, "sign-secret"));

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal(650, role.AllRecharge);
        Assert.Equal(1, handler.Count("/game/api/post_mail"));
        // 邮件按订单快照发货：商品 Id 已映射为道具 ConfigId
        Assert.Contains(handler.RequestBodies, requestBody => requestBody.Contains("90011", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GameWebShop_NotifyWebShop_RejectsForgedSign()
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

        var outTradeNo = await SeedUnpaidOrderAsync(db);
        var result = await controller.NotifyWebShop(BuildSignedNotifyRequest(outTradeNo, 6.50m, "sign-secret", "forged-sign"));

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        var failedOrders = await db.TenantDb.Queryable<TenantGameWebShopOrderEntity>().ToListAsync();
        // 验签缺省强制：伪造签名一律拒绝且不发货（不再有 log 灰度放行）
        Assert.Equal((int)AuthErrorCode.SignatureVerifyFailed, ClientApiResultReader.Code(root));
        Assert.Equal(OrderStatusEnum.Unpaid, order.Status);
        Assert.Equal(0, role.AllRecharge);
        Assert.Equal(0, handler.Count("/game/api/post_mail"));
        Assert.Single(failedOrders);
        Assert.Contains("验签失败", failedOrders[0].Reason);
    }

    [Fact]
    public async Task GameWebShop_NotifyWebShop_ShipsFromOrderSnapshotWhenShopChangedOrDelisted()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedGoods(11, configId: 90011);
        db.SeedGoods(12, configId: 90012);
        // 下单后商品被修改（换成 goods 12）并下架：发货仍按订单快照（goods 11）
        db.SeedWebShop(5001, "6.50", JsonHelper.Serialize(new[] { new PropertyItemResponse { Id = 12, ItemId = 12, Count = 9, }, }), isUp: false);
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson());
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler);
        db.AttachContext(controller);

        var outTradeNo = await SeedUnpaidOrderAsync(db);
        var result = await controller.NotifyWebShop(BuildSignedNotifyRequest(outTradeNo, 6.50m, "sign-secret"));

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal(650, role.AllRecharge);
        Assert.Equal(1, handler.Count("/game/api/post_mail"));
        // 邮件内容来自订单快照的 goods 11（ConfigId 90011），不是商品当前被改成的 goods 12
        Assert.Contains(handler.RequestBodies, requestBody => requestBody.Contains("90011", StringComparison.Ordinal));
        Assert.DoesNotContain(handler.RequestBodies, requestBody => requestBody.Contains("90012", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GameWebShop_NotifyWebShop_MailFailureKeepsOrderRetryableThenSucceedsOnRetry()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedGoods(11, configId: 90011);
        db.SeedWebShop(5001, "6.50", ItemJson());
        db.SeedSystemConfig("web_shop_params", WebShopConfigJson());
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        handler.SetJson("/game/api/post_mail", HttpJsonResultData<GameWebShpBuySendMailResponse>.Fail("游戏服邮件网关不可用").ToString());
        var controller = CreateController(db, handler);
        db.AttachContext(controller);

        var outTradeNo = await SeedUnpaidOrderAsync(db);
        var failed = ClientApiResultReader.Read(await controller.NotifyWebShop(BuildSignedNotifyRequest(outTradeNo, 6.50m, "sign-secret")));

        var orderAfterFailure = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        var roleAfterFailure = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        var failedRows = await db.TenantDb.Queryable<TenantGameWebShopOrderEntity>().ToListAsync();
        // 发货失败：订单不能停在已完成，保留 NotShipped 可重试状态，累充不增加
        Assert.Equal((int)AreaErrorCode.AreaCannotBuy, ClientApiResultReader.Code(failed));
        Assert.Equal(OrderStatusEnum.NotShipped, orderAfterFailure.Status);
        Assert.Equal(0, roleAfterFailure.AllRecharge);
        Assert.Single(failedRows);

        // 游戏服恢复后网关重试：重新发货成功，迁移 Shipped 并恰一累充
        handler.SetJson("/game/api/post_mail", HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameWebShpBuySendMailResponse { ErrorCode = 0, })).ToString());
        var retried = ClientApiResultReader.Read(await controller.NotifyWebShop(BuildSignedNotifyRequest(outTradeNo, 6.50m, "sign-secret")));

        var orderAfterRetry = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        var roleAfterRetry = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(0, ClientApiResultReader.Code(retried));
        Assert.Equal(OrderStatusEnum.Shipped, orderAfterRetry.Status);
        Assert.Equal(650, roleAfterRetry.AllRecharge);
        Assert.Equal(2, handler.Count("/game/api/post_mail"));
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

        var outTradeNo = await SeedUnpaidOrderAsync(db);
        var request = BuildSignedNotifyRequest(outTradeNo, 6.50m, "sign-secret");
        var first = ClientApiResultReader.Read(await controller.NotifyWebShop(request));
        var second = ClientApiResultReader.Read(await controller.NotifyWebShop(request));

        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(0, ClientApiResultReader.Code(first));
        Assert.Equal(0, ClientApiResultReader.Code(second));
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal(650, role.AllRecharge);
    }

    [Theory]
    [InlineData(OrderStatusEnum.Voided)]
    [InlineData(OrderStatusEnum.Refunded)]
    public async Task GameWebShop_NotifyWebShop_TerminalStatusOrder_DoesNotShip(OrderStatusEnum status)
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

        // 已作废 / 已退款订单收到有效签名回调：幂等返回成功，不发货、不累充、状态不变
        var outTradeNo = await SeedUnpaidOrderAsync(db, status);
        var result = await controller.NotifyWebShop(BuildSignedNotifyRequest(outTradeNo, 6.50m, "sign-secret"));

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.OrderId == outTradeNo);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(status, order.Status);
        Assert.Equal(0, role.AllRecharge);
        Assert.Equal(0, handler.Count("/game/api/post_mail"));
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

        var outTradeNo = await SeedUnpaidOrderAsync(db);
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
    public async Task GameWebShop_NotifyWebShop_ValidatesConfigurationWithoutLegacyAppContext()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);
        db.AttachContext(controller, includeApp: false);

        var result = await controller.NotifyWebShop(new TenantGameWebShopEntityBuyNotifyRequest { OutTradeNo = "unknown-order", TradeStatus = "TRADE_SUCCESS", });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)AreaErrorCode.AreaCannotBuy, ClientApiResultReader.Code(root));
        Assert.Contains("联系管理员", ClientApiResultReader.Message(root));
    }

    // 入站回调测试使用 Buy 同口径的预创建订单快照（Content 含 RewardsItem）。
    private static async Task<string> SeedUnpaidOrderAsync(ClientApiTestDb db, OrderStatusEnum status = OrderStatusEnum.Unpaid)
    {
        const string orderId = "800000000000001";
        await db.MainDb.Insertable(new TenantGameOrderEntity
        {
            Id = 800000000000001,
            OrderId = orderId,
            RoleId = "3001",
            AreaId = 2001,
            Sku = 5001,
            AccountId = "1001",
            Amount = 650,
            Status = status,
            Content = JsonHelper.Serialize(new PlayerCreateOrderResponse
            {
                RoleId = 3001,
                Sku = 5001,
                Amount = 650,
                IsNeedPay = true,
                RewardsItem = new List<PropertyItemResponse>
                {
                    new() { Id = 11, ItemId = 11, Count = 2, },
                },
            }),
            Channel = "聚支付平台",
            PayChannel = "网页支付",
            Platform = "web",
            AdventureSuffix = string.Empty,
        }).ExecuteCommandAsync();
        return orderId;
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
            typeof(TenantGameGoodsEntity),
            typeof(SystemConfigEntity));
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

    private static string WebShopConfigJson(string notifyUrl = "https://client.example.test/api/GameWebShop/NotifyWebShop")
    {
        return JsonHelper.Serialize(new GameWebConfig
        {
            Pid = "pid",
            Type = "alipay",
            Sign = "sign-secret",
            NotifyUrl = notifyUrl,
            ReturnUrl = "https://client.example.test/return",
            WebShopTitle = "Gift",
            WebShopSubTitle = "Gift sub",
            WebShopContent = "Gift content",
        });
    }
}
