using GameFrameX.Foundation.Json;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Payment.Abstract.Models;
using GameFrameX.Payment.Apple.AppStoreServer;
using GameFrameX.Payment.Apple.AppStoreServer.Models;
using GameFrameX.Payment.Apple.Models;
using GameFrameX.Payment.Apple.Option;
using GameFrameX.Payment.Apple.Models.Verification;

namespace GameFrameX.Payment.Apple;

/// <summary>
/// Apple支付渠道处理器
/// <para>C271 双轨验证：ReceiptData 形态（StoreKit 1）走 verifyReceipt 旧路径；
/// 仅 TransactionId 形态（StoreKit 2）走 App Store Server API v2 新路径。</para>
/// </summary>
public class ApplePaymentHandler : IPaymentChannelHandler
{
    /// <summary>
    /// 支付渠道名称
    /// </summary>
    public string ChannelName
    {
        get { return "apple"; }
    }

    /// <summary>
    /// 创建订单
    /// </summary>
    /// <param name="entity">订单实体</param>
    /// <param name="input">输入参数</param>
    /// <param name="user">用户信息</param>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="optionSetting">支付配置</param>
    /// <returns>订单创建结果</returns>
    public Task<PaymentChannelCreateOrderResponse> CreateOrderAsync(IGameOrderBase entity, object input, object user, HttpClient httpClient, IPaymentOptionSetting optionSetting)
    {
        try
        {
            // Apple支付通常不需要预先创建订单，而是在客户端完成支付后验证收据
            // 创建Apple支付订单输出
            var output = new AppleCreateOrderResponse
            {
                RoleId = entity.RoleId,
                Sku = entity.Sku,
                IsNeedPay = true,
            };
            return Task.FromResult<PaymentChannelCreateOrderResponse>(PaymentChannelCreateOrderResponse.Success(entity, output));
        }
        catch (Exception ex)
        {
            return Task.FromResult(PaymentChannelCreateOrderResponse.Fail($"创建Apple支付订单失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 验证支付回执（如支付通知或收据）。双轨路由：
    /// <para>带 ReceiptData（StoreKit 1 收据）→ /verifyReceipt 旧路径；
    /// 仅带 TransactionId（StoreKit 2 交易号）→ App Store Server API v2 新路径
    /// （GetTransactionInfo + JWS x5c 证书链验签），两者映射同一验证结果，幂等键 = TransactionId。</para>
    /// </summary>
    /// <param name="entity">订单实体</param>
    /// <param name="transactionData">交易数据</param>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="optionSetting">支付配置</param>
    /// <returns>验证结果</returns>
    public async Task<PaymentChannelVerifyReceiptResponse> VerifyReceiptAsync(IGameOrderBase entity, ITransactionData transactionData, HttpClient httpClient, IPaymentOptionSetting optionSetting)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(transactionData);
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(optionSetting);

            if (optionSetting is not ApplePaymentOptionSetting applePayOptionSetting)
            {
                return PaymentChannelVerifyReceiptResponse.Fail("Apple支付配置类型错误");
            }

            if (transactionData is not AppleTransactionData appleTransactionData)
            {
                return PaymentChannelVerifyReceiptResponse.Fail("Apple交易数据类型错误");
            }

            // 双轨路由：仅带 TransactionId（StoreKit 2）走 App Store Server API 新路径
            if (string.IsNullOrEmpty(appleTransactionData.ReceiptData))
            {
                if (string.IsNullOrEmpty(appleTransactionData.TransactionId))
                {
                    return PaymentChannelVerifyReceiptResponse.Fail("收据数据与交易ID不能同时为空");
                }

                return await VerifyViaServerApiAsync(entity, appleTransactionData, httpClient, applePayOptionSetting);
            }

            // 旧路径（StoreKit 1 receipt）：TransactionId 必填校验保留
            if (string.IsNullOrEmpty(appleTransactionData.TransactionId))
            {
                return PaymentChannelVerifyReceiptResponse.Fail("交易ID不能为空");
            }

            // 创建Apple收据验证管理器
            var verificationManager = new AppleReceiptVerificationManager(applePayOptionSetting);

            // 验证收据
            var verificationResponse = await verificationManager.VerifyReceiptAsync(appleTransactionData.ReceiptData, httpClient);

            if (verificationResponse == null)
            {
                return PaymentChannelVerifyReceiptResponse.Fail("收据验证响应为空");
            }

            // 检查验证状态
            if (!verificationManager.IsVerificationSuccessful(verificationResponse))
            {
                var errorMessage = GetAppleErrorMessage(verificationResponse.Status);
                return PaymentChannelVerifyReceiptResponse.Fail($"收据验证失败，状态码: {verificationResponse.Status}, 错误: {errorMessage}");
            }

            // 验证Bundle ID
            if (!verificationManager.ValidateBundleId(verificationResponse))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"Bundle ID不匹配，期望: {applePayOptionSetting.ApplePaymentBundleId}, 实际: {verificationResponse.Receipt?.BundleId}");
            }

            // 提取交易信息
            var transactionInfo = verificationManager.ExtractTransactionInfo(verificationResponse, appleTransactionData.TransactionId);
            if (transactionInfo == null)
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"未找到交易ID为 {appleTransactionData.TransactionId} 的交易记录");
            }

            // 验证产品ID（如果提供了期望的产品ID）
            var expectedProductId = !string.IsNullOrWhiteSpace(appleTransactionData.ExpectedProductId)
                                        ? appleTransactionData.ExpectedProductId
                                        : entity.Sku;
            if (!string.IsNullOrWhiteSpace(expectedProductId) &&
                !transactionInfo.ProductId.Equals(expectedProductId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"产品ID不匹配，期望: {expectedProductId}, 实际: {transactionInfo.ProductId}");
            }

            if (!string.IsNullOrWhiteSpace(appleTransactionData.UserId) &&
                !string.Equals(entity.RoleId, appleTransactionData.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"用户不匹配，期望: {entity.RoleId}, 实际: {appleTransactionData.UserId}");
            }

            if (!string.IsNullOrWhiteSpace(appleTransactionData.OrderId) &&
                !string.Equals(entity.OrderId, appleTransactionData.OrderId, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(entity.Id.ToString(), appleTransactionData.OrderId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"订单不匹配，期望: {entity.OrderId}, 实际: {appleTransactionData.OrderId}");
            }

            if (!string.IsNullOrWhiteSpace(appleTransactionData.OriginalTransactionId) &&
                !string.Equals(transactionInfo.OriginalTransactionId, appleTransactionData.OriginalTransactionId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"原始交易ID不匹配，期望: {appleTransactionData.OriginalTransactionId}, 实际: {transactionInfo.OriginalTransactionId}");
            }

            var isSandboxReceipt = verificationResponse.IsSandboxReceipt || string.Equals(verificationResponse.Environment, "Sandbox", StringComparison.OrdinalIgnoreCase);
            if (entity.IsSandbox != isSandboxReceipt)
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"收据环境不匹配，订单沙盒: {entity.IsSandbox}, 收据沙盒: {isSandboxReceipt}");
            }

            // 解析日期
            var purchaseDate = ParseAppleDate(transactionInfo.PurchaseDate, transactionInfo.PurchaseDateMs);
            var originalPurchaseDate = ParseAppleDate(transactionInfo.OriginalPurchaseDate, transactionInfo.OriginalPurchaseDateMs);
            var receiptCreationDate = ParseAppleDate(verificationResponse.Receipt?.ReceiptCreationDate, verificationResponse.Receipt?.ReceiptCreationDateMs);
            var requestDate = ParseAppleDate(verificationResponse.Receipt?.RequestDate, verificationResponse.Receipt?.RequestDateMs);

            // 创建验证成功的结果
            var verificationResult = new AppleVerifyReceiptResponse
            {
                IsSuccess = true,
                Sku = transactionInfo.ProductId,
                RoleId = appleTransactionData.UserId,
                OrderId = transactionInfo.TransactionId,
                ChannelTransactionId = transactionInfo.TransactionId,
                MerchantOrderId = entity.OrderId,
                AccountId = entity.AccountId,
                Amount = entity.Amount / 100m,
                Currency = entity.Country,
                OrderStatus = "PURCHASED",
                IdempotencyKey = transactionInfo.TransactionId,
                TransactionId = transactionInfo.TransactionId,
                OriginalTransactionId = transactionInfo.OriginalTransactionId,
                ProductId = transactionInfo.ProductId,
                PurchaseDate = purchaseDate,
                PurchaseDateMs = long.TryParse(transactionInfo.PurchaseDateMs, out var purchaseDateMs) ? purchaseDateMs : 0,
                OriginalPurchaseDate = originalPurchaseDate,
                OriginalPurchaseDateMs = long.TryParse(transactionInfo.OriginalPurchaseDateMs, out var originalPurchaseDateMs) ? originalPurchaseDateMs : 0,
                ExpiresDate = ParseAppleDate(transactionInfo.ExpiresDate, transactionInfo.ExpiresDateMs),
                ExpiresDateMs = long.TryParse(transactionInfo.ExpiresDateMs ?? "0", out var expiresDateMs) ? expiresDateMs : null,
                Quantity = int.TryParse(transactionInfo.Quantity, out var quantity) ? quantity : 1,
                BundleId = verificationResponse.Receipt?.BundleId ?? string.Empty,
                ApplicationVersion = verificationResponse.Receipt?.ApplicationVersion ?? string.Empty,
                IsSandbox = verificationResponse.IsSandboxReceipt || appleTransactionData.IsSandbox,
                Status = verificationResponse.Status,
                IsTrialPeriod = transactionInfo.IsTrialPeriod == "true",
                IsInIntroOfferPeriod = transactionInfo.IsInIntroOfferPeriod == "true",
                CancellationDate = ParseAppleDate(transactionInfo.CancellationDate, null),
                CancellationReason = transactionInfo.CancellationReason ?? string.Empty,
                WebOrderLineItemId = transactionInfo.WebOrderLineItemId ?? string.Empty,
                PromotionalOfferId = transactionInfo.PromotionalOfferId ?? string.Empty,
                SubscriptionGroupIdentifier = transactionInfo.SubscriptionGroupIdentifier ?? string.Empty,
                Environment = verificationResponse.Environment,
                ReceiptCreationDate = receiptCreationDate,
                ReceiptCreationDateMs = long.TryParse(verificationResponse.Receipt?.ReceiptCreationDateMs ?? "0", out var receiptCreationDateMs) ? receiptCreationDateMs : 0,
                RequestDate = requestDate,
                RequestDateMs = long.TryParse(verificationResponse.Receipt?.RequestDateMs ?? "0", out var requestDateMs) ? requestDateMs : 0,
            };

            return PaymentChannelVerifyReceiptResponse.Success(entity, verificationResult);
        }
        catch (Exception ex)
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"验证Apple支付收据时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 解析Apple日期格式
    /// </summary>
    private DateTime ParseAppleDate(string? dateString, string? dateMs)
    {
        if (!string.IsNullOrEmpty(dateMs) && long.TryParse(dateMs, out var ms))
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(ms).DateTime;
        }

        if (!string.IsNullOrEmpty(dateString) && DateTime.TryParse(dateString, out var date))
        {
            return date;
        }

        return DateTime.MinValue;
    }

    /// <summary>
    /// 获取Apple错误消息
    /// </summary>
    private string GetAppleErrorMessage(int status)
    {
        return status switch
        {
            0     => "成功",
            21000 => "App Store无法读取提供的JSON对象",
            21002 => "收据数据属性中的数据格式错误或缺失",
            21003 => "收据无法通过验证",
            21004 => "提供的共享密钥与您账户中的共享密钥不匹配",
            21005 => "收据服务器当前不可用",
            21006 => "此收据有效但订阅已过期",
            21007 => "此收据来自测试环境，但已发送到生产环境进行验证",
            21008 => "此收据来自生产环境，但已发送到测试环境进行验证",
            21009 => "内部数据访问错误",
            21010 => "找不到或删除了用户账户",
            _     => $"未知错误码: {status}",
        };
    }

    /// <summary>
    /// App Store Server API v2 新路径（StoreKit 2 transactionId 形态主动验证）。
    /// <para>GetTransactionInfo 取 Apple 签名 JWS → AppleJwsVerifier x5c 证书链验签 →
    /// 校验交易一致性（交易号 / BundleId / 商品 / 用户 / 订单 / 环境）→ 映射统一
    /// <see cref="AppleVerifyReceiptResponse"/>（幂等键 = TransactionId，与旧路径同键域）。</para>
    /// <para>fail-closed：p8 私钥 / KeyID / IssuerId / RootCA / BundleId 任一缺失即失败返回，
    /// 绝不静默回退 verifyReceipt 旧路径（ApplePaymentOptionSetting.ApplePaymentPrivateKey 注释已声明此语义）。</para>
    /// </summary>
    /// <param name="entity">订单实体</param>
    /// <param name="appleTransactionData">交易数据（ReceiptData 为空、TransactionId 必填）</param>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="applePayOptionSetting">Apple支付配置</param>
    /// <returns>验证结果（成功时 Output 为 AppleVerifyReceiptResponse）</returns>
    private static async Task<PaymentChannelVerifyReceiptResponse> VerifyViaServerApiAsync(IGameOrderBase entity, AppleTransactionData appleTransactionData, HttpClient httpClient, ApplePaymentOptionSetting applePayOptionSetting)
    {
        // fail-closed 防御：Server API 凭证任一缺失即失败（上层 GamePayReceiptVerifier 已前置拒绝，此处为直接调用防御）
        var missingConfig = GetMissingServerApiConfig(applePayOptionSetting);
        if (missingConfig.Length > 0)
        {
            return PaymentChannelVerifyReceiptResponse.Fail("Apple Server API v2 配置缺失，禁止降级旧路径: " + missingConfig);
        }

        AppStoreServerApiClient apiClient;
        try
        {
            apiClient = new AppStoreServerApiClient(
                applePayOptionSetting.ApplePaymentPrivateKey,
                applePayOptionSetting.ApplePaymentApiKeyId,
                applePayOptionSetting.ApplePaymentIssuerId,
                applePayOptionSetting.ApplePaymentBundleId,
                applePayOptionSetting.IsSandbox ? ApplePayEnvironment.Sandbox : ApplePayEnvironment.Production,
                httpClient);
        }
        catch (Exception ex)
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"Apple Server API v2 客户端初始化失败: {ex.Message}");
        }

        // 主动拉取交易详情（Apple 返回 JWS 签名交易）
        TransactionInfoResponse transactionInfoResponse;
        try
        {
            transactionInfoResponse = await apiClient.GetTransactionInfo(appleTransactionData.TransactionId);
        }
        catch (ApiException ex)
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"Apple Server API 查询交易失败: transactionId={appleTransactionData.TransactionId}, httpStatus={ex.HttpStatusCode}, apiError={ex.ApiError}, {ex.ErrorMessage}");
        }

        if (transactionInfoResponse == null || string.IsNullOrWhiteSpace(transactionInfoResponse.SignedTransactionInfo))
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"Apple Server API 未返回签名交易信息: transactionId={appleTransactionData.TransactionId}");
        }

        // x5c 证书链验签 + 解码 payload（验签失败抛 SecurityException，由外层 catch 统一转 Fail → 上层 Pending）
        string transactionPayloadJson;
        JWSTransactionDecodedPayload payload;
        try
        {
            transactionPayloadJson = AppleJwsVerifier.VerifyAndDecode(transactionInfoResponse.SignedTransactionInfo, applePayOptionSetting.ApplePaymentRootCertificate);
            payload = JsonHelper.Deserialize<JWSTransactionDecodedPayload>(transactionPayloadJson);
        }
        catch (Exception ex)
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"Apple JWS 交易验签失败: {ex.Message}");
        }

        if (payload == null || string.IsNullOrWhiteSpace(payload.TransactionId))
        {
            return PaymentChannelVerifyReceiptResponse.Fail("Apple JWS 交易信息解码为空");
        }

        // 交易一致性校验（语义对齐旧路径）
        if (!string.Equals(payload.TransactionId, appleTransactionData.TransactionId, StringComparison.OrdinalIgnoreCase))
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"交易ID不匹配，期望: {appleTransactionData.TransactionId}, 实际: {payload.TransactionId}");
        }

        if (!string.Equals(payload.BundleId, applePayOptionSetting.ApplePaymentBundleId, StringComparison.OrdinalIgnoreCase))
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"Bundle ID不匹配，期望: {applePayOptionSetting.ApplePaymentBundleId}, 实际: {payload.BundleId}");
        }

        var expectedProductId = !string.IsNullOrWhiteSpace(appleTransactionData.ExpectedProductId)
                                    ? appleTransactionData.ExpectedProductId
                                    : entity.Sku;
        if (!string.IsNullOrWhiteSpace(expectedProductId) &&
            !string.Equals(payload.ProductId, expectedProductId, StringComparison.OrdinalIgnoreCase))
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"产品ID不匹配，期望: {expectedProductId}, 实际: {payload.ProductId}");
        }

        if (!string.IsNullOrWhiteSpace(appleTransactionData.UserId) &&
            !string.Equals(entity.RoleId, appleTransactionData.UserId, StringComparison.OrdinalIgnoreCase))
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"用户不匹配，期望: {entity.RoleId}, 实际: {appleTransactionData.UserId}");
        }

        if (!string.IsNullOrWhiteSpace(appleTransactionData.OrderId) &&
            !string.Equals(entity.OrderId, appleTransactionData.OrderId, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(entity.Id.ToString(), appleTransactionData.OrderId, StringComparison.OrdinalIgnoreCase))
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"订单不匹配，期望: {entity.OrderId}, 实际: {appleTransactionData.OrderId}");
        }

        var isSandboxTransaction = payload.Environment == ApplePayEnvironment.Sandbox;
        if (entity.IsSandbox != isSandboxTransaction)
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"交易环境不匹配，订单沙盒: {entity.IsSandbox}, 交易沙盒: {isSandboxTransaction}");
        }

        // 映射统一验证结果（对齐旧路径 AppleVerifyReceiptResponse 等价字段；幂等键 = TransactionId）
        var verificationResult = new AppleVerifyReceiptResponse
        {
            IsSuccess = true,
            Sku = payload.ProductId,
            RoleId = appleTransactionData.UserId,
            OrderId = payload.TransactionId,
            ChannelTransactionId = payload.TransactionId,
            MerchantOrderId = entity.OrderId,
            AccountId = entity.AccountId,
            Amount = entity.Amount / 100m,
            Currency = string.IsNullOrWhiteSpace(payload.Currency) ? entity.Country : payload.Currency,
            OrderStatus = "PURCHASED",
            IdempotencyKey = payload.TransactionId,
            TransactionId = payload.TransactionId,
            OriginalTransactionId = payload.OriginalTransactionId ?? string.Empty,
            ProductId = payload.ProductId,
            PurchaseDate = FromUnixTimeMilliseconds(payload.PurchaseDate),
            PurchaseDateMs = payload.PurchaseDate ?? 0,
            OriginalPurchaseDate = FromUnixTimeMilliseconds(payload.OriginalPurchaseDate),
            OriginalPurchaseDateMs = payload.OriginalPurchaseDate ?? 0,
            ExpiresDate = FromUnixTimeMilliseconds(payload.ExpiresDate),
            ExpiresDateMs = payload.ExpiresDate,
            Quantity = payload.Quantity ?? 1,
            BundleId = payload.BundleId,
            ApplicationVersion = string.Empty,
            IsSandbox = isSandboxTransaction,
            Status = 0,
            IsTrialPeriod = false,
            IsInIntroOfferPeriod = false,
            CancellationDate = FromUnixTimeMilliseconds(payload.RevocationDate),
            CancellationReason = payload.RevocationReason?.ToString() ?? string.Empty,
            WebOrderLineItemId = payload.WebOrderLineItemId ?? string.Empty,
            PromotionalOfferId = payload.OfferIdentifier ?? string.Empty,
            SubscriptionGroupIdentifier = payload.SubscriptionGroupIdentifier ?? string.Empty,
            Environment = payload.Environment?.ToString() ?? string.Empty,
            ReceiptCreationDate = FromUnixTimeMilliseconds(payload.SignedDate),
            ReceiptCreationDateMs = payload.SignedDate ?? 0,
            RequestDate = DateTime.MinValue,
            RequestDateMs = 0,
        };

        return PaymentChannelVerifyReceiptResponse.Success(entity, verificationResult);
    }

    /// <summary>
    /// 汇总 Server API v2 必需配置缺失项（fail-closed 依据，缺失即拒绝且不降级旧路径）
    /// </summary>
    private static string GetMissingServerApiConfig(ApplePaymentOptionSetting setting)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(setting.ApplePaymentPrivateKey))
        {
            missing.Add("私钥内容(ApplePaymentPrivateKey)");
        }

        if (string.IsNullOrWhiteSpace(setting.ApplePaymentApiKeyId))
        {
            missing.Add("API密钥ID(ApplePaymentApiKeyId)");
        }

        if (string.IsNullOrWhiteSpace(setting.ApplePaymentIssuerId))
        {
            missing.Add("发行商ID(ApplePaymentIssuerId)");
        }

        if (string.IsNullOrWhiteSpace(setting.ApplePaymentRootCertificate))
        {
            missing.Add("Root CA证书(ApplePaymentRootCertificate)");
        }

        if (string.IsNullOrWhiteSpace(setting.ApplePaymentBundleId))
        {
            missing.Add("应用BundleID(ApplePaymentBundleId)");
        }

        return string.Join(", ", missing);
    }

    /// <summary>
    /// UNIX 毫秒时间戳转本地时间（null 或非法值返回 DateTime.MinValue，对齐旧路径 ParseAppleDate 兜底语义）
    /// </summary>
    private static DateTime FromUnixTimeMilliseconds(long? milliseconds)
    {
        if (milliseconds == null || milliseconds.Value <= 0)
        {
            return DateTime.MinValue;
        }

        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds.Value).DateTime;
    }
}