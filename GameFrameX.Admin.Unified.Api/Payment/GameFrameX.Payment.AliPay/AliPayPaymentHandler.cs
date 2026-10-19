using System.Globalization;
using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using GameFrameX.Foundation.Json;
using GameFrameX.Foundation.Logger;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Payment.Abstract.Models;

namespace GameFrameX.Payment.AliPay;

/// <summary>
/// 支付宝支付处理器
/// </summary>
public class AliPayPaymentHandler : IPaymentChannelHandler
{
    /// <summary>
    /// 渠道名称
    /// </summary>
    public string ChannelName
    {
        get { return "alipay"; }
    }

    /// <summary>
    /// 下单一个商品
    /// </summary>
    /// <returns></returns>
    public Task<PaymentChannelCreateOrderResponse> CreateOrderAsync(IGameOrderBase entity, object input, object user, HttpClient httpClient, IPaymentOptionSetting optionSetting)
    {
        var settings = optionSetting as AliPayOptionSetting;
        if (settings == null)
        {
            return Task.FromResult(PaymentChannelCreateOrderResponse.Fail("配置类型错误"));
        }

        try
        {
            var validationError = ValidateSettings(settings);
            if (!string.IsNullOrEmpty(validationError))
            {
                return Task.FromResult(PaymentChannelCreateOrderResponse.Fail(validationError));
            }

            var merchantPrivateKey = NormalizePrivateKey(settings.AliPayMerchantPrivateKey);
            IAopClient client = new DefaultAopClient(settings.AliPayGatewayUrl, settings.AliPayAppId, merchantPrivateKey, settings.AliPayFormat, "1.0", settings.AliPaySignType, settings.AliPayPublicKey, settings.AliPayCharset, false);

            var request = new AlipayTradeAppPayRequest();
            var model = new AlipayTradeAppPayModel
            {
                Body = entity.Sku,
                Subject = entity.Sku,
                OutTradeNo = entity.OrderId,
                TimeoutExpress = "30m",
                TotalAmount = FormatAmount(entity.Amount),
                ProductCode = settings.AliPayProductCode,
                PassbackParams = JsonHelper.Serialize(new
                {
                    entity.RoleId,
                    entity.AreaId,
                    entity.OrderId,
                }),
            };

            request.SetBizModel(model);
            request.SetNotifyUrl(settings.AliPayNotifyUrl);
            request.SetReturnUrl(settings.AliPayReturnUrl);

            var response = client.SdkExecute(request);
            if (response.IsError)
            {
                LogHelper.Error($"支付宝下单失败: {response.Code} {response.Msg} {response.SubCode} {response.SubMsg}");
                return Task.FromResult(PaymentChannelCreateOrderResponse.Fail($"支付宝下单失败: {response.SubMsg ?? response.Msg}"));
            }

            var output = new AlipayCreateOrderResponse
            {
                IsNeedPay = true,
                OrderId = entity.OrderId,
                Sku = entity.Sku,
                RoleId = entity.RoleId,
                Token = response.Body,
            };

            return Task.FromResult(PaymentChannelCreateOrderResponse.Success(entity, output));
        }
        catch (Exception ex)
        {
            LogHelper.Error("支付宝创建订单异常", ex);
            return Task.FromResult(PaymentChannelCreateOrderResponse.Fail($"支付宝创建订单异常: {ex.Message}"));
        }
    }

    /// <summary>
    /// 验证支付回执
    /// </summary>
    public Task<PaymentChannelVerifyReceiptResponse> VerifyReceiptAsync(IGameOrderBase entity, ITransactionData transactionData, HttpClient httpClient, IPaymentOptionSetting optionSetting)
    {
        var settings = optionSetting as AliPayOptionSetting;
        if (settings == null)
        {
            return Task.FromResult(new PaymentChannelVerifyReceiptResponse { IsSuccess = false, ErrorMessage = "配置类型错误", });
        }

        try
        {
            IAopClient client = new DefaultAopClient(
                settings.AliPayGatewayUrl,
                settings.AliPayAppId,
                settings.AliPayMerchantPrivateKey,
                settings.AliPayFormat,
                "1.0",
                settings.AliPaySignType,
                settings.AliPayPublicKey,
                settings.AliPayCharset,
                false
            );

            var request = new AlipayTradeQueryRequest();
            request.BizContent = JsonHelper.Serialize(new
            {
                out_trade_no = entity.OrderId,
            });

            var response = client.Execute(request);

            if (response.IsError)
            {
                LogHelper.Error($"支付宝查询订单失败: {response.Code} {response.Msg} {response.SubCode} {response.SubMsg}");
                return Task.FromResult(new PaymentChannelVerifyReceiptResponse { IsSuccess = false, ErrorMessage = $"查询失败: {response.SubMsg ?? response.Msg}", });
            }

            // 交易状态：WAIT_BUYER_PAY（交易创建，等待买家付款）、TRADE_CLOSED（未付款交易超时关闭，或支付完成后全额退款）、TRADE_SUCCESS（交易支付成功）、TRADE_FINISHED（交易结束，不可退款）
            if (response.TradeStatus == "TRADE_SUCCESS" || response.TradeStatus == "TRADE_FINISHED")
            {
                var result = new AliPayVerifyReceiptResponse
                {
                    IsSuccess = true,
                    OrderId = entity.OrderId,
                    Sku = entity.Sku,
                    RoleId = entity.RoleId,
                    ChannelTransactionId = response.TradeNo ?? string.Empty,
                    MerchantOrderId = entity.OrderId,
                    AccountId = entity.AccountId,
                    Amount = entity.Amount / 100m,
                    Currency = entity.Country,
                    IsSandbox = entity.IsSandbox,
                    OrderStatus = response.TradeStatus,
                    IdempotencyKey = response.TradeNo ?? entity.OrderId,
                    TradeNo = response.TradeNo ?? string.Empty, // 支付宝交易号
                };
                return Task.FromResult(PaymentChannelVerifyReceiptResponse.Success(entity, result));
            }

            return Task.FromResult(PaymentChannelVerifyReceiptResponse.Fail($"订单状态: {response.TradeStatus}"));
        }
        catch (Exception ex)
        {
            LogHelper.Error("支付宝验证订单异常", ex);
            return Task.FromResult(PaymentChannelVerifyReceiptResponse.Fail($"验证异常: {ex.Message}"));
        }
    }

    /// <summary>
    /// 从输入参数中获取金额
    /// </summary>
    private static string ValidateSettings(AliPayOptionSetting settings)
    {
        if (string.IsNullOrWhiteSpace(settings.AliPayAppId))
        {
            return "支付宝 AppId 未配置";
        }

        if (string.IsNullOrWhiteSpace(settings.AliPayMerchantPrivateKey))
        {
            return "支付宝商户私钥未配置";
        }

        if (string.IsNullOrWhiteSpace(settings.AliPayPublicKey))
        {
            return "支付宝公钥未配置";
        }

        if (string.IsNullOrWhiteSpace(settings.AliPayNotifyUrl))
        {
            return "支付宝异步通知地址未配置";
        }

        if (string.IsNullOrWhiteSpace(settings.AliPayReturnUrl))
        {
            return "支付宝同步跳转地址未配置";
        }

        return string.Empty;
    }

    private static string NormalizePrivateKey(string privateKey)
    {
        return privateKey
               .Replace("-----BEGIN PRIVATE KEY-----", "")
               .Replace("-----END PRIVATE KEY-----", "")
               .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
               .Replace("-----END RSA PRIVATE KEY-----", "")
               .Replace("\r", "")
               .Replace("\n", "")
               .Replace(" ", "");
    }

    private static string FormatAmount(long amountInCents)
    {
        return (amountInCents / 100m).ToString("0.00", CultureInfo.InvariantCulture);
    }
}