using System.Globalization;
using System.Text.Json;
using GameFrameX.Client.Api.GameUserPay.Dto;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Payment.Apple.Models;
using GameFrameX.Payment.Apple.Option;
using GameFrameX.Payment.Google.Models;

namespace GameFrameX.Client.Api.Services.PayVerification;

/// <summary>
/// 默认验真实现：按 PayChannel 取渠道 handler 调其 VerifyReceiptAsync（原生实现 Pay.Apple / Pay.Google）。
/// <para>C271 Apple 双形态路由：receiptData 形态（StoreKit 1）走 verifyReceipt 旧路径；
/// 仅 transactionId 形态（StoreKit 2）走 App Store Server API v2 新路径（前置 fail-closed 检查）。</para>
/// <para>渠道不支持 / 支付配置未加载 / 必要凭证缺失 → Rejected（明确错误码拒绝，绝不静默放行）；
/// 渠道验真失败（收据无效等）→ Pending（Rcode=7，可疑不记账）。</para>
/// </summary>
public sealed class GamePayReceiptVerifier : IPayReceiptVerifier
{
    private readonly IPaymentChannelFactory _paymentChannelFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    ///
    /// </summary>
    /// <param name="paymentChannelFactory">支付渠道工厂</param>
    /// <param name="httpClientFactory">HTTP 客户端工厂（渠道验真外呼）</param>
    public GamePayReceiptVerifier(IPaymentChannelFactory paymentChannelFactory, IHttpClientFactory httpClientFactory)
    {
        _paymentChannelFactory = paymentChannelFactory;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// 验真一笔订单的渠道凭证
    /// </summary>
    /// <param name="order">待验真订单</param>
    /// <param name="request">验真请求</param>
    /// <returns>验真结果三态</returns>
    public async Task<GamePayVerifyResponse> VerifyAsync(TenantGameOrderEntity order, GameUserPayVerifyRequest request)
    {
        var channelName = MapChannelName(order.PayChannel);
        if (channelName.Length == 0)
        {
            return GamePayVerifyResponse.Rejected("不支持的支付渠道: " + order.PayChannel);
        }

        // ponytail: Amazon(66) 无验真通道——Payment/ 下无 Amazon handler 与配置 schema，
        // 工厂取不到 handler 即拒绝（等价于「配置开关未开即拒绝」）；接入 Pay.Amazon 后自动放行
        var handler = _paymentChannelFactory.GetHandler(channelName);
        if (handler == null)
        {
            return GamePayVerifyResponse.Rejected("支付渠道处理器未注册: " + channelName);
        }

        var optionSetting = _paymentChannelFactory.GetOptionSetting(channelName);
        if (optionSetting == null)
        {
            return GamePayVerifyResponse.Rejected("支付渠道配置未加载: " + channelName);
        }

        var accountId = GetAccountId(order);
        var productId = GetProductId(order);
        Payment.Abstract.Interface.ITransactionData transactionData; // ponytail: 完全限定——本 host 引 Sentry，其同名 ITransactionData 与 Pay.Abstract 歧义
        if (channelName == "apple")
        {
            // C271 双形态路由：StoreKit 1（receiptData）走 verifyReceipt 旧路径；StoreKit 2（仅 transactionId）走 Server API v2 新路径
            if (!string.IsNullOrEmpty(request.ReceiptData))
            {
                if (string.IsNullOrEmpty(request.TransactionId))
                {
                    return GamePayVerifyResponse.Rejected("Apple 交易ID不能为空");
                }

                transactionData = new AppleTransactionData
                {
                    UserId = accountId,
                    Sku = productId,
                    OrderId = order.OrderId,
                    ReceiptData = request.ReceiptData,
                    TransactionId = request.TransactionId,
                    ExpectedProductId = string.IsNullOrWhiteSpace(request.ProductId) ? null : request.ProductId,
                    IsSandbox = false,
                };
            }
            else if (!string.IsNullOrEmpty(request.TransactionId))
            {
                // 新路径前置（fail-closed）：开关未启用 / Server API 凭证缺失即拒绝，绝不静默回退旧路径
                if (optionSetting is not ApplePaymentOptionSetting appleSetting)
                {
                    return GamePayVerifyResponse.Rejected("Apple支付配置类型错误，无法走 Server API v2 路径");
                }

                if (!appleSetting.ApplePaymentUseServerApiV2)
                {
                    return GamePayVerifyResponse.Rejected("Apple Server API v2 验证未启用（ApplePaymentUseServerApiV2=false），请改用收据（receiptData）上报");
                }

                var missing = GetMissingServerApiCredentials(appleSetting);
                if (missing.Length > 0)
                {
                    return GamePayVerifyResponse.Rejected("Apple Server API v2 配置缺失，禁止降级旧路径: " + missing);
                }

                transactionData = new AppleTransactionData
                {
                    UserId = accountId,
                    Sku = productId,
                    OrderId = order.OrderId,
                    ReceiptData = string.Empty,
                    TransactionId = request.TransactionId,
                    ExpectedProductId = string.IsNullOrWhiteSpace(request.ProductId) ? null : request.ProductId,
                    IsSandbox = false,
                };
            }
            else
            {
                return GamePayVerifyResponse.Rejected("Apple 需提供收据数据（StoreKit 1 receiptData）或交易ID（StoreKit 2 transactionId）");
            }
        }
        else
        {
            if (string.IsNullOrEmpty(request.PurchaseToken))
            {
                return GamePayVerifyResponse.Rejected("Google 购买令牌不能为空");
            }

            transactionData = new GoogleTransactionData
            {
                PurchaseToken = request.PurchaseToken,
                ProductId = string.IsNullOrWhiteSpace(request.ProductId) ? productId : request.ProductId,
                OrderId = order.OrderId,
                UserId = accountId,
                IsSandbox = false,
            };
        }

        var entityBase = new GameOrderBase
        {
            Id = order.Id,
            RoleId = order.RoleId,
            AccountId = accountId,
            Sku = productId,
            OrderId = order.OrderId,
            Country = order.Country ?? string.Empty,
            Amount = order.Amount,
            IsSandbox = false,
        };
        using (var httpClient = _httpClientFactory.CreateClient())
        {
            var result = await handler.VerifyReceiptAsync(entityBase, transactionData, httpClient, optionSetting);
            if (result.IsSuccess && result.Output != null)
            {
                var providerTransactionId = FirstNonEmpty(result.Output.ChannelTransactionId, result.Output.IdempotencyKey, result.Output.OrderId);
                return GamePayVerifyResponse.Succeeded(providerTransactionId, result.Output.Sku, JsonSerializer.Serialize(result.Output));
            }

            return GamePayVerifyResponse.Pending(result.ErrorMessage);
        }
    }

    /// <summary>
    /// 汇总 Server API v2 新路径必需凭证缺失项（C271 fail-closed：缺任一即 Rejected，不外呼、不降级旧路径）
    /// </summary>
    private static string GetMissingServerApiCredentials(ApplePaymentOptionSetting setting)
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

        return string.Join(", ", missing);
    }

    /// <summary>
    /// 旧协议渠道号 → 渠道名（PaymentChannelRegistry 同名口径）
    /// </summary>
    private static string MapChannelName(string payChannel)
    {
        if (!int.TryParse(payChannel, NumberStyles.Integer, CultureInfo.InvariantCulture, out var legacyPayChannel))
        {
            return payChannel.ToLowerInvariant() switch
            {
                "apple"  => "apple",
                "google" => "google",
                "amazon" => "amazon",
                _        => string.Empty,
            };
        }

        return legacyPayChannel switch
        {
            100 => "apple",
            7   => "google",
            66  => "amazon",
            _   => string.Empty,
        };
    }

    private static string FirstNonEmpty(string first, string second, string third)
    {
        if (!string.IsNullOrEmpty(first))
        {
            return first;
        }

        if (!string.IsNullOrEmpty(second))
        {
            return second;
        }

        return third ?? string.Empty;
    }

    private static string GetAccountId(TenantGameOrderEntity order)
    {
        if (!string.IsNullOrWhiteSpace(order.AccountId))
        {
            return order.AccountId;
        }

        return order.RoleId;
    }

    private static string GetProductId(TenantGameOrderEntity order)
    {
        if (!string.IsNullOrWhiteSpace(order.ProductId))
        {
            return order.ProductId;
        }

        return order.Sku > 0 ? order.Sku.ToString(CultureInfo.InvariantCulture) : string.Empty;
    }
}