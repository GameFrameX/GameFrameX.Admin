using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Payment.Abstract.Models;
using GameFrameX.Payment.Google.Models;
using GameFrameX.Payment.Google.Models.CreateOrder;
using GameFrameX.Payment.Google.Models.Receipt;
using GameFrameX.Payment.Google.Option;

namespace GameFrameX.Payment.Google;

/// <summary>
/// Google支付渠道处理器
/// </summary>
public sealed class GooglePaymentHandler : IPaymentChannelHandler
{
    /// <summary>
    /// 支付渠道名称
    /// </summary>
    public string ChannelName
    {
        get { return "google"; }
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
    public async Task<PaymentChannelCreateOrderResponse> CreateOrderAsync(IGameOrderBase entity, object input, object user, HttpClient httpClient, IPaymentOptionSetting optionSetting)
    {
        try
        {
            // 类型转换 - 在实际使用中需要根据具体的实体类型进行转换
            var output = new GoogleCreateOrderResponse
            {
                IsNeedPay = true,
                RoleId = entity.RoleId,
                Sku = Convert.ToString(entity.Sku),
            };

            return PaymentChannelCreateOrderResponse.Success(entity, output);
        }
        catch (Exception e)
        {
            return await Task.FromResult(PaymentChannelCreateOrderResponse.Fail(e.Message));
        }
    }

    /// <summary>
    /// 验证支付回执（如支付通知或收据）
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

            if (optionSetting is not GooglePaymentOptionSetting googlePayOptionSetting)
            {
                return PaymentChannelVerifyReceiptResponse.Fail("Google支付配置类型错误");
            }

            if (transactionData is not GoogleTransactionData googleTransactionData)
            {
                return PaymentChannelVerifyReceiptResponse.Fail("Google交易数据类型错误");
            }

            // 验证必需参数
            if (string.IsNullOrEmpty(googleTransactionData.PurchaseToken))
            {
                return PaymentChannelVerifyReceiptResponse.Fail("购买令牌不能为空");
            }

            if (string.IsNullOrEmpty(googleTransactionData.ProductId))
            {
                return PaymentChannelVerifyReceiptResponse.Fail("产品ID不能为空");
            }

            var packageName = !string.IsNullOrEmpty(googleTransactionData.PackageName) ? googleTransactionData.PackageName : googlePayOptionSetting.GooglePaymentPackageName;

            if (string.IsNullOrEmpty(packageName))
            {
                return PaymentChannelVerifyReceiptResponse.Fail("应用包名不能为空");
            }

            // 创建Google Play API管理器
            await GooglePlayApiManager.InitializeAsync(googlePayOptionSetting);

            // 验证购买
            var purchase = await GooglePlayApiManager.VerifyPurchaseAsync(packageName, googleTransactionData.ProductId, googleTransactionData.PurchaseToken);

            if (purchase == null)
            {
                return PaymentChannelVerifyReceiptResponse.Fail("未找到对应的购买记录");
            }

            // 验证购买状态
            if (!GooglePlayApiManager.IsPurchaseValid(purchase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"购买状态无效: {purchase.PurchaseState}");
            }

            if (!string.IsNullOrEmpty(purchase.ProductId) &&
                !string.Equals(purchase.ProductId, googleTransactionData.ProductId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"产品ID不匹配，期望: {googleTransactionData.ProductId}, 实际: {purchase.ProductId}");
            }

            if (!string.IsNullOrWhiteSpace(entity.Sku) &&
                !string.Equals(entity.Sku, googleTransactionData.ProductId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"订单商品不匹配，期望: {entity.Sku}, 实际: {googleTransactionData.ProductId}");
            }

            if (!string.IsNullOrWhiteSpace(googleTransactionData.UserId) &&
                !string.Equals(entity.RoleId, googleTransactionData.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"用户不匹配，期望: {entity.RoleId}, 实际: {googleTransactionData.UserId}");
            }

            // 验证订单ID（如果提供了）
            if (!string.IsNullOrWhiteSpace(googleTransactionData.OrderId) &&
                !string.Equals(purchase.OrderId, googleTransactionData.OrderId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"Google订单ID不匹配，期望: {googleTransactionData.OrderId}, 实际: {purchase.OrderId}");
            }

            if (!string.IsNullOrWhiteSpace(entity.OrderId) &&
                !string.Equals(purchase.OrderId, entity.OrderId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"订单实体Google订单ID不匹配，期望: {entity.OrderId}, 实际: {purchase.OrderId}");
            }

            var expectedObfuscatedAccountId = !string.IsNullOrWhiteSpace(googleTransactionData.ObfuscatedExternalAccountId)
                                                  ? googleTransactionData.ObfuscatedExternalAccountId
                                                  : entity.Id.ToString();
            if (string.IsNullOrWhiteSpace(purchase.ObfuscatedExternalAccountId) ||
                !string.Equals(purchase.ObfuscatedExternalAccountId, expectedObfuscatedAccountId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"订单归属不匹配，期望: {expectedObfuscatedAccountId}, 实际: {purchase.ObfuscatedExternalAccountId}");
            }

            var expectedObfuscatedProfileId = !string.IsNullOrWhiteSpace(googleTransactionData.ObfuscatedExternalProfileId)
                                                  ? googleTransactionData.ObfuscatedExternalProfileId
                                                  : googleTransactionData.UserId;
            if (!string.IsNullOrWhiteSpace(expectedObfuscatedProfileId) &&
                !string.Equals(purchase.ObfuscatedExternalProfileId, expectedObfuscatedProfileId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"用户归属不匹配，期望: {expectedObfuscatedProfileId}, 实际: {purchase.ObfuscatedExternalProfileId}");
            }

            // 验证开发者载荷（如果提供了）
            if (!string.IsNullOrEmpty(googleTransactionData.DeveloperPayload) &&
                !string.Equals(purchase.DeveloperPayload, googleTransactionData.DeveloperPayload, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"开发者载荷不匹配");
            }

            // 验证消耗状态（如果指定了期望状态）
            if (googleTransactionData.ExpectedConsumptionState.HasValue && purchase.ConsumptionState != googleTransactionData.ExpectedConsumptionState.Value)
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"消耗状态不匹配，期望: {googleTransactionData.ExpectedConsumptionState}, 实际: {purchase.ConsumptionState}");
            }

            // 验证确认状态（如果指定了期望状态）
            if (googleTransactionData.ExpectedAcknowledgementState.HasValue && purchase.AcknowledgementState != googleTransactionData.ExpectedAcknowledgementState.Value)
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"确认状态不匹配，期望: {googleTransactionData.ExpectedAcknowledgementState}, 实际: {purchase.AcknowledgementState}");
            }

            // 创建验证成功的结果
            var verificationResult = new GoogleVerifyReceiptResponse
            {
                IsSuccess = true,
                Sku = purchase.ProductId ?? string.Empty,
                RoleId = googleTransactionData.UserId,
                OrderId = purchase.OrderId ?? string.Empty,
                ChannelTransactionId = purchase.OrderId ?? string.Empty,
                MerchantOrderId = entity.OrderId,
                AccountId = entity.AccountId,
                Amount = entity.Amount / 100m,
                Currency = entity.Country,
                OrderStatus = purchase.PurchaseState?.ToString() ?? string.Empty,
                IdempotencyKey = purchase.OrderId ?? googleTransactionData.PurchaseToken,
                PurchaseToken = googleTransactionData.PurchaseToken,
                ProductId = purchase.ProductId ?? string.Empty,
                PurchaseState = purchase.PurchaseState ?? 0,
                ConsumptionState = purchase.ConsumptionState ?? 0,
                AcknowledgementState = purchase.AcknowledgementState ?? 0,
                PurchaseType = purchase.PurchaseType,
                PurchaseTimeMillis = purchase.PurchaseTimeMillis ?? 0,
                RegionCode = purchase.RegionCode ?? string.Empty,
                DeveloperPayload = purchase.DeveloperPayload ?? string.Empty,
                ObfuscatedExternalAccountId = purchase.ObfuscatedExternalAccountId ?? string.Empty,
                ObfuscatedExternalProfileId = purchase.ObfuscatedExternalProfileId ?? string.Empty,
                Quantity = purchase.Quantity ?? 1,
                RefundableQuantity = purchase.RefundableQuantity ?? 0,
                IsSandbox = googleTransactionData.IsSandbox,
                PackageName = packageName,
            };

            return PaymentChannelVerifyReceiptResponse.Success(entity, verificationResult);
        }
        catch (Exception ex)
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"验证Google Play交易时发生错误: {ex.Message}");
        }
    }
}