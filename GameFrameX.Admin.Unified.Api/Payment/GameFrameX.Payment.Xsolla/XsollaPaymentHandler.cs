using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Payment.Abstract.Models;
using GameFrameX.Payment.Xsolla.Models;
using GameFrameX.Payment.Xsolla.Option;
using GameFrameX.Payment.Xsolla.Order;
using GameFrameX.Payment.Xsolla.Transaction;

namespace GameFrameX.Payment.Xsolla;

/// <summary>
/// Xsolla支付渠道处理器
/// </summary>
public sealed class XsollaPaymentHandler : IPaymentChannelHandler
{
    /// <summary>
    /// 支付渠道名称
    /// </summary>
    public string ChannelName
    {
        get { return "xsolla"; }
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
            ArgumentNullException.ThrowIfNull(optionSetting);
            var xsollaPayOptionSetting = (XsollaPayOptionSetting)optionSetting;
            var validationError = ValidateSettings(xsollaPayOptionSetting);
            if (!string.IsNullOrEmpty(validationError))
            {
                return PaymentChannelCreateOrderResponse.Fail(validationError);
            }

            // 类型转换 - 在实际使用中需要根据具体的实体类型进行转换
            var gameOrderEntity = entity;
            // dynamic addGameOrderInput = input;
            dynamic gameRoleEntity = user;

            // 向第三方支付平台发起订单创建
            var orderManager = new OrderManager();
            var orderParams = new OrderManager.OrderParamsCreate
            {
                Sandbox = gameRoleEntity.IsSandbox is true,
            };

            orderParams.User.Id.Value = entity.RoleId;
            orderParams.Settings.Currency = entity.Country;
            orderParams.User.Email.Value = xsollaPayOptionSetting.XsollaPaymentEmail;
            orderParams.Settings.ExternalId = gameOrderEntity.Id.ToString();
            orderParams.Purchase.Items.Add(new OrderManager.OrderParamsCreateItem(1, entity.Sku));

            var orderParamsCreateOutput = await orderManager.CreateAsync(httpClient, orderParams, entity.ClientIp, optionSetting);
            gameOrderEntity.OrderId = orderParamsCreateOutput.OrderId.ToString();

            // 创建输出对象 - 需要根据实际的输出类型进行调整
            var output = new XsollaCreateOrderResponse
            {
                Token = orderParamsCreateOutput.Token,
                IsNeedPay = true,
                OrderId = orderParamsCreateOutput.OrderId.ToString(),
                RoleId = entity.RoleId,
                Sku = entity.Sku,
            };

            return PaymentChannelCreateOrderResponse.Success(gameOrderEntity, output);
        }
        catch (Exception e)
        {
            return PaymentChannelCreateOrderResponse.Fail(e.Message);
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
            ArgumentNullException.ThrowIfNull(transactionData);
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(optionSetting);

            var xsollaPayOptionSetting = (XsollaPayOptionSetting)optionSetting;
            var validationError = ValidateSettings(xsollaPayOptionSetting);
            if (!string.IsNullOrEmpty(validationError))
            {
                return PaymentChannelVerifyReceiptResponse.Fail(validationError);
            }

            var xsollaTransactionData = (XsollaTransactionData)transactionData;

            var transactionManager = new TransactionManager();

            // 首先尝试根据外部订单ID查询交易
            XsollaTransaction? transaction = null;

            if (!string.IsNullOrEmpty(xsollaTransactionData.ExternalId))
            {
                var transactionResponse = await transactionManager.FindTransactionsByExternalIdAsync(httpClient, xsollaTransactionData.ExternalId, xsollaPayOptionSetting, xsollaTransactionData.IsSandbox);

                transaction = transactionResponse?.Transactions?.FirstOrDefault();
            }

            // 如果根据外部ID没找到，且有交易ID，则根据交易ID查询
            if (transaction == null && !string.IsNullOrEmpty(xsollaTransactionData.TransactionId))
            {
                if (long.TryParse(xsollaTransactionData.TransactionId, out var transactionId))
                {
                    transaction = await transactionManager.GetTransactionByIdAsync(httpClient, transactionId, xsollaPayOptionSetting, xsollaTransactionData.IsSandbox);
                }
            }

            if (transaction == null)
            {
                return PaymentChannelVerifyReceiptResponse.Fail("未找到对应的交易记录");
            }

            // 验证交易状态
            if (!TransactionManager.IsTransactionSuccessful(transaction))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"交易状态无效: {transaction.Status}");
            }

            // 验证交易金额（如果提供了金额信息）
            if (xsollaTransactionData.Amount > 0 && Math.Abs(transaction.Amount.Amount - xsollaTransactionData.Amount) > 0.01m)
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"交易金额不匹配，期望: {xsollaTransactionData.Amount}, 实际: {transaction.Amount.Amount}");
            }

            // 验证货币类型（如果提供了货币信息）
            if (!string.IsNullOrEmpty(xsollaTransactionData.Currency) && !transaction.Amount.Currency.Equals(xsollaTransactionData.Currency, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"货币类型不匹配，期望: {xsollaTransactionData.Currency}, 实际: {transaction.Amount.Currency}");
            }

            // 验证用户ID（如果提供了用户ID）
            if (!string.IsNullOrEmpty(xsollaTransactionData.UserId) && !transaction.User.Id.Equals(xsollaTransactionData.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentChannelVerifyReceiptResponse.Fail($"用户ID不匹配，期望: {xsollaTransactionData.UserId}, 实际: {transaction.User.Id}");
            }

            // 验证商品SKU（如果提供了SKU信息）
            if (!string.IsNullOrEmpty(xsollaTransactionData.ProductSku))
            {
                var hasMatchingSku = transaction.Purchase.Items.Any(item => item.Sku.Equals(xsollaTransactionData.ProductSku, StringComparison.OrdinalIgnoreCase));

                if (!hasMatchingSku)
                {
                    return PaymentChannelVerifyReceiptResponse.Fail($"商品SKU不匹配: {xsollaTransactionData.ProductSku}");
                }
            }

            // 创建验证成功的结果
            var verificationResult = new XsollaVerifyReceiptResponse
            {
                IsSuccess = true,
                OrderId = transaction.TransactionId.ToString(),
                RoleId = transaction.User.Id,
                Sku = transaction.Purchase.Items.FirstOrDefault()?.Sku ?? string.Empty,
                TransactionId = transaction.TransactionId.ToString(),
                ChannelTransactionId = transaction.TransactionId.ToString(),
                MerchantOrderId = entity.OrderId,
                AccountId = transaction.User.Id,
                ExternalId = transaction.ExternalId,
                Amount = transaction.Amount.Amount,
                Currency = transaction.Amount.Currency,
                TransactionStatus = transaction.Status,
                OrderStatus = transaction.Status,
                IdempotencyKey = transaction.TransactionId.ToString(),
                TransactionTime = transaction.UpdatedAt,
                IsSandbox = transaction.IsSandbox,
                UserEmail = transaction.User.Email,
                Items = transaction.Purchase.Items.Select(item => new XsollaVerifyReceiptItem
                {
                    Sku = item.Sku,
                    Name = item.Name,
                    Quantity = item.Quantity,
                    Price = item.Price.Amount,
                    Currency = item.Price.Currency,
                }).ToList(),
            };

            return PaymentChannelVerifyReceiptResponse.Success(entity, verificationResult);
        }
        catch (Exception ex)
        {
            return PaymentChannelVerifyReceiptResponse.Fail($"验证Xsolla交易时发生错误: {ex.Message}");
        }
    }

    private static string ValidateSettings(XsollaPayOptionSetting settings)
    {
        if (settings.XsollaPaymentMerchantId <= 0)
        {
            return "Xsolla商户ID未配置";
        }

        if (settings.XsollaPaymentProjectId <= 0)
        {
            return "Xsolla项目ID未配置";
        }

        if (string.IsNullOrWhiteSpace(settings.XsollaPaymentApiKey))
        {
            return "Xsolla API密钥未配置";
        }

        if (string.IsNullOrWhiteSpace(settings.XsollaPaymentWebHookSecretKey))
        {
            return "Xsolla WebHook密钥未配置";
        }

        return string.Empty;
    }
}