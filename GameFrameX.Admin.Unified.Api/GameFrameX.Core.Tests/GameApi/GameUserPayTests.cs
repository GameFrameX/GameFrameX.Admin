using GameFrameX.Client.Api.Services.PayVerification;
using GameFrameX.Client.Api.GameUserPay;
using System.Reflection;
using System.Text.Json;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Client.Api.GameUserPay.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.GameApi;

/// <summary>
/// C11 T3 RED：支付域契约（Create 下单/沙箱直发、Verify 验真三态/幂等重放、Query 双形态/Paid 判定、Report 记账容错/幂等）
/// <para>契约来源：C11 任务书（支付域 4 action）。</para>
/// </summary>
public sealed class GameUserPayTests
{
    [Fact]
    public async Task Create_RealPay_PersistsUnpaidOrder_NeedPayTrue()
    {
        using (var db = new ClientApiTestDb())
        {
            var verifier = new FakePayReceiptVerifier();
            var controller = CreateController(db, verifier);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Create(
                                                                                 MakeCreate(7001, "sku.gold", 7))));
            Assert.True(data.GetProperty("IsNeedPay").GetBoolean());
            var orderId = data.GetProperty("OrderId").GetString();
            Assert.StartsWith("pay", orderId);
            Assert.Equal("sku.gold", data.GetProperty("Sku").GetString());
            Assert.Equal(990, data.GetProperty("Amount").GetInt64());

            var row = db.TenantDb.Queryable<TenantGameOrderEntity>().Single(x => x.OrderId == orderId);
            Assert.Equal(OrderStatusEnum.Unpaid, row.Status);
            Assert.Equal("7001", row.AccountId);
            Assert.Equal("sku.gold", row.ProductId);
            Assert.Equal("7", row.PayChannel);
            Assert.Equal("1", row.Platform);
            Assert.Equal((uint)990, row.Amount);
            Assert.Null(row.ProviderTransactionId);
        }
    }

    [Fact]
    public async Task Create_SandboxPolicy_IssuesShippedOrder_NeedPayFalse_ClosedLoop()
    {
        using (var db = new ClientApiTestDb())
        {
            var policyRepository = CreatePolicyRepository(db);
            await policyRepository.InsertAsync(MakePolicy(0, true));
            var verifier = new FakePayReceiptVerifier();
            var controller = CreateController(db, verifier);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Create(
                                                                                 MakeCreate(7002, "sku.gem", 100))));
            Assert.False(data.GetProperty("IsNeedPay").GetBoolean());
            var orderId = data.GetProperty("OrderId").GetString() ?? string.Empty;

            // 沙箱直发闭环：落库即 Shipped，免支付免验真，Query 直接 Paid
            var row = db.TenantDb.Queryable<TenantGameOrderEntity>().Single(x => x.OrderId == orderId);
            Assert.Equal(OrderStatusEnum.Shipped, row.Status);

            var query = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Query(
                                                                                  new GameUserPayQueryRequest { UserId = 7002, OrderId = orderId, })));
            Assert.True(query.GetProperty("Paid").GetBoolean());

            // Verify 幂等：沙箱单已到账，不跑验真直接回 0
            var verify = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Verify(
                                                                                   new GameUserPayVerifyRequest { UserId = 7002, OrderId = orderId, PayChannel = 100, })));
            Assert.Equal(0, verify.GetProperty("Rcode").GetInt64());
            Assert.Equal(0, verifier.CallCount);
        }
    }

    [Fact]
    public async Task Create_AppLevelPolicy_OverridesTenantDefault()
    {
        using (var db = new ClientApiTestDb())
        {
            var policyRepository = CreatePolicyRepository(db);
            await policyRepository.InsertAsync(MakePolicy(0, true));
            await policyRepository.InsertAsync(MakePolicy(db.AppId, false));
            var controller = CreateController(db, new FakePayReceiptVerifier(), true);

            // 应用级（关）压过租户默认（开）——沙箱判定服务端权威按应用作用域
            var first = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Create(MakeCreate(7003, "sku.a"))));
            Assert.True(first.GetProperty("IsNeedPay").GetBoolean());

            // 应用级打开后直发（provider 请求级缓存：配置变更加载后需新请求生效，重建 controller = 新 Scoped provider）
            var appRow = db.TenantDb.Queryable<TenantClientApiPolicyEntity>().Single(x => x.AppId == db.AppId);
            appRow.EnableSandBoxRecharge = true;
            await policyRepository.UpdateAsync(appRow);
            var secondController = CreateController(db, new FakePayReceiptVerifier(), true);
            var second = ClientApiResultReader.Data(ClientApiResultReader.Read(await secondController.Create(MakeCreate(7003, "sku.a"))));
            Assert.False(second.GetProperty("IsNeedPay").GetBoolean());
        }
    }

    [Fact]
    public async Task Create_GeneratesUniqueOrderIds()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db, new FakePayReceiptVerifier());

            var first = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Create(MakeCreate(7004, "sku.a"))));
            var second = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Create(MakeCreate(7004, "sku.a"))));
            Assert.NotEqual(first.GetProperty("OrderId").GetString(), second.GetProperty("OrderId").GetString());
        }
    }

    [Fact]
    public async Task Verify_Success_MarksShipped_FillsProviderTransactionId()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreatePayRepository(db);
            await repository.InsertAsync(MakeOrder(7101, "pay_seed_v1", status: (int)OrderStatusEnum.Unpaid));
            var verifier = new FakePayReceiptVerifier
            {
                Result = GamePayVerifyResponse.Succeeded("txn_apple_1", "sku.gold", "{\"status\":0}"),
            };
            var controller = CreateController(db, verifier);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Verify(
                                                                                 new GameUserPayVerifyRequest { UserId = 7101, OrderId = "pay_seed_v1", PayChannel = 100, ReceiptData = "receipt", TransactionId = "t1", })));
            Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
            Assert.Equal("pay_seed_v1", data.GetProperty("OrderId").GetString());
            Assert.Equal("sku.gold", data.GetProperty("ProductId").GetString());
            Assert.Equal(1, verifier.CallCount);

            var row = db.TenantDb.Queryable<TenantGameOrderEntity>().Single(x => x.OrderId == "pay_seed_v1");
            Assert.Equal(OrderStatusEnum.Shipped, row.Status);
            Assert.Equal("txn_apple_1", row.ProviderTransactionId);
            Assert.Equal("{\"status\":0}", row.ReceiptJson);
        }
    }

    [Fact]
    public async Task Verify_IdempotentReplay_AlreadyShipped_ReturnsZeroWithoutVerifier()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreatePayRepository(db);
            await repository.InsertAsync(MakeOrder(7102, "pay_seed_v2", status: (int)OrderStatusEnum.Shipped));
            var verifier = new FakePayReceiptVerifier
            {
                Result = GamePayVerifyResponse.Pending("不应被调用"),
            };
            var controller = CreateController(db, verifier);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Verify(
                                                                                 new GameUserPayVerifyRequest { UserId = 7102, OrderId = "pay_seed_v2", PayChannel = 7, PurchaseToken = "tok", })));
            Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
            Assert.Equal(0, verifier.CallCount);
        }
    }

    [Fact]
    public async Task Verify_Pending_ReturnsRcode7_StatusUnchanged()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreatePayRepository(db);
            await repository.InsertAsync(MakeOrder(7103, "pay_seed_v3", status: (int)OrderStatusEnum.Unpaid));
            var verifier = new FakePayReceiptVerifier
            {
                Result = GamePayVerifyResponse.Pending("收据验证失败"),
            };
            var controller = CreateController(db, verifier);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Verify(
                                                                                 new GameUserPayVerifyRequest { UserId = 7103, OrderId = "pay_seed_v3", PayChannel = 7, PurchaseToken = "tok", })));
            Assert.Equal(7, data.GetProperty("Rcode").GetInt64());

            var row = db.TenantDb.Queryable<TenantGameOrderEntity>().Single(x => x.OrderId == "pay_seed_v3");
            Assert.Equal(OrderStatusEnum.Unpaid, row.Status);
            Assert.Null(row.ProviderTransactionId);
        }
    }

    [Fact]
    public async Task Verify_OrderMissing_ReturnsRcode60()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db, new FakePayReceiptVerifier());

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Verify(
                                                                                 new GameUserPayVerifyRequest { UserId = 7104, OrderId = "pay_nope", PayChannel = 100, ReceiptData = "r", })));
            Assert.Equal(60, data.GetProperty("Rcode").GetInt64());
            Assert.Equal("pay_nope", data.GetProperty("OrderId").GetString());
        }
    }

    [Fact]
    public async Task Verify_Rejected_ReturnsExplicitEnvelopeError_DoesNotCredit()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreatePayRepository(db);
            await repository.InsertAsync(MakeOrder(7105, "pay_seed_v4", status: (int)OrderStatusEnum.Unpaid));
            var verifier = new FakePayReceiptVerifier
            {
                Result = GamePayVerifyResponse.Rejected("支付渠道配置未加载: amazon"),
            };
            var controller = CreateController(db, verifier);

            var root = ClientApiResultReader.Read(await controller.Verify(
                                                      new GameUserPayVerifyRequest { UserId = 7105, OrderId = "pay_seed_v4", PayChannel = 66, }));
            Assert.Equal((int)PaymentErrorCode.PayChannelNotSupported, ClientApiResultReader.Code(root));
            Assert.NotEqual(0, ClientApiResultReader.Code(root));
            Assert.Equal(1, verifier.CallCount);

            var row = db.TenantDb.Queryable<TenantGameOrderEntity>().Single(x => x.OrderId == "pay_seed_v4");
            Assert.Equal(OrderStatusEnum.Unpaid, row.Status);
        }
    }

    [Fact]
    public async Task Verify_TransactionIdBoundToOtherOrder_RejectsWithoutDoubleShipping()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreatePayRepository(db);
            // 订单 A 已用 t-dup 发货并回填 ProviderTransactionId（verifyReceipt / Server API 任一路径的到账形态）
            var shipped = MakeOrder(7106, "pay_dup_a", status: (int)OrderStatusEnum.Shipped);
            shipped.ProviderTransactionId = "t-dup";
            await repository.InsertAsync(shipped);
            // 订单 B 未到账，用同一渠道交易号重复验真
            await repository.InsertAsync(MakeOrder(7106, "pay_dup_b", status: (int)OrderStatusEnum.Unpaid));
            var verifier = new FakePayReceiptVerifier
            {
                Result = GamePayVerifyResponse.Succeeded("t-dup", "sku.gold", "{\"status\":0}"),
            };
            var controller = CreateController(db, verifier);

            var root = ClientApiResultReader.Read(await controller.Verify(
                                                      new GameUserPayVerifyRequest { UserId = 7106, OrderId = "pay_dup_b", PayChannel = 100, TransactionId = "t-dup", }));
            // C271 跨订单幂等防重：信封错误拒绝，验真不执行，订单 B 不发货
            Assert.Equal((int)PaymentErrorCode.PayChannelNotSupported, ClientApiResultReader.Code(root));
            Assert.Equal(0, verifier.CallCount);

            var row = db.TenantDb.Queryable<TenantGameOrderEntity>().Single(x => x.OrderId == "pay_dup_b");
            Assert.Equal(OrderStatusEnum.Unpaid, row.Status);
            Assert.Null(row.ProviderTransactionId);
        }
    }

    [Fact]
    public async Task Query_ByOrderId_ReturnsSingleOrder()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreatePayRepository(db);
            await repository.InsertAsync(MakeOrder(7201, "pay_q1", "sku.gold", amount: 660));
            await repository.InsertAsync(MakeOrder(7201, "pay_q2", "sku.gem", amount: 1200));
            var controller = CreateController(db, new FakePayReceiptVerifier());

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Query(
                                                                                 new GameUserPayQueryRequest { UserId = 7201, OrderId = "pay_q2", })));
            Assert.Equal("pay_q2", data.GetProperty("OrderId").GetString());
            Assert.Equal("sku.gem", data.GetProperty("ProductId").GetString());
            Assert.Equal((int)OrderStatusEnum.Unpaid, data.GetProperty("Status").GetInt64());
            Assert.False(data.GetProperty("Paid").GetBoolean());
            Assert.Equal(1200, data.GetProperty("Amount").GetInt64());
        }
    }

    [Fact]
    public async Task Query_ByProductId_ReturnsLatestForUser()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreatePayRepository(db);
            await repository.InsertAsync(MakeOrder(7202, "pay_p1", "sku.gold", id: 3001));
            await repository.InsertAsync(MakeOrder(7202, "pay_p2", "sku.gold", id: 3002));
            // 其他用户的同商品订单不串
            await repository.InsertAsync(MakeOrder(7203, "pay_p3", "sku.gold", id: 3003));
            var controller = CreateController(db, new FakePayReceiptVerifier());

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Query(
                                                                                 new GameUserPayQueryRequest { UserId = 7202, ProductId = "sku.gold", })));
            Assert.Equal("pay_p2", data.GetProperty("OrderId").GetString());
        }
    }

    [Fact]
    public async Task Query_NotFound_ReturnsCode0_EmptyData()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db, new FakePayReceiptVerifier());

            var byOrderId = ClientApiResultReader.Read(await controller.Query(
                                                           new GameUserPayQueryRequest { UserId = 7204, OrderId = "pay_missing", }));
            Assert.Equal(0, ClientApiResultReader.Code(byOrderId));
            var data = ClientApiResultReader.Data(byOrderId);
            Assert.Equal(string.Empty, data.GetProperty("OrderId").GetString());
            Assert.False(data.GetProperty("Paid").GetBoolean());

            var byProductId = ClientApiResultReader.Read(await controller.Query(
                                                             new GameUserPayQueryRequest { UserId = 7204, ProductId = "sku.none", }));
            Assert.Equal(0, ClientApiResultReader.Code(byProductId));
        }
    }

    [Fact]
    public async Task Query_PaidFlag_TrueOnlyWhenShippedOrLater()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreatePayRepository(db);
            await repository.InsertAsync(MakeOrder(7205, "pay_s1", status: (int)OrderStatusEnum.Unpaid));
            await repository.InsertAsync(MakeOrder(7205, "pay_s2", status: (int)OrderStatusEnum.Shipped));
            var controller = CreateController(db, new FakePayReceiptVerifier());

            var unpaid = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Query(
                                                                                   new GameUserPayQueryRequest { UserId = 7205, OrderId = "pay_s1", })));
            Assert.False(unpaid.GetProperty("Paid").GetBoolean());

            var shipped = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Query(
                                                                                    new GameUserPayQueryRequest { UserId = 7205, OrderId = "pay_s2", })));
            Assert.True(shipped.GetProperty("Paid").GetBoolean());
        }
    }

    [Fact]
    public async Task Report_RecordsIntoExistingOrder_DoesNotAdvanceStatus()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreatePayRepository(db);
            await repository.InsertAsync(MakeOrder(7301, "pay_r1", "sku.gold", payChannel: 100, status: (int)OrderStatusEnum.Unpaid));
            var controller = CreateController(db, new FakePayReceiptVerifier());

            var root = ClientApiResultReader.Read(await controller.Report(
                                                      MakeReport(7301, "pay_r1", 990)));
            Assert.Equal(0, ClientApiResultReader.Code(root));
            // 契约：Report 响应 data 为 null
            Assert.Equal(JsonValueKind.Null, ClientApiResultReader.Data(root).ValueKind);

            var row = db.TenantDb.Queryable<TenantGameOrderEntity>().Single(x => x.OrderId == "pay_r1");
            Assert.Equal(990, row.RealMoney);
            Assert.Equal("USD", row.Currency);
            Assert.Equal(12, row.Level);
            Assert.Equal("shop", row.StrScene);
            // 纯记账：订单状态与渠道字段不被推进
            Assert.Equal(OrderStatusEnum.Unpaid, row.Status);
            Assert.Equal("100", row.PayChannel);
            Assert.Equal(1, db.TenantDb.Queryable<TenantGameOrderEntity>().Count(x => x.OrderId == "pay_r1"));
        }
    }

    [Fact]
    public async Task Report_UnknownOrder_ToleratedAndRecorded()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db, new FakePayReceiptVerifier());

            var root = ClientApiResultReader.Read(await controller.Report(
                                                      MakeReport(7302, "pay_unknown", 199)));
            Assert.Equal(0, ClientApiResultReader.Code(root));

            var row = db.TenantDb.Queryable<TenantGameOrderEntity>().Single(x => x.OrderId == "pay_unknown");
            Assert.Equal("7302", row.AccountId);
            Assert.Equal("sku.gem", row.ProductId);
            Assert.Equal(199, row.RealMoney);
            Assert.Equal("7", row.PayChannel);
            // 容错记账行不冒充已到账
            Assert.Equal(OrderStatusEnum.Unpaid, row.Status);
        }
    }

    [Fact]
    public async Task Report_IdempotentOverwrite_SameOrderIdSingleRow()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreatePayRepository(db);
            await repository.InsertAsync(MakeOrder(7303, "pay_r2", "sku.gold"));
            var controller = CreateController(db, new FakePayReceiptVerifier());

            await controller.Report(MakeReport(7303, "pay_r2", 990));
            await controller.Report(MakeReport(7303, "pay_r2", 880));

            Assert.Equal(1, db.TenantDb.Queryable<TenantGameOrderEntity>().Count(x => x.OrderId == "pay_r2"));
            var row = db.TenantDb.Queryable<TenantGameOrderEntity>().Single(x => x.OrderId == "pay_r2");
            Assert.Equal(880, row.RealMoney);
        }
    }

    [Fact]
    public void Controller_HasExplicitRouteAndHttpPostVerbs()
    {
        Assert.Equal("api/[controller]/[action]",
                     typeof(GameUserPayController).GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(typeof(GameUserPayController).GetMethod("Create")!.GetCustomAttribute<HttpPostAttribute>());
        Assert.NotNull(typeof(GameUserPayController).GetMethod("Verify")!.GetCustomAttribute<HttpPostAttribute>());
        Assert.NotNull(typeof(GameUserPayController).GetMethod("Query")!.GetCustomAttribute<HttpPostAttribute>());
        Assert.NotNull(typeof(GameUserPayController).GetMethod("Report")!.GetCustomAttribute<HttpPostAttribute>());
    }

    private static GameUserPayController CreateController(ClientApiTestDb db, FakePayReceiptVerifier verifier, bool withAppContext = false)
    {
        db.InitTenantTables(typeof(TenantGameOrderEntity), typeof(TenantClientApiPolicyEntity));
        var controller = new GameUserPayController(CreatePayRepository(db), CreateOptionsProvider(db, withAppContext), verifier);
        if (withAppContext)
        {
            // Items 键与 ClientApiAppContext.AppIdKey / TenantContextAccessor.TenantIdKey 同串
            controller.ControllerContext = new ControllerContext { HttpContext = db.CreateHttpContext(true, true), };
        }

        return controller;
    }

    /// <summary>
    /// 真 Provider（C378 起沙箱判定经 IClientApiTenantOptionsProvider 读策略合并表，不再直连仓储）
    /// </summary>
    private static IClientApiTenantOptionsProvider CreateOptionsProvider(ClientApiTestDb db, bool withAppContext)
    {
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        var accessor = new HttpContextAccessor { HttpContext = db.CreateHttpContext(true, withAppContext), };
        return new ClientApiTenantOptionsProvider(tenantDbProvider, accessor);
    }

    private static Repository<TenantGameOrderEntity> CreatePayRepository(ClientApiTestDb db)
    {
        db.InitTenantTables(typeof(TenantGameOrderEntity));
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        return new Repository<TenantGameOrderEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
    }

    private static Repository<TenantClientApiPolicyEntity> CreatePolicyRepository(ClientApiTestDb db)
    {
        db.InitTenantTables(typeof(TenantClientApiPolicyEntity));
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        return new Repository<TenantClientApiPolicyEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
    }

    private static GameUserPayCreateRequest MakeCreate(long userId, string sku, int payChannel = 100)
    {
        return new GameUserPayCreateRequest
        {
            UserId = userId,
            Sku = sku,
            PayChannel = payChannel,
            Platform = 1,
            Channel = 2,
            Country = "US",
            Amount = 990,
            ClientIp = "127.0.0.1",
        };
    }

    private static GameUserPayReportRequest MakeReport(long userId, string orderId, long realMoney)
    {
        return new GameUserPayReportRequest
        {
            UserId = userId,
            OrderId = orderId,
            RealMoney = realMoney,
            Currency = "USD",
            ProductId = "sku.gem",
            Level = 12,
            PayChannel = 7,
            StrScene = "shop",
            Channel = 2,
        };
    }

    private static TenantGameOrderEntity MakeOrder(long userId, string orderId, string sku = "sku.gold",
        int status = (int)OrderStatusEnum.Unpaid, long? amount = 990, int payChannel = 100, long id = 0)
    {
        return new TenantGameOrderEntity
        {
            Id = id,
            AccountId = userId.ToString(),
            RoleId = userId.ToString(),
            AreaId = 0,
            OrderId = orderId,
            Sku = 0,
            ProductId = sku,
            Status = (OrderStatusEnum)status,
            Amount = amount.HasValue && amount.Value > 0 ? (uint)amount.Value : 0,
            PayChannel = payChannel.ToString(),
            Platform = "1",
            Channel = string.Empty,
            SubChannel = string.Empty,
            Country = "US",
            Content = string.Empty,
            AdventureSuffix = string.Empty,
        };
    }

    private static TenantClientApiPolicyEntity MakePolicy(long appId, bool enableSandbox)
    {
        return new TenantClientApiPolicyEntity
        {
            AppId = appId,
            EnableSandBoxRecharge = enableSandbox,
            IsEnabled = true,
        };
    }

    /// <summary>
    /// 验真器假实现（成功 / pending / 拒绝三态由 Result 注入；CallCount 锁幂等不重跑）
    /// </summary>
    private sealed class FakePayReceiptVerifier : IPayReceiptVerifier
    {
        public int CallCount { get; private set; }

        public GamePayVerifyResponse Result { get; set; } = GamePayVerifyResponse.Pending("默认 pending");

        public Task<GamePayVerifyResponse> VerifyAsync(TenantGameOrderEntity order, GameUserPayVerifyRequest request)
        {
            CallCount++;
            return Task.FromResult(Result);
        }
    }
}