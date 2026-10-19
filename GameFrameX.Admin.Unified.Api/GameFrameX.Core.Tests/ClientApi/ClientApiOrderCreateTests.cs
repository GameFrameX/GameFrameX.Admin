using GameFrameX.Client.Api.Services.PayVerification;
using GameFrameX.Client.Api.Integrations.Payment.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Application.Game.Order.GameOrder;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Payment.Abstract.Models;
using GameFrameX.Payment.Google.Option;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiOrderCreateTests
{
    [Fact]
    public async Task GameOrder_Create_PersistsTrustedOrderWhenPaymentChannelSucceeds()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        var factory = new TestPaymentChannelFactory(new TestPaymentChannelHandler("testpay"));
        var controller = CreateController(db, factory);
        db.AttachContext(controller);

        var result = await controller.Create(new CreateTenantGameOrderEntityRequest
        {
            RoleId = 3001,
            AreaId = 2001,
            PayChannel = "testpay",
            Platform = "ios",
            Channel = "official",
            Sku = 9001,
            Country = "CNY",
            ClientIp = "127.0.0.1",
        });

        var root = ClientApiResultReader.Read(result);
        var data = ClientApiResultReader.Data(root);
        var saved = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.RoleId == "3001");
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal("provider-order-1", data.GetProperty("OrderId").GetString());
        Assert.Equal(OrderStatusEnum.Unpaid, saved.Status);
        Assert.Equal(db.AppId, saved.AppId);
        Assert.Equal(650u, saved.Amount);
    }

    [Fact]
    public async Task GameOrder_Create_RejectsMissingAppContext()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db, new TestPaymentChannelFactory(new TestPaymentChannelHandler("testpay")));
        db.AttachContext(controller, includeApp: false);

        var result = await controller.Create(new CreateTenantGameOrderEntityRequest
        {
            RoleId = 3001,
            AreaId = 2001,
            PayChannel = "testpay",
            Platform = "ios",
            Channel = "official",
            Sku = 9001,
            Country = "CNY",
            ClientIp = "127.0.0.1",
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)AuthErrorCode.AuthorizeInvalid, ClientApiResultReader.Code(root));
        Assert.Equal("应用未认证", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameOrder_Create_RejectsUnsupportedPayChannel()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        var controller = CreateController(db, new TestPaymentChannelFactory());
        db.AttachContext(controller);

        var result = await controller.Create(new CreateTenantGameOrderEntityRequest
        {
            RoleId = 3001,
            AreaId = 2001,
            PayChannel = "unknown",
            Platform = "ios",
            Channel = "official",
            Sku = 9001,
            Country = "CNY",
            ClientIp = "127.0.0.1",
        });

        var root = ClientApiResultReader.Read(result);
        var orderCount = await db.TenantDb.Queryable<TenantGameOrderEntity>().CountAsync();
        Assert.Equal((int)PaymentErrorCode.PayChannelNotSupported, ClientApiResultReader.Code(root));
        Assert.Equal("不支持的支付渠道", ClientApiResultReader.Message(root));
        Assert.Equal(0, orderCount);
    }

    private static GameOrderController CreateController(ClientApiTestDb db, IPaymentChannelFactory factory)
    {
        db.InitTenantTables(typeof(TenantGameAreaEntity), typeof(TenantGameRoleEntity), typeof(TenantGameOrderEntity));
        return new GameOrderController(
            db.Repository<TenantGameOrderEntity>(),
            FakeOptionsProvider.WithAreaPolicy(false),
            db.Repository<TenantGameRoleEntity>(),
            new HttpClient(ClientApiRecordingHttpHandler.WithGameServerDefaults(), false),
            new TenantGameOrderShipService(db.TenantDb, new ClientApiTestHttpClientFactory(ClientApiRecordingHttpHandler.WithGameServerDefaults())),
            factory,
            new TestGooglePubSubPushAuthenticator(),
            new TestGooglePlayPurchaseService());
    }

    private sealed class TestPaymentChannelFactory : IPaymentChannelFactory
    {
        private readonly Dictionary<string, IPaymentChannelHandler> _handlers;
        private readonly Dictionary<string, IPaymentOptionSetting> _settings;

        public TestPaymentChannelFactory(params IPaymentChannelHandler[] handlers)
        {
            _handlers = handlers.ToDictionary(handler => handler.ChannelName, StringComparer.OrdinalIgnoreCase);
            _settings = handlers.ToDictionary(handler => handler.ChannelName, _ => (IPaymentOptionSetting)new TestPaymentOptionSetting(), StringComparer.OrdinalIgnoreCase);
        }

        public IPaymentChannelHandler GetHandler(string channelName)
        {
            _handlers.TryGetValue(channelName, out var handler);
#pragma warning disable CS8603 // 测试桩：未注册渠道返回 null，与接口契约 string? 语义一致
            return handler;
#pragma warning restore CS8603
        }

        public bool IsSupported(string channelName)
        {
            return _handlers.ContainsKey(channelName);
        }

        public IEnumerable<string> GetSupportedChannels()
        {
            return _handlers.Keys;
        }

        public IPaymentOptionSetting GetOptionSetting(string channelName)
        {
            _settings.TryGetValue(channelName, out var setting);
#pragma warning disable CS8603 // 测试桩：未配置渠道返回 null，与接口契约 string? 语义一致
            return setting;
#pragma warning restore CS8603
        }

        public IEnumerable<IPaymentOptionSetting> GetOptionSettings(string channelName)
        {
            var setting = GetOptionSetting(channelName);
            return setting == null ? Array.Empty<IPaymentOptionSetting>() : new[] { setting, };
        }
    }

    private sealed class TestPaymentChannelHandler : IPaymentChannelHandler
    {
        public TestPaymentChannelHandler(string channelName)
        {
            ChannelName = channelName;
        }

        public string ChannelName { get; }

        public Task<PaymentChannelCreateOrderResponse> CreateOrderAsync(IGameOrderBase entity, object input, object user, HttpClient httpClient, IPaymentOptionSetting optionSetting)
        {
            return Task.FromResult(PaymentChannelCreateOrderResponse.Success(
                                       new GameOrderBase
                                       {
                                           Id = entity.Id,
                                           RoleId = entity.RoleId,
                                           AccountId = entity.AccountId,
                                           AreaId = entity.AreaId,
                                           Sku = entity.Sku,
                                           OrderId = "provider-order-1",
                                           Country = entity.Country,
                                           ClientIp = entity.ClientIp,
                                           IsSandbox = entity.IsSandbox,
                                           Amount = entity.Amount,
                                       },
                                       new TestPaymentCreateOrderResponse
                                       {
                                           IsNeedPay = true,
                                           Sku = entity.Sku,
                                           RoleId = entity.RoleId,
                                           OrderId = "provider-order-1",
                                       }));
        }

        public Task<PaymentChannelVerifyReceiptResponse> VerifyReceiptAsync(IGameOrderBase entity, GameFrameX.Payment.Abstract.Interface.ITransactionData transactionData, HttpClient httpClient, IPaymentOptionSetting optionSetting)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class TestPaymentCreateOrderResponse : BaseCreateOrderResponse
    {
    }

    private sealed class TestPaymentOptionSetting : IPaymentOptionSetting
    {
    }

    private sealed class TestGooglePubSubPushAuthenticator : IGooglePubSubPushAuthenticator
    {
        public Task<bool> ValidateAsync(string authorizationHeader, IReadOnlyCollection<GooglePaymentOptionSetting> settings)
        {
            return Task.FromResult(false);
        }
    }

    private sealed class TestGooglePlayPurchaseService : IGooglePlayPurchaseService
    {
        public Task<GooglePlayPurchaseVerificationResponse> VerifyPurchaseAsync(IReadOnlyCollection<GooglePaymentOptionSetting> settings, string packageName, string productId, string purchaseToken)
        {
#pragma warning disable CS8625 // 测试桩：模拟验证未命中，返回 null 结果
            return Task.FromResult<GooglePlayPurchaseVerificationResponse>(null);
#pragma warning restore CS8625
        }

        public Task<bool> AcknowledgePurchaseAsync(GooglePaymentOptionSetting setting, string packageName, string productId, string purchaseToken, string? developerPayload = null)
        {
            return Task.FromResult(false);
        }
    }
}