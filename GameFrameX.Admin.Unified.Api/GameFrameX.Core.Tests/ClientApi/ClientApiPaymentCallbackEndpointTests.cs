using GameFrameX.Client.Api.Services.PayVerification;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using GameFrameX.Application.Game.Order.GameOrder;
using GameFrameX.Client.Api.Integrations.Payment.Dto.Google;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Json;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.AliPay;
using GameFrameX.Payment.Apple.AppStoreServer.Models;
using GameFrameX.Payment.Apple.Option;
using GameFrameX.Payment.Google.Option;
using GameFrameX.Payment.Xsolla.Option;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiPaymentCallbackEndpointTests
{
    [Fact]
    public async Task GameOrder_NotifyAliPay_ValidSignatureShipsOrderOnce()
    {
        using var db = new ClientApiTestDb();
        SeedOrderGraph(db, 7001, "alipay");
        using var rsa = RSA.Create(2048);
        var factory = CallbackPaymentFactory.WithAliPay(Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo()));
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler, factory);
        AttachQuery(controller, db, CreateAliPayNotify(7001, "6.50", rsa));

        var result = await controller.NotifyAliPay();

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == 7001);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal(650, role.AllRecharge);
        Assert.Equal(1, handler.Count("/game/api/req_player_send_item"));
    }

    [Fact]
    public async Task GameOrder_NotifyAliPay_RejectsInvalidSignature()
    {
        using var db = new ClientApiTestDb();
        SeedOrderGraph(db, 7001, "alipay");
        using var rsa = RSA.Create(2048);
        var query = CreateAliPayNotify(7001, "6.50", rsa);
        using var wrongRsa = RSA.Create(2048);
        query["sign"] = SignAliPay(query, wrongRsa);
        var controller = CreateController(db, ClientApiRecordingHttpHandler.WithGameServerDefaults(), CallbackPaymentFactory.WithAliPay(Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo())));
        AttachQuery(controller, db, query);

        var result = await controller.NotifyAliPay();

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == 7001);
        Assert.Equal((int)AuthErrorCode.SignatureVerifyFailed, ClientApiResultReader.Code(root));
        Assert.Equal("签名验证失败", ClientApiResultReader.Message(root));
        Assert.Equal(OrderStatusEnum.Unpaid, order.Status);
    }

    [Fact]
    public async Task GameOrder_NotifyXsolla_ValidSignatureShipsOrderOnce()
    {
        using var db = new ClientApiTestDb();
        SeedOrderGraph(db, 7001, "xsolla", "9007001");
        var secret = "xsolla-secret";
        var body = JsonSerializer.Serialize(new
        {
            notification_type = "order_paid",
            order = new { id = 9007001, mode = "default", amount = "6.50", },
            user = new { external_id = "3001", },
        });
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler, CallbackPaymentFactory.WithXsolla(secret));
        AttachBody(controller, db, body);
        controller.Request.Headers.Authorization = "Signature " + Sha1Hex($"{body}{secret}", Encoding.ASCII);

        var result = await controller.NotifyXsolla();

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == 7001);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal("9007001", order.ProviderTransactionId);
    }

    [Fact]
    public async Task GameOrder_NotifyXsolla_RejectsMissingSignature()
    {
        using var db = new ClientApiTestDb();
        SeedOrderGraph(db, 7001, "xsolla", "9007001");
        var body = JsonSerializer.Serialize(new { notification_type = "order_paid", });
        var controller = CreateController(db, ClientApiRecordingHttpHandler.WithGameServerDefaults(), CallbackPaymentFactory.WithXsolla("secret"));
        AttachBody(controller, db, body);

        var result = await controller.NotifyXsolla();

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(400, controller.Response.StatusCode);
        Assert.NotEqual(0, ClientApiResultReader.Code(root));
    }

    [Fact]
    public async Task GameOrder_NotifyGoogle_ValidAuthenticatedPushShipsOrderOnce()
    {
        using var db = new ClientApiTestDb();
        SeedOrderGraph(db, 7001, "google");
        var body = CreateGoogleOneTimeProductPushBody();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var purchaseService = new TestGooglePlayPurchaseService(new GooglePlayPurchaseVerificationResponse
        {
            Setting = new GooglePaymentOptionSetting(),
            Purchase = new ProductPurchase
            {
                OrderId = "google-provider-order-1",
                ObfuscatedExternalAccountId = "7001",
                PurchaseState = 0,
                PurchaseType = 1,
                AcknowledgementState = 1,
            },
        });
        var controller = CreateController(
            db,
            handler,
            CallbackPaymentFactory.WithGoogle(),
            new TestGooglePubSubPushAuthenticator(true),
            purchaseService);
        AttachBody(controller, db, body);
        controller.Request.Headers.Authorization = "Bearer valid";

        var result = await controller.NotifyGoogle();

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == 7001);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal("google-provider-order-1", order.ProviderTransactionId);
    }

    [Fact]
    public async Task GameOrder_NotifyGoogle_RejectsInvalidPushJwt()
    {
        using var db = new ClientApiTestDb();
        SeedOrderGraph(db, 7001, "google");
        var controller = CreateController(
            db,
            ClientApiRecordingHttpHandler.WithGameServerDefaults(),
            CallbackPaymentFactory.WithGoogle(),
            new TestGooglePubSubPushAuthenticator(false));
        AttachBody(controller, db, CreateGoogleOneTimeProductPushBody());

        var result = await controller.NotifyGoogle();

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(401, controller.Response.StatusCode);
        Assert.NotEqual(0, ClientApiResultReader.Code(root));
    }

    [Fact]
    public async Task GameOrder_NotifyApple_ValidJwsShipsOrderOnce()
    {
        using var db = new ClientApiTestDb();
        SeedOrderGraph(db, 7001, "apple");
        using var appleMaterial = AppleJwsTestMaterial.Create();
        var appAccountToken = LongToGuidExact(7001).ToString();
        var transactionJws = appleMaterial.Sign(new JWSTransactionDecodedPayload
        {
            TransactionId = "apple-transaction-1",
            OriginalTransactionId = "apple-original-1",
            AppAccountToken = appAccountToken,
            ProductId = "9001",
            Environment = ApplePayEnvironment.Production,
        });
        var signedPayload = appleMaterial.Sign(new ResponseBodyV2DecodedPayload
        {
            NotificationType = NotificationTypeV2.ONE_TIME_CHARGE,
            NotificationUUID = "c108-apple-notification",
            Version = "2.0",
            Data = new Data
            {
                BundleId = "com.gameframex.c108",
                Environment = ApplePayEnvironment.Production,
                SignedTransactionInfo = transactionJws,
            },
        });
        var body = JsonHelper.Serialize(new ResponseBodyV2 { SignedPayload = signedPayload, });
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler, CallbackPaymentFactory.WithApple(appleMaterial.RootCertificatePem));
        AttachBody(controller, db, body);

        var result = await controller.NotifyApple();

        var root = ClientApiResultReader.Read(result);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == 7001);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal("apple-transaction-1", order.AppleTransactionId);
    }

    [Fact]
    public async Task GameOrder_NotifyApple_RejectsInvalidBody()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db, ClientApiRecordingHttpHandler.WithGameServerDefaults(), CallbackPaymentFactory.WithApple("invalid-root"));
        AttachBody(controller, db, "{}");

        var result = await controller.NotifyApple();

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(400, controller.Response.StatusCode);
        Assert.NotEqual(0, ClientApiResultReader.Code(root));
    }

    private static GameOrderController CreateController(
        ClientApiTestDb db,
        ClientApiRecordingHttpHandler handler,
        IPaymentChannelFactory factory,
        IGooglePubSubPushAuthenticator? authenticator = null,
        IGooglePlayPurchaseService? googlePurchaseService = null)
    {
        db.InitTenantTables(typeof(TenantGameAreaEntity), typeof(TenantGameRoleEntity), typeof(TenantGameOrderEntity), typeof(TenantGameOrderFulfillmentEntity));
#pragma warning disable CS8625 // 测试桩：默认未配置 Google 购买验证服务，模拟 null 结果
        return new GameOrderController(
            db.Repository<TenantGameOrderEntity>(),
            FakeOptionsProvider.WithAreaPolicy(false),
            db.Repository<TenantGameRoleEntity>(),
            new HttpClient(handler, false),
            new TenantGameOrderShipService(db.TenantDb, new ClientApiTestHttpClientFactory(handler)),
            factory,
            authenticator ?? new TestGooglePubSubPushAuthenticator(false),
            googlePurchaseService ?? new TestGooglePlayPurchaseService(null));
#pragma warning restore CS8625
    }

    private static void SeedOrderGraph(ClientApiTestDb db, long orderId, string payChannel, string? providerOrderId = null)
    {
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedOrder(orderId, 3001, 2001, payChannel, OrderStatusEnum.Unpaid, 650, 9001, orderProviderId: providerOrderId);
    }

    private static void AttachQuery(GameOrderController controller, ClientApiTestDb db, Dictionary<string, string> query)
    {
        var context = db.CreateHttpContext();
        context.Response.Body = new MemoryStream();
#pragma warning disable CS8620 // 测试输入 Dictionary<string,string> 与 QueryString.Create 期望的 string? 仅可空性差异，运行时等价
        context.Request.QueryString = QueryString.Create(query);
#pragma warning restore CS8620
        controller.ControllerContext = new ControllerContext { HttpContext = context, };
    }

    private static void AttachBody(GameOrderController controller, ClientApiTestDb db, string body)
    {
        var context = db.CreateHttpContext();
        context.Response.Body = new MemoryStream();
        var bytes = Encoding.UTF8.GetBytes(body);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        context.Request.ContentType = "application/json";
        controller.ControllerContext = new ControllerContext { HttpContext = context, };
    }

    private static Dictionary<string, string> CreateAliPayNotify(long orderId, string amountYuan, RSA rsa)
    {
        var query = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["out_trade_no"] = orderId.ToString(),
            ["trade_no"] = "ali-trade-1",
            ["trade_status"] = "TRADE_SUCCESS",
            ["total_amount"] = amountYuan,
            ["sign_type"] = "RSA2",
        };
        query["sign"] = SignAliPay(query, rsa);
        return query;
    }

    private static string SignAliPay(IDictionary<string, string> query, RSA rsa)
    {
        var raw = string.Join("&", query
                                   .Where(item => item.Key != "sign" && item.Key != "sign_type" && !string.IsNullOrEmpty(item.Value))
                                   .OrderBy(item => item.Key, StringComparer.Ordinal)
                                   .Select(item => $"{item.Key}={item.Value}"));
        return Convert.ToBase64String(rsa.SignData(Encoding.UTF8.GetBytes(raw), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
    }

    private static string CreateGoogleOneTimeProductPushBody()
    {
        var notification = new GoogleWebHookInputGooglePlayNotification
        {
            Version = "1.0",
            PackageName = "com.gameframex.c108",
            EventTimeMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            OneTimeProductNotification = new GoogleWebHookInputOneTimeProductNotification
            {
                Version = "1.0",
                NotificationType = 1,
                PurchaseToken = "purchase-token-1",
                Sku = "9001",
            },
        };
        var dataJson = JsonHelper.Serialize(notification);
        return JsonHelper.Serialize(new GoogleWebHookRequest
        {
            Message = new GoogleWebHookInputMessage
            {
                Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(dataJson)),
                messageId = "message-1",
            },
            Subscription = "subscriptions/c108",
        });
    }

    private static string Sha1Hex(string value, Encoding encoding)
    {
        return Convert.ToHexString(SHA1.HashData(encoding.GetBytes(value))).ToLowerInvariant();
    }

    private static Guid LongToGuidExact(long value)
    {
        var valueBytes = BitConverter.GetBytes(value);
        var guidBytes = new byte[16];
        guidBytes[3] = valueBytes[0];
        guidBytes[2] = valueBytes[1];
        guidBytes[1] = valueBytes[2];
        guidBytes[0] = valueBytes[3];
        guidBytes[5] = valueBytes[4];
        guidBytes[4] = valueBytes[5];
        guidBytes[7] = valueBytes[6];
        guidBytes[6] = valueBytes[7];
        return new Guid(guidBytes);
    }

    private sealed class CallbackPaymentFactory : IPaymentChannelFactory
    {
        private readonly Dictionary<string, IPaymentOptionSetting[]> _settings;

        private CallbackPaymentFactory(Dictionary<string, IPaymentOptionSetting[]> settings)
        {
            _settings = settings;
        }

        public static CallbackPaymentFactory WithAliPay(string publicKey)
        {
            return new CallbackPaymentFactory(new Dictionary<string, IPaymentOptionSetting[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["alipay"] = new IPaymentOptionSetting[] { new AliPayOptionSetting { AliPayPublicKey = publicKey, AliPayCharset = "UTF-8", AliPaySignType = "RSA2", }, },
            });
        }

        public static CallbackPaymentFactory WithXsolla(string secret)
        {
            return new CallbackPaymentFactory(new Dictionary<string, IPaymentOptionSetting[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["xsolla"] = new IPaymentOptionSetting[] { new XsollaPayOptionSetting { XsollaPaymentWebHookSecretKey = secret, }, },
            });
        }

        public static CallbackPaymentFactory WithGoogle()
        {
            return new CallbackPaymentFactory(new Dictionary<string, IPaymentOptionSetting[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["google"] = new IPaymentOptionSetting[] { new GooglePaymentOptionSetting { GooglePaymentPubSubPushAudience = "audience", GooglePaymentPubSubPushServiceAccountEmail = "push@example.test", }, },
            });
        }

        public static CallbackPaymentFactory WithApple(string rootCertificatePem)
        {
            return new CallbackPaymentFactory(new Dictionary<string, IPaymentOptionSetting[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["apple"] = new IPaymentOptionSetting[] { new ApplePaymentOptionSetting { ApplePaymentRootCertificate = rootCertificatePem, ApplePaymentEnvironment = "production", }, },
            });
        }

        public IPaymentChannelHandler GetHandler(string channelName)
        {
#pragma warning disable CS8603 // 测试桩：本工厂只为回调路由注册 OptionSetting，不返回 Handler
            return null;
#pragma warning restore CS8603
        }

        public bool IsSupported(string channelName)
        {
            return _settings.ContainsKey(channelName);
        }

        public IEnumerable<string> GetSupportedChannels()
        {
            return _settings.Keys;
        }

        public IPaymentOptionSetting GetOptionSetting(string channelName)
        {
#pragma warning disable CS8603 // 测试桩：FirstOrDefault 未命中返回 null，与 IPaymentOptionSettingProvider 契约一致
            return GetOptionSettings(channelName).FirstOrDefault();
#pragma warning restore CS8603
        }

        public IEnumerable<IPaymentOptionSetting> GetOptionSettings(string channelName)
        {
            return _settings.TryGetValue(channelName, out var settings) ? settings : Array.Empty<IPaymentOptionSetting>();
        }
    }

    private sealed class TestGooglePubSubPushAuthenticator : IGooglePubSubPushAuthenticator
    {
        private readonly bool _result;

        public TestGooglePubSubPushAuthenticator(bool result)
        {
            _result = result;
        }

        public Task<bool> ValidateAsync(string authorizationHeader, IReadOnlyCollection<GooglePaymentOptionSetting> settings)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class TestGooglePlayPurchaseService : IGooglePlayPurchaseService
    {
        private readonly GooglePlayPurchaseVerificationResponse _result;

        public TestGooglePlayPurchaseService(GooglePlayPurchaseVerificationResponse result)
        {
            _result = result;
        }

        public Task<GooglePlayPurchaseVerificationResponse> VerifyPurchaseAsync(IReadOnlyCollection<GooglePaymentOptionSetting> settings, string packageName, string productId, string purchaseToken)
        {
            return Task.FromResult(_result);
        }

        public Task<bool> AcknowledgePurchaseAsync(GooglePaymentOptionSetting setting, string packageName, string productId, string purchaseToken, string? developerPayload = null)
        {
            return Task.FromResult(true);
        }
    }

    private sealed class AppleJwsTestMaterial : IDisposable
    {
        private readonly ECDsa _leafKey;
        private readonly X509Certificate2 _leafCertificate;
        private readonly X509Certificate2 _rootCertificate;

        private AppleJwsTestMaterial(ECDsa leafKey, X509Certificate2 leafCertificate, X509Certificate2 rootCertificate)
        {
            _leafKey = leafKey;
            _leafCertificate = leafCertificate;
            _rootCertificate = rootCertificate;
            RootCertificatePem = rootCertificate.ExportCertificatePem();
        }

        public string RootCertificatePem { get; }

        public static AppleJwsTestMaterial Create()
        {
            var now = DateTimeOffset.UtcNow;
            var rootKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var rootRequest = new CertificateRequest("CN=C108 Apple Root", rootKey, HashAlgorithmName.SHA256);
            rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.DigitalSignature, true));
            using var rootWithKey = rootRequest.CreateSelfSigned(now.AddDays(-1), now.AddDays(1));
            var rootCertificate = X509CertificateLoader.LoadCertificate(rootWithKey.Export(X509ContentType.Cert));

            var leafKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var leafRequest = new CertificateRequest("CN=C108 Apple Leaf", leafKey, HashAlgorithmName.SHA256);
            leafRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
            leafRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
            var serial = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            var leafCertificate = leafRequest.Create(rootWithKey, now.AddDays(-1), now.AddDays(1), serial);

            return new AppleJwsTestMaterial(leafKey, leafCertificate, rootCertificate);
        }

        public string Sign<T>(T payload)
        {
            var headerJson = JsonSerializer.Serialize(new
            {
                alg = "ES256",
                x5c = new[]
                {
                    Convert.ToBase64String(_leafCertificate.RawData),
                    Convert.ToBase64String(_rootCertificate.RawData),
                },
            });
            var payloadJson = JsonHelper.Serialize(payload);
            var signingInput = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(headerJson)) + "." + Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payloadJson));
            var signature = _leafKey.SignData(Encoding.UTF8.GetBytes(signingInput), HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
            return signingInput + "." + Base64UrlEncoder.Encode(signature);
        }

        public void Dispose()
        {
            _leafCertificate.Dispose();
            _rootCertificate.Dispose();
            _leafKey.Dispose();
        }
    }
}