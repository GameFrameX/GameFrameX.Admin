// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json;
using GameFrameX.Foundation.Logger;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Payment.Abstract.Models;
using SKIT.FlurlHttpClient.Wechat.TenpayV3;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;

namespace GameFrameX.Payment.Wechat;

/// <summary>
/// 微信支付处理器（商户直连）。支持三种调起场景（下单请求 Scene 字段，缺省 app）：
/// jsapi（小程序 / 公众号网页，需 OpenId）/ app（原生 APP）/ native（Web 商城扫码）。
/// Token 承载对应调起参数 JSON；金额单位沿用订单实体「分」，直接对接微信 total（分）。
/// </summary>
public class WechatPaymentHandler : IPaymentChannelHandler
{
    /// <summary>
    /// JSAPI 调起场景（小程序 / 公众号网页）
    /// </summary>
    public const string SceneJsapi = "jsapi";

    /// <summary>
    /// APP 调起场景（原生 APP）
    /// </summary>
    public const string SceneApp = "app";

    /// <summary>
    /// Native 扫码调起场景（Web 商城）
    /// </summary>
    public const string SceneNative = "native";

    /// <summary>
    /// 支付渠道名称
    /// </summary>
    public string ChannelName
    {
        get { return "wechat"; }
    }

    /// <summary>
    /// 微信预下单：按 Scene 调用 TenpayV3 对应接口生成 prepay，并组装调起参数
    /// </summary>
    public async Task<PaymentChannelCreateOrderResponse> CreateOrderAsync(IGameOrderBase entity, object input, object user, HttpClient httpClient, IPaymentOptionSetting optionSetting)
    {
        var settings = optionSetting as WechatPayOptionSetting;
        if (settings == null)
        {
            return PaymentChannelCreateOrderResponse.Fail("配置类型错误");
        }

        var validationError = ValidateSettings(settings);
        if (!string.IsNullOrEmpty(validationError))
        {
            return PaymentChannelCreateOrderResponse.Fail(validationError);
        }

        // 渠道附加参数（Scene / OpenId）由下单请求携带；Payment.Wechat 不引用 Client.Api DTO，按属性名反射软读取
        var scene = NormalizeScene(ResolveInputString(input, "Scene"));
        var openId = ResolveInputString(input, "OpenId");
        if (scene == SceneJsapi && string.IsNullOrWhiteSpace(openId))
        {
            return PaymentChannelCreateOrderResponse.Fail("JSAPI 支付必须提供 OpenId");
        }

        try
        {
            var client = await WechatTenpayClientFactory.GetClientAsync(settings);
            var attachment = JsonSerializer.Serialize(new
            {
                entity.RoleId,
                entity.AreaId,
                entity.OrderId,
            });

            if (scene == SceneJsapi)
            {
                var response = await client.ExecuteCreatePayTransactionJsapiAsync(new CreatePayTransactionJsapiRequest
                {
                    OutTradeNumber = entity.OrderId,
                    AppId = settings.WechatPayAppId,
                    Description = $"商品 {entity.Sku}",
                    Attachment = attachment,
                    NotifyUrl = settings.WechatPayNotifyUrl,
                    Amount = new CreatePayTransactionJsapiRequest.Types.Amount { Total = (int)entity.Amount, },
                    Payer = new CreatePayTransactionJsapiRequest.Types.Payer { OpenId = openId!, },
                });
                if (!response.IsSuccessful())
                {
                    LogHelper.Error($"微信支付 JSAPI 下单失败: {response.ErrorMessage}");
                    return PaymentChannelCreateOrderResponse.Fail($"微信支付下单失败: {response.ErrorMessage}");
                }

                var parameters = client.GenerateParametersForJsapiPayRequest(settings.WechatPayAppId, response.PrepayId);
                return BuildSuccess(entity, JsonSerializer.Serialize(parameters));
            }

            if (scene == SceneApp)
            {
                var response = await client.ExecuteCreatePayTransactionAppAsync(new CreatePayTransactionAppRequest
                {
                    OutTradeNumber = entity.OrderId,
                    AppId = settings.WechatPayAppId,
                    Description = $"商品 {entity.Sku}",
                    Attachment = attachment,
                    NotifyUrl = settings.WechatPayNotifyUrl,
                    Amount = new CreatePayTransactionAppRequest.Types.Amount { Total = (int)entity.Amount, },
                });
                if (!response.IsSuccessful())
                {
                    LogHelper.Error($"微信支付 APP 下单失败: {response.ErrorMessage}");
                    return PaymentChannelCreateOrderResponse.Fail($"微信支付下单失败: {response.ErrorMessage}");
                }

                var parameters = client.GenerateParametersForAppPayRequest(settings.WechatPayMerchantId, settings.WechatPayAppId, response.PrepayId);
                return BuildSuccess(entity, JsonSerializer.Serialize(parameters));
            }

            var nativeResponse = await client.ExecuteCreatePayTransactionNativeAsync(new CreatePayTransactionNativeRequest
            {
                OutTradeNumber = entity.OrderId,
                AppId = settings.WechatPayAppId,
                Description = $"商品 {entity.Sku}",
                Attachment = attachment,
                NotifyUrl = settings.WechatPayNotifyUrl,
                Amount = new CreatePayTransactionNativeRequest.Types.Amount { Total = (int)entity.Amount, },
            });
            if (!nativeResponse.IsSuccessful())
            {
                LogHelper.Error($"微信支付 Native 下单失败: {nativeResponse.ErrorMessage}");
                return PaymentChannelCreateOrderResponse.Fail($"微信支付下单失败: {nativeResponse.ErrorMessage}");
            }

            return BuildSuccess(entity, JsonSerializer.Serialize(new { codeUrl = nativeResponse.QrcodeUrl, }));
        }
        catch (Exception ex)
        {
            LogHelper.Error("微信支付创建订单异常", ex);
            return PaymentChannelCreateOrderResponse.Fail($"微信支付创建订单异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 主动查单验证：按商户订单号查询微信支付订单，SUCCESS 状态才视为支付成功
    /// </summary>
    public async Task<PaymentChannelVerifyReceiptResponse> VerifyReceiptAsync(IGameOrderBase entity, ITransactionData transactionData, HttpClient httpClient, IPaymentOptionSetting optionSetting)
    {
        var settings = optionSetting as WechatPayOptionSetting;
        if (settings == null)
        {
            return PaymentChannelVerifyReceiptResponse.Fail("配置类型错误");
        }

        try
        {
            var client = await WechatTenpayClientFactory.GetClientAsync(settings);
            var response = await client.ExecuteGetPayTransactionByOutTradeNumberAsync(new GetPayTransactionByOutTradeNumberRequest
            {
                MerchantId = settings.WechatPayMerchantId,
                OutTradeNumber = entity.OrderId,
            });
            if (!response.IsSuccessful())
            {
                LogHelper.Error($"微信支付查询订单失败: {response.ErrorMessage}");
                return PaymentChannelVerifyReceiptResponse.Fail($"查询失败: {response.ErrorMessage}");
            }

            // 交易状态：SUCCESS（支付成功）/ NOTPAY（未支付）/ CLOSED（已关闭）/ REFUND（已退款）/ USERPAYING（用户支付中）/ PAYERROR（支付失败）
            if (response.TradeState == "SUCCESS")
            {
                var result = new WechatVerifyReceiptResponse
                {
                    IsSuccess = true,
                    OrderId = entity.OrderId,
                    Sku = entity.Sku,
                    RoleId = entity.RoleId,
                    ChannelTransactionId = response.TransactionId ?? string.Empty,
                    MerchantOrderId = entity.OrderId,
                    AccountId = entity.AccountId,
                    Amount = response.Amount?.Total / 100m ?? 0m,
                    Currency = entity.Country,
                    IsSandbox = entity.IsSandbox,
                    OrderStatus = response.TradeState,
                    IdempotencyKey = response.TransactionId ?? entity.OrderId,
                    TransactionId = response.TransactionId ?? string.Empty,
                };
                return PaymentChannelVerifyReceiptResponse.Success(entity, result);
            }

            return PaymentChannelVerifyReceiptResponse.Fail($"订单状态: {response.TradeState}");
        }
        catch (Exception ex)
        {
            LogHelper.Error("微信支付验证订单异常", ex);
            return PaymentChannelVerifyReceiptResponse.Fail($"验证异常: {ex.Message}");
        }
    }

    private static PaymentChannelCreateOrderResponse BuildSuccess(IGameOrderBase entity, string token)
    {
        var output = new WechatCreateOrderResponse
        {
            IsNeedPay = true,
            OrderId = entity.OrderId,
            Sku = entity.Sku,
            RoleId = entity.RoleId,
            Token = token,
        };
        return PaymentChannelCreateOrderResponse.Success(entity, output);
    }

    private static string ValidateSettings(WechatPayOptionSetting settings)
    {
        if (string.IsNullOrWhiteSpace(settings.WechatPayMerchantId))
        {
            return "微信支付商户号未配置";
        }

        if (string.IsNullOrWhiteSpace(settings.WechatPayAppId))
        {
            return "微信支付应用 ID 未配置";
        }

        if (string.IsNullOrWhiteSpace(settings.WechatPayApiV3Secret))
        {
            return "微信支付 APIv3 密钥未配置";
        }

        if (string.IsNullOrWhiteSpace(settings.WechatPayCertificateSerialNumber))
        {
            return "微信支付商户证书序列号未配置";
        }

        if (string.IsNullOrWhiteSpace(settings.WechatPayCertificatePrivateKey))
        {
            return "微信支付商户私钥未配置";
        }

        if (string.IsNullOrWhiteSpace(settings.WechatPayNotifyUrl))
        {
            return "微信支付回调通知地址未配置";
        }

        return string.Empty;
    }

    private static string NormalizeScene(string? scene)
    {
        var normalized = scene?.Trim().ToLowerInvariant();
        if (normalized == SceneJsapi || normalized == SceneNative)
        {
            return normalized;
        }

        // 缺省 app：游戏客户端主链路为原生 APP 调起；jsapi 必须显式携带（需 OpenId），native 由 Web 商城显式指定
        return SceneApp;
    }

    /// <summary>
    /// 反射软读取下单请求的字符串属性（Scene / OpenId）：Payment.Wechat 不引用 Client.Api 的请求 DTO，
    /// 避免卫星层反向依赖宿主；属性不存在时返回 null 走缺省语义
    /// </summary>
    private static string? ResolveInputString(object? input, string propertyName)
    {
        var property = input?.GetType().GetProperty(propertyName);
        return property?.GetValue(input) as string;
    }
}
