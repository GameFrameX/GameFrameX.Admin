using GameFrameX.Client.Api.Services.PayVerification;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Json;
using GameFrameX.Client.Api.GameUserPay.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Entity.Enum;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Payment.Apple;
using GameFrameX.Payment.Apple.AppStoreServer;
using GameFrameX.Payment.Apple.AppStoreServer.Models;
using GameFrameX.Payment.Apple.Models;
using GameFrameX.Payment.Apple.Models.Verification;
using GameFrameX.Payment.Apple.Option;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GameFrameX.Core.Tests.Payment;

/// <summary>
/// C271 Apple 支付验证双轨测试：
/// <para>Handler 层（Server API 新路径成功映射 / 校验链 / fail-closed / 双轨路由）、
/// Verifier 层（双形态路由判定 / fail-closed 前置 / 开关）、
/// CreateBearerToken 前置修复回归（20 分钟有效期 + claims）。</para>
/// </summary>
public sealed class AppleServerApiVerifyTests
{
    private const string ProductionBaseUrl = "https://api.storekit.itunes.apple.com";
    private const string SandboxBaseUrl = "https://api.storekit-sandbox.itunes.apple.com";

    // ---------- Handler 层：Server API 新路径 ----------

    [Fact]
    public async Task ServerApi_TransactionIdForm_Succeeds_And_MapsUnifiedResult()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            handler.SetTransactionInfoResponse("t-271", material.Sign(MakePayload()));
            var setting = MakeSetting(material);
            using (var httpClient = new HttpClient(handler, false))
            {
                var result = await new ApplePaymentHandler().VerifyReceiptAsync(
                                 MakeOrderEntity(), MakeTransactionData(), httpClient, setting);

                Assert.True(result.IsSuccess, result.ErrorMessage);
                var apple = Assert.IsType<AppleVerifyReceiptResponse>(result.Output);
                Assert.True(apple.IsSuccess);
                // 等价字段映射（对齐旧路径 AppleVerifyReceiptResponse）
                Assert.Equal("sku.gold", apple.ProductId);
                Assert.Equal("sku.gold", apple.Sku);
                Assert.Equal("t-271", apple.TransactionId);
                Assert.Equal("t-271", apple.ChannelTransactionId);
                Assert.Equal("t-271", apple.IdempotencyKey);
                Assert.Equal("o-271", apple.OriginalTransactionId);
                Assert.Equal(1717171717000, apple.PurchaseDateMs);
                Assert.Equal(2717171717000, apple.ExpiresDateMs);
                Assert.Equal(2, apple.Quantity);
                Assert.Equal("com.gameframex.c271", apple.BundleId);
                Assert.Equal("USD", apple.Currency);
                Assert.False(apple.IsSandbox);
                Assert.Equal("Production", apple.Environment);
                Assert.Equal("pay-271", apple.MerchantOrderId);

                // URL base 路由：production 配置打 production base + GetTransactionInfo 路径
                var url = Assert.Single(handler.RequestUrls);
                Assert.StartsWith(ProductionBaseUrl + "/inApps/v1/transactions/t-271", url, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public async Task ServerApi_SandboxSetting_RoutesToSandboxBaseUrl()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            handler.SetTransactionInfoResponse("t-271", material.Sign(MakePayload(environment: ApplePayEnvironment.Sandbox)));
            var setting = MakeSetting(material, "sandbox");
            using (var httpClient = new HttpClient(handler, false))
            {
                var entity = MakeOrderEntity(true);
                var result = await new ApplePaymentHandler().VerifyReceiptAsync(entity, MakeTransactionData(), httpClient, setting);

                Assert.True(result.IsSuccess, result.ErrorMessage);
                Assert.True(Assert.IsType<AppleVerifyReceiptResponse>(result.Output).IsSandbox);
                var url = Assert.Single(handler.RequestUrls);
                Assert.StartsWith(SandboxBaseUrl + "/inApps/v1/transactions/t-271", url, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public async Task ServerApi_EnvironmentMismatch_Fails()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            // 订单非沙盒，但交易来自 Sandbox
            handler.SetTransactionInfoResponse("t-271", material.Sign(MakePayload(environment: ApplePayEnvironment.Sandbox)));
            using (var httpClient = new HttpClient(handler, false))
            {
                var result = await new ApplePaymentHandler().VerifyReceiptAsync(MakeOrderEntity(), MakeTransactionData(), httpClient, MakeSetting(material));

                Assert.False(result.IsSuccess);
                Assert.Contains("环境不匹配", result.ErrorMessage);
            }
        }
    }

    [Fact]
    public async Task ServerApi_TransactionIdMismatch_Fails()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            handler.SetTransactionInfoResponse("t-271", material.Sign(MakePayload("t-other")));
            using (var httpClient = new HttpClient(handler, false))
            {
                var result = await new ApplePaymentHandler().VerifyReceiptAsync(MakeOrderEntity(), MakeTransactionData(), httpClient, MakeSetting(material));

                Assert.False(result.IsSuccess);
                Assert.Contains("交易ID不匹配", result.ErrorMessage);
            }
        }
    }

    [Fact]
    public async Task ServerApi_BundleIdMismatch_Fails()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            handler.SetTransactionInfoResponse("t-271", material.Sign(MakePayload(bundleId: "com.other.app")));
            using (var httpClient = new HttpClient(handler, false))
            {
                var result = await new ApplePaymentHandler().VerifyReceiptAsync(MakeOrderEntity(), MakeTransactionData(), httpClient, MakeSetting(material));

                Assert.False(result.IsSuccess);
                Assert.Contains("Bundle ID不匹配", result.ErrorMessage);
            }
        }
    }

    [Fact]
    public async Task ServerApi_ProductIdMismatch_Fails()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            handler.SetTransactionInfoResponse("t-271", material.Sign(MakePayload(productId: "sku.other")));
            using (var httpClient = new HttpClient(handler, false))
            {
                var result = await new ApplePaymentHandler().VerifyReceiptAsync(MakeOrderEntity(), MakeTransactionData(), httpClient, MakeSetting(material));

                Assert.False(result.IsSuccess);
                Assert.Contains("产品ID不匹配", result.ErrorMessage);
            }
        }
    }

    [Fact]
    public async Task ServerApi_MissingConfig_FailsClosed_WithoutHttpCall()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            var setting = MakeSetting(material);
            setting.ApplePaymentPrivateKey = string.Empty;
            using (var httpClient = new HttpClient(handler, false))
            {
                var result = await new ApplePaymentHandler().VerifyReceiptAsync(MakeOrderEntity(), MakeTransactionData(), httpClient, setting);

                // fail-closed：缺配置明确失败，禁止降级旧路径（消息声明），且零外呼
                Assert.False(result.IsSuccess);
                Assert.Contains("配置缺失", result.ErrorMessage);
                Assert.Contains("禁止降级旧路径", result.ErrorMessage);
                Assert.Empty(handler.RequestUrls);
            }
        }
    }

    [Fact]
    public async Task ServerApi_InvalidJws_Fails()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            handler.SetTransactionInfoResponse("t-271", "tampered.jws.value");
            using (var httpClient = new HttpClient(handler, false))
            {
                var result = await new ApplePaymentHandler().VerifyReceiptAsync(MakeOrderEntity(), MakeTransactionData(), httpClient, MakeSetting(material));

                Assert.False(result.IsSuccess);
                Assert.Contains("验签失败", result.ErrorMessage);
            }
        }
    }

    [Fact]
    public async Task LegacyPath_ReceiptDataForm_DoesNotCallServerApi()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            using (var httpClient = new HttpClient(handler, false))
            {
                var transactionData = MakeTransactionData();
                transactionData.ReceiptData = "legacy-receipt-base64";
                var result = await new ApplePaymentHandler().VerifyReceiptAsync(MakeOrderEntity(), transactionData, httpClient, MakeSetting(material));

                // 路由判定：receiptData 形态走 verifyReceipt 旧路径（本地桩响应必然验证失败 → 非成功），
                // 绝不触发 Server API（无 /inApps/ 请求）
                Assert.False(result.IsSuccess);
                Assert.DoesNotContain(handler.RequestUrls, url => url.Contains("/inApps/", StringComparison.Ordinal));
                Assert.Contains(handler.RequestUrls, url => url.Contains("/verifyReceipt", StringComparison.Ordinal));
            }
        }
    }

    // ---------- Verifier 层：双形态路由 + fail-closed 前置 ----------

    [Fact]
    public async Task Verifier_TransactionIdOnly_RoutesToServerApiPath_AndSucceeds()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            handler.SetTransactionInfoResponse("t-271", material.Sign(MakePayload()));
            var verifier = new GamePayReceiptVerifier(
                new StubPaymentChannelFactory(new ApplePaymentHandler(), MakeSetting(material)),
                new ClientApiTestHttpClientFactory(handler));

            var result = await verifier.VerifyAsync(MakeTenantOrder(), new GameUserPayVerifyRequest
            {
                UserId = 7001,
                OrderId = "pay-271",
                PayChannel = 100,
                TransactionId = "t-271",
                ProductId = "sku.gold",
            });

            Assert.Equal(GamePayVerifyState.Succeeded, result.State);
            Assert.Equal("t-271", result.ProviderTransactionId);
            Assert.Equal("sku.gold", result.ProductId);
            Assert.Single(handler.RequestUrls);
        }
    }

    [Theory]
    [InlineData("privateKey")]
    [InlineData("keyId")]
    [InlineData("issuerId")]
    [InlineData("rootCa")]
    public async Task Verifier_FailClosed_MissingCredential_Rejected_WithoutHttpCall(string missingField)
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            var setting = MakeSetting(material);
            switch (missingField)
            {
                case "privateKey":
                    setting.ApplePaymentPrivateKey = string.Empty;
                    break;
                case "keyId":
                    setting.ApplePaymentApiKeyId = string.Empty;
                    break;
                case "issuerId":
                    setting.ApplePaymentIssuerId = string.Empty;
                    break;
                case "rootCa":
                    setting.ApplePaymentRootCertificate = string.Empty;
                    break;
            }

            var verifier = new GamePayReceiptVerifier(new StubPaymentChannelFactory(new ApplePaymentHandler(), setting), new ClientApiTestHttpClientFactory(handler));

            var result = await verifier.VerifyAsync(MakeTenantOrder(), new GameUserPayVerifyRequest
            {
                UserId = 7001,
                OrderId = "pay-271",
                PayChannel = 100,
                TransactionId = "t-271",
            });

            // fail-closed：明确业务拒绝（不 500、不静默降级、零外呼）
            Assert.Equal(GamePayVerifyState.Rejected, result.State);
            Assert.Contains("配置缺失", result.Message);
            Assert.Empty(handler.RequestUrls);
        }
    }

    [Fact]
    public async Task Verifier_ServerApiV2Disabled_RejectsTransactionIdForm()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            var setting = MakeSetting(material, useServerApiV2: false);
            var verifier = new GamePayReceiptVerifier(new StubPaymentChannelFactory(new ApplePaymentHandler(), setting), new ClientApiTestHttpClientFactory(handler));

            var result = await verifier.VerifyAsync(MakeTenantOrder(), new GameUserPayVerifyRequest
            {
                UserId = 7001,
                OrderId = "pay-271",
                PayChannel = 100,
                TransactionId = "t-271",
            });

            Assert.Equal(GamePayVerifyState.Rejected, result.State);
            Assert.Contains("未启用", result.Message);
            Assert.Empty(handler.RequestUrls);
        }
    }

    [Fact]
    public async Task Verifier_BothReceiptAndTransactionEmpty_Rejected()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            var verifier = new GamePayReceiptVerifier(new StubPaymentChannelFactory(new ApplePaymentHandler(), MakeSetting(material)), new ClientApiTestHttpClientFactory(handler));

            var result = await verifier.VerifyAsync(MakeTenantOrder(), new GameUserPayVerifyRequest
            {
                UserId = 7001,
                OrderId = "pay-271",
                PayChannel = 100,
            });

            Assert.Equal(GamePayVerifyState.Rejected, result.State);
            Assert.Contains("或交易ID", result.Message);
            Assert.Empty(handler.RequestUrls);
        }
    }

    [Fact]
    public async Task Verifier_LegacyForm_StillRequiresTransactionId()
    {
        using (var material = AppleJwsTestMaterial.Create())
        {
            var handler = new RecordingServerApiHandler();
            var verifier = new GamePayReceiptVerifier(new StubPaymentChannelFactory(new ApplePaymentHandler(), MakeSetting(material)), new ClientApiTestHttpClientFactory(handler));

            var result = await verifier.VerifyAsync(MakeTenantOrder(), new GameUserPayVerifyRequest
            {
                UserId = 7001,
                OrderId = "pay-271",
                PayChannel = 100,
                ReceiptData = "legacy-receipt-base64",
            });

            // 旧路径双必填校验保留：receiptData 形态仍要求 TransactionId
            Assert.Equal(GamePayVerifyState.Rejected, result.State);
            Assert.Equal("Apple 交易ID不能为空", result.Message);
        }
    }

    // ---------- CreateBearerToken 前置修复回归 ----------

    [Fact]
    public void CreateBearerToken_ExpiresIn20Minutes_WithRequiredClaims()
    {
        using (var handler = new RecordingServerApiHandler())
        {
            var client = new AppStoreServerApiClient(
                MakeTestP8Key(), "key-271", "issuer-271", "com.gameframex.c271", ApplePayEnvironment.Sandbox, new HttpClient(handler, false));

            var token = client.CreateBearerToken();
            var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token);

            var issuedAt = long.Parse(jwt.GetClaim("iat")!.Value, CultureInfo.InvariantCulture);
            var expires = long.Parse(jwt.GetClaim("exp")!.Value, CultureInfo.InvariantCulture);
            // Apple 官方 JWT 有效期上限 20 分钟（1200 秒），修复前为 1 小时
            Assert.Equal(1200, expires - issuedAt);
            Assert.Equal("issuer-271", jwt.GetClaim("iss")!.Value);
            Assert.Equal("appstoreconnect-v1", jwt.GetClaim("aud")!.Value);
            Assert.Equal("com.gameframex.c271", jwt.GetClaim("bid")!.Value);
            // 生成 token 不外呼
            Assert.Empty(handler.RequestUrls);
        }
    }

    // ---------- 构造工具 ----------

    private static JWSTransactionDecodedPayload MakePayload(
        string transactionId = "t-271",
        string originalTransactionId = "o-271",
        string productId = "sku.gold",
        string bundleId = "com.gameframex.c271",
        ApplePayEnvironment environment = ApplePayEnvironment.Production)
    {
        return new JWSTransactionDecodedPayload
        {
            TransactionId = transactionId,
            OriginalTransactionId = originalTransactionId,
            WebOrderLineItemId = "w-271",
            BundleId = bundleId,
            ProductId = productId,
            SubscriptionGroupIdentifier = "sg-271",
            PurchaseDate = 1717171717000,
            OriginalPurchaseDate = 1717171710000,
            ExpiresDate = 2717171717000,
            Quantity = 2,
            AppAccountToken = "00000000-0000-0000-0000-000000001701",
            OfferIdentifier = "offer-271",
            Environment = environment,
            Storefront = "USA",
            Currency = "USD",
            SignedDate = 1717171718000,
        };
    }

    private static AppleTransactionData MakeTransactionData()
    {
        return new AppleTransactionData
        {
            UserId = "7001",
            Sku = "sku.gold",
            OrderId = "pay-271",
            ReceiptData = string.Empty,
            TransactionId = "t-271",
            ExpectedProductId = "sku.gold",
            IsSandbox = false,
        };
    }

    private static GameOrderBase MakeOrderEntity(bool isSandbox = false)
    {
        return new GameOrderBase
        {
            Id = 9001,
            RoleId = "7001",
            AccountId = "7001",
            AreaId = "0",
            Sku = "sku.gold",
            OrderId = "pay-271",
            Country = "US",
            ClientIp = "127.0.0.1",
            IsSandbox = isSandbox,
            Amount = 990,
        };
    }

    private static TenantGameOrderEntity MakeTenantOrder()
    {
        return new TenantGameOrderEntity
        {
            Id = 9001,
            AccountId = "7001",
            RoleId = "7001",
            AreaId = 0,
            OrderId = "pay-271",
            Sku = 0,
            ProductId = "sku.gold",
            Status = OrderStatusEnum.Unpaid,
            Amount = 990,
            PayChannel = "100",
            Platform = "1",
            Channel = string.Empty,
            SubChannel = string.Empty,
            Country = "US",
            Content = string.Empty,
            AdventureSuffix = string.Empty,
        };
    }

    private static ApplePaymentOptionSetting MakeSetting(AppleJwsTestMaterial material, string environment = "production", bool useServerApiV2 = true)
    {
        return new ApplePaymentOptionSetting
        {
            ApplePaymentBundleId = "com.gameframex.c271",
            ApplePaymentEnvironment = environment,
            ApplePaymentApiKeyId = "key-271",
            ApplePaymentIssuerId = "issuer-271",
            ApplePaymentPrivateKey = MakeTestP8Key(),
            ApplePaymentRootCertificate = material.RootCertificatePem,
            ApplePaymentUseServerApiV2 = useServerApiV2,
        };
    }

    private static string MakeTestP8Key()
    {
        using (var key = ECDsa.Create(ECCurve.NamedCurves.nistP256))
        {
            return key.ExportPkcs8PrivateKeyPem();
        }
    }

    /// <summary>
    /// Server API 外呼记录桩：记录完整请求 URL（可断言 sandbox/production base），按路径返回配置的 TransactionInfo JSON
    /// </summary>
    private sealed class RecordingServerApiHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> _responses = new(StringComparer.OrdinalIgnoreCase);

        public List<string> RequestUrls { get; } = new();

        public void SetTransactionInfoResponse(string transactionId, string signedTransactionInfo)
        {
            _responses["/inApps/v1/transactions/" + transactionId] =
                JsonHelper.Serialize(new TransactionInfoResponse { SignedTransactionInfo = signedTransactionInfo, });
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUrls.Add(request.RequestUri?.ToString() ?? string.Empty);
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            var payload = _responses.TryGetValue(path, out var response) ? response : "{}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            });
        }
    }

    /// <summary>
    /// 单渠道工厂桩：apple 渠道返回注入的 handler 与配置
    /// </summary>
    private sealed class StubPaymentChannelFactory : IPaymentChannelFactory
    {
        private readonly IPaymentChannelHandler _handler;
        private readonly IPaymentOptionSetting _setting;

        public StubPaymentChannelFactory(IPaymentChannelHandler handler, IPaymentOptionSetting setting)
        {
            _handler = handler;
            _setting = setting;
        }

        public IPaymentChannelHandler GetHandler(string channelName)
        {
            return _handler;
        }

        public bool IsSupported(string channelName)
        {
            return channelName == "apple";
        }

        public IEnumerable<string> GetSupportedChannels()
        {
            return new[] { "apple", };
        }

        public IPaymentOptionSetting GetOptionSetting(string channelName)
        {
            return _setting;
        }

        public IEnumerable<IPaymentOptionSetting> GetOptionSettings(string channelName)
        {
            return new[] { _setting, };
        }
    }
}