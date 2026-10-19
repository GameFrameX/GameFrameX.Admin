// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using GameFrameX.Application.Game.Order.GameOrder;
using GameFrameX.Client.Api.Integrations.Payment;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.PayVerification;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Google.Option;
using GameFrameX.Payment.Wechat;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// 微信支付回调信任链测试：验签（平台证书）→ 资源解密（APIv3 密钥）→ 金额校验 → 原子认领 → 幂等发货 → 微信规范应答。
/// 与支付宝回调用例同构（ClientApiPaymentCallbackEndpointTests）。
/// </summary>
public sealed class ClientApiWeChatPayCallbackTests
{
    /// <summary>
    /// 固定商户配置签名（商户号|应用ID|证书序列号）：四个用例共享同一 WechatTenpayClientFactory 缓存实例，
    /// 平台证书只在首个用例注册一次，后续用例复用（模拟生产常驻进程行为）
    /// </summary>
    private const string TestMerchantId = "1900000109";
    private const string TestAppId = "wx4a4b5c6d7e8f9012";
    private const string TestMerchantCertSerial = "C108-WECHAT-MERCHANT-SERIAL";
    private const string TestTransactionId = "4200001234567890123456789012";

    [Fact]
    public async Task GameOrder_NotifyWeChatPay_ValidSignatureShipsOrderOnce()
    {
        using var db = new ClientApiTestDb();
        SeedOrderGraph(db, 7101);
        using var material = WechatPayCallbackTestMaterial.Create();
        var setting = material.CreateOptionSetting();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler, setting);
        AttachWechatCallback(controller, db, material.BuildSuccessNotification(7101, 650));

        var result = await controller.NotifyWeChatPay();

        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Contains("\"code\":\"SUCCESS\"", contentResult.Content);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == 7101);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal(TestTransactionId, order.ProviderTransactionId);
        Assert.Equal(650, role.AllRecharge);
        Assert.Equal(1, handler.Count("/game/api/req_player_send_item"));
    }

    [Fact]
    public async Task GameOrder_NotifyWeChatPay_RejectsInvalidSignature()
    {
        using var db = new ClientApiTestDb();
        SeedOrderGraph(db, 7101);
        using var material = WechatPayCallbackTestMaterial.Create();
        var setting = material.CreateOptionSetting();
        var notification = material.BuildSuccessNotification(7101, 650);
        using var wrongRsa = RSA.Create(2048);
        notification = notification with { Signature = material.SignWith(wrongRsa, notification.Timestamp, notification.Nonce, notification.Body), };
        var controller = CreateController(db, ClientApiRecordingHttpHandler.WithGameServerDefaults(), setting);
        AttachWechatCallback(controller, db, notification);

        var result = await controller.NotifyWeChatPay();

        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal(500, contentResult.StatusCode);
        Assert.Contains("\"code\":\"FAIL\"", contentResult.Content);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == 7101);
        Assert.Equal(OrderStatusEnum.Unpaid, order.Status);
    }

    [Fact]
    public async Task GameOrder_NotifyWeChatPay_RejectsMismatchedAmount()
    {
        using var db = new ClientApiTestDb();
        SeedOrderGraph(db, 7101);
        using var material = WechatPayCallbackTestMaterial.Create();
        var setting = material.CreateOptionSetting();
        // 回调金额 100 分 ≠ 订单 650 分：伪造金额必须被拒
        var controller = CreateController(db, ClientApiRecordingHttpHandler.WithGameServerDefaults(), setting);
        AttachWechatCallback(controller, db, material.BuildSuccessNotification(7101, 100));

        var result = await controller.NotifyWeChatPay();

        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal(500, contentResult.StatusCode);
        Assert.Contains("\"code\":\"FAIL\"", contentResult.Content);
        var order = await db.TenantDb.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == 7101);
        Assert.Equal(OrderStatusEnum.Unpaid, order.Status);
    }

    [Fact]
    public async Task GameOrder_NotifyWeChatPay_RepeatedNotificationShipsOnce()
    {
        using var db = new ClientApiTestDb();
        SeedOrderGraph(db, 7101);
        using var material = WechatPayCallbackTestMaterial.Create();
        var setting = material.CreateOptionSetting();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler, setting);
        var notification = material.BuildSuccessNotification(7101, 650);

        AttachWechatCallback(controller, db, notification);
        var first = await controller.NotifyWeChatPay();
        AttachWechatCallback(controller, db, notification);
        var second = await controller.NotifyWeChatPay();

        // 微信重试通知：两次均 200 SUCCESS（避免继续重试），但发货与累充恰一次（ShipAsync 幂等 + 状态前置守卫）
        Assert.Contains("\"code\":\"SUCCESS\"", Assert.IsType<ContentResult>(first).Content);
        Assert.Contains("\"code\":\"SUCCESS\"", Assert.IsType<ContentResult>(second).Content);
        var role = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(650, role.AllRecharge);
        Assert.Equal(1, handler.Count("/game/api/req_player_send_item"));
    }

    private static GameOrderController CreateController(
        ClientApiTestDb db,
        ClientApiRecordingHttpHandler handler,
        WechatPayOptionSetting setting)
    {
        db.InitTenantTables(typeof(TenantGameAreaEntity), typeof(TenantGameRoleEntity), typeof(TenantGameOrderEntity), typeof(TenantGameOrderFulfillmentEntity));
#pragma warning disable CS8625 // 测试桩：微信回调链路不使用 Google 验证服务，传 null 与其它渠道回调用例一致
        return new GameOrderController(
            db.Repository<TenantGameOrderEntity>(),
            FakeOptionsProvider.WithAreaPolicy(false),
            db.Repository<TenantGameRoleEntity>(),
            new HttpClient(handler, false),
            new TenantGameOrderShipService(db.TenantDb, new ClientApiTestHttpClientFactory(handler)),
            new WechatCallbackPaymentFactory(setting),
            new TestGooglePubSubPushAuthenticator(false),
            new TestGooglePlayPurchaseService(null));
#pragma warning restore CS8625
    }

    private static void SeedOrderGraph(ClientApiTestDb db, long orderId)
    {
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedOrder(orderId, 3001, 2001, "wechat", OrderStatusEnum.Unpaid, 650, 9001);
    }

    private static void AttachWechatCallback(GameOrderController controller, ClientApiTestDb db, WechatPayCallbackNotification notification)
    {
        var context = db.CreateHttpContext();
        context.Response.Body = new MemoryStream();
        var bytes = Encoding.UTF8.GetBytes(notification.Body);
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
        context.Request.ContentType = "application/json";
        context.Request.Headers["Wechatpay-Timestamp"] = notification.Timestamp;
        context.Request.Headers["Wechatpay-Nonce"] = notification.Nonce;
        context.Request.Headers["Wechatpay-Signature"] = notification.Signature;
        context.Request.Headers["Wechatpay-Serial"] = notification.SerialNumber;
        controller.ControllerContext = new ControllerContext { HttpContext = context, };
    }

    /// <summary>
    /// 微信支付回调测试材料：自签 RSA-2048 平台证书 + APIv3 密钥 AES-GCM 资源加密 + 微信 v3 规范验签串签名。
    /// 证书通过 <see cref="WechatTenpayClientFactory"/> 的进程内缓存注册（生产路径同一入口），
    /// 首次 GetClientAsync 的平台证书下载在测试环境快速失败并被工厂放行（Timeout=15s、异常兜底）。
    /// </summary>
    private sealed class WechatPayCallbackTestMaterial : IDisposable
    {
        private const string ApiV3Secret = "0123456789abcdef0123456789abcdef"; // 32 字节 AES-256 密钥（微信 APIv3 密钥固定 32 字节）

        private readonly RSA _platformRsa;
        private readonly X509Certificate2 _platformCertificate;

        private WechatPayCallbackTestMaterial(RSA platformRsa, X509Certificate2 platformCertificate)
        {
            _platformRsa = platformRsa;
            _platformCertificate = platformCertificate;
        }

        public static WechatPayCallbackTestMaterial Create()
        {
            var now = DateTimeOffset.UtcNow;
            var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=C108 Wechat Pay Platform", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var certificate = request.CreateSelfSigned(now.AddDays(-1), now.AddDays(1));
            return new WechatPayCallbackTestMaterial(rsa, certificate);
        }

        /// <summary>
        /// 构造微信支付配置并把自签平台证书注册进工厂缓存客户端（Wechatpay-Signature 验签依赖）
        /// </summary>
        public WechatPayOptionSetting CreateOptionSetting()
        {
            var setting = new WechatPayOptionSetting
            {
                WechatPayMerchantId = TestMerchantId,
                WechatPayAppId = TestAppId,
                WechatPayApiV3Secret = ApiV3Secret,
                WechatPayCertificateSerialNumber = TestMerchantCertSerial,
                WechatPayCertificatePrivateKey = _platformRsa.ExportPkcs8PrivateKeyPem(),
                WechatPayNotifyUrl = "https://callback.example.test",
            };
            RegisterPlatformCertificate(setting).GetAwaiter().GetResult();
            return setting;
        }

        private async Task RegisterPlatformCertificate(WechatPayOptionSetting setting)
        {
            var client = await WechatTenpayClientFactory.GetClientAsync(setting);
            if (client.PlatformCertificateManager is InMemoryCertificateManager manager)
            {
                manager.AddEntry(new CertificateEntry(
                    "RSA",
                    _platformCertificate.SerialNumber,
                    _platformCertificate.ExportCertificatePem(),
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow.AddDays(1)));
            }
        }

        public WechatPayCallbackNotification BuildSuccessNotification(long orderId, int totalFen)
        {
            var resourcePlain = JsonSerializer.Serialize(new
            {
                out_trade_no = orderId.ToString(),
                transaction_id = TestTransactionId,
                trade_state = "SUCCESS",
                trade_state_description = "支付成功",
                success_time = "2026-10-05T12:00:00+08:00",
                amount = new { total = totalFen, currency = "CNY", payer_total = totalFen, payer_currency = "CNY", },
            });
            var nonceBytes = RandomNumberGenerator.GetBytes(12);
            // 微信 resource.nonce 为 12 个可见 ASCII 字符（SDK 将其 UTF8 编码后须恰为 12 字节 GCM nonce），不能用 hex 编码
            var nonce = new string(nonceBytes.Select(b => (char)('a' + b % 26)).ToArray());
            nonceBytes = Encoding.UTF8.GetBytes(nonce);
            var associatedData = Encoding.UTF8.GetBytes("transaction");
            using (var aes = new AesGcm(Encoding.UTF8.GetBytes(ApiV3Secret), 16))
            {
                var plainBytes = Encoding.UTF8.GetBytes(resourcePlain);
                var cipherBytes = new byte[plainBytes.Length];
                var tagBytes = new byte[16];
                // 命名参数消歧义：.NET 8+ 新增 nonce 在前的同形重载，位置传参会误绑
                aes.Encrypt(plaintext: plainBytes, nonce: nonceBytes, ciphertext: cipherBytes, tag: tagBytes, associatedData: associatedData);
                var cipherText = Convert.ToBase64String(cipherBytes.Concat(tagBytes).ToArray());
                var body = JsonSerializer.Serialize(new
                {
                    id = "evt-c108-test",
                    create_time = "2026-10-05T12:00:00+08:00",
                    resource_type = "encrypt-resource",
                    event_type = "TRANSACTION.SUCCESS",
                    summary = "支付成功",
                    resource = new
                    {
                        original_type = "transaction",
                        algorithm = "AEAD_AES_256_GCM",
                        ciphertext = cipherText,
                        associated_data = "transaction",
                        nonce = nonce,
                    },
                });
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                return new WechatPayCallbackNotification(
                    body,
                    timestamp,
                    nonce,
                    SignWith(_platformRsa, timestamp, nonce, body),
                    _platformCertificate.SerialNumber);
            }
        }

        /// <summary>
        /// 微信 v3 验签串：{timestamp}\n{nonce}\n{body}\n，RSA-SHA256(PKCS1) 后 Base64
        /// </summary>
        public string SignWith(RSA rsa, string timestamp, string nonce, string body)
        {
            var message = $"{timestamp}\n{nonce}\n{body}\n";
            return Convert.ToBase64String(rsa.SignData(Encoding.UTF8.GetBytes(message), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        }

        public void Dispose()
        {
            _platformCertificate.Dispose();
            _platformRsa.Dispose();
        }
    }

    private sealed record WechatPayCallbackNotification(string Body, string Timestamp, string Nonce, string Signature, string SerialNumber);

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

    private sealed class WechatCallbackPaymentFactory : IPaymentChannelFactory
    {
        private readonly WechatPayOptionSetting _setting;

        public WechatCallbackPaymentFactory(WechatPayOptionSetting setting)
        {
            _setting = setting;
        }

        public IPaymentChannelHandler GetHandler(string channelName)
        {
#pragma warning disable CS8603 // 测试桩：回调链路只消费 OptionSetting，不返回 Handler
            return null;
#pragma warning restore CS8603
        }

        public bool IsSupported(string channelName)
        {
            return string.Equals(channelName, "wechat", StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<string> GetSupportedChannels()
        {
            return new[] { "wechat", };
        }

        public IPaymentOptionSetting GetOptionSetting(string channelName)
        {
            return _setting;
        }

        public IEnumerable<IPaymentOptionSetting> GetOptionSettings(string channelName)
        {
            return IsSupported(channelName) ? new IPaymentOptionSetting[] { _setting, } : Array.Empty<IPaymentOptionSetting>();
        }
    }
}
