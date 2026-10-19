// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Globalization;
using Aop.Api;
using Aop.Api.Domain;
using Aop.Api.Request;
using GameFrameX.Foundation.Logger;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Models;

namespace GameFrameX.Payment.AliPay.Refund;

/// <summary>
/// 支付宝退款管理器（C310 / GFX-718）：统一收单交易退款 alipay.trade.refund + 退款查询 alipay.trade.fastpay.refund.query。
/// <para>全额退款：out_request_no 取商户订单号——同单重复退款支付宝按幂等语义返回原退款结果。</para>
/// <para>SDK 为同步调用，按现有 AliPayPaymentHandler 同款 DefaultAopClient 构造与 Task.FromResult 包装风格。</para>
/// </summary>
public sealed class AliPayRefundManager : IRefundManager
{
    /// <summary>
    /// 渠道名称（与 AliPayOptionSetting 的 [PaymentChannel("alipay")] 对齐）。
    /// </summary>
    public string ChannelName
    {
        get { return "alipay"; }
    }

    /// <summary>
    /// 支付宝支持商家主动发起退款（alipay.trade.refund）。
    /// </summary>
    public bool SupportsProactiveRefund
    {
        get { return true; }
    }

    /// <summary>
    /// 发起支付宝全额退款。
    /// </summary>
    public Task<PaymentChannelRefundResponse> RefundAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(optionSetting);
        ArgumentNullException.ThrowIfNull(context);

        if (optionSetting is not AliPayOptionSetting settings)
        {
            return Task.FromResult(PaymentChannelRefundResponse.Fail("支付宝退款配置类型错误"));
        }

        if (string.IsNullOrWhiteSpace(context.OutTradeNo))
        {
            return Task.FromResult(PaymentChannelRefundResponse.Fail("支付宝退款失败：商户订单号（outTradeNo）为空"));
        }

        try
        {
            IAopClient client = new DefaultAopClient(
                settings.AliPayGatewayUrl,
                settings.AliPayAppId,
                NormalizePrivateKey(settings.AliPayMerchantPrivateKey),
                settings.AliPayFormat,
                "1.0",
                settings.AliPaySignType,
                settings.AliPayPublicKey,
                settings.AliPayCharset,
                false);

            var request = new AlipayTradeRefundRequest();
            var model = new AlipayTradeRefundModel
            {
                OutTradeNo = context.OutTradeNo,
                RefundAmount = FormatAmount(context.Amount),
                RefundReason = string.IsNullOrWhiteSpace(context.Description) ? "商家退款" : context.Description,
                // 全额退款幂等键：同单号重复退款（如管理员重试）支付宝按幂等语义返回原退款结果
                OutRequestNo = context.OutTradeNo,
            };
            request.SetBizModel(model);

            var response = client.Execute(request);
            if (response.IsError)
            {
                LogHelper.Error($"支付宝退款失败: {response.Code} {response.Msg} {response.SubCode} {response.SubMsg}");
                return Task.FromResult(PaymentChannelRefundResponse.Fail(
                    $"支付宝退款失败: {response.SubMsg ?? response.Msg} (code={response.Code}, subCode={response.SubCode})", response.Body));
            }

            return Task.FromResult(PaymentChannelRefundResponse.Success(response.TradeNo ?? string.Empty, response.Body));
        }
        catch (Exception ex)
        {
            LogHelper.Error("支付宝退款异常", ex);
            return Task.FromResult(PaymentChannelRefundResponse.Fail($"支付宝退款异常: {ex.Message}"));
        }
    }

    /// <summary>
    /// 查询支付宝退款状态（alipay.trade.fastpay.refund.query，按商户订单号 + 退款请求号）。
    /// </summary>
    public Task<PaymentChannelRefundQueryResponse> QueryRefundAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(optionSetting);
        ArgumentNullException.ThrowIfNull(context);

        if (optionSetting is not AliPayOptionSetting settings)
        {
            return Task.FromResult(PaymentChannelRefundQueryResponse.Fail("支付宝退款查询配置类型错误"));
        }

        if (string.IsNullOrWhiteSpace(context.OutTradeNo))
        {
            return Task.FromResult(PaymentChannelRefundQueryResponse.Fail("支付宝退款查询失败：商户订单号（outTradeNo）为空"));
        }

        try
        {
            IAopClient client = new DefaultAopClient(
                settings.AliPayGatewayUrl,
                settings.AliPayAppId,
                NormalizePrivateKey(settings.AliPayMerchantPrivateKey),
                settings.AliPayFormat,
                "1.0",
                settings.AliPaySignType,
                settings.AliPayPublicKey,
                settings.AliPayCharset,
                false);

            var request = new AlipayTradeFastpayRefundQueryRequest();
            var model = new AlipayTradeFastpayRefundQueryModel
            {
                OutTradeNo = context.OutTradeNo,
                OutRequestNo = context.OutTradeNo,
                TradeNo = context.TransactionId,
            };
            request.SetBizModel(model);

            var response = client.Execute(request);
            if (response.IsError)
            {
                // ACQ.TRADE_NOT_EXIST：退款不存在（未发起过退款），归一为 None 而非查询失败
                if (string.Equals(response.SubCode, "ACQ.TRADE_NOT_EXIST", StringComparison.Ordinal) || string.Equals(response.SubCode, "ACQ.REFUND_NOT_EXIST", StringComparison.Ordinal))
                {
                    return Task.FromResult(PaymentChannelRefundQueryResponse.Success(RefundChannelStatusEnum.None, response.Body));
                }

                LogHelper.Error($"支付宝退款查询失败: {response.Code} {response.Msg} {response.SubCode} {response.SubMsg}");
                return Task.FromResult(PaymentChannelRefundQueryResponse.Fail(
                    $"支付宝退款查询失败: {response.SubMsg ?? response.Msg} (code={response.Code}, subCode={response.SubCode})", response.Body));
            }

            var status = string.Equals(response.RefundStatus, "REFUND_SUCCESS", StringComparison.Ordinal) ? RefundChannelStatusEnum.Refunded : RefundChannelStatusEnum.Unknown;
            return Task.FromResult(PaymentChannelRefundQueryResponse.Success(status, response.Body));
        }
        catch (Exception ex)
        {
            LogHelper.Error("支付宝退款查询异常", ex);
            return Task.FromResult(PaymentChannelRefundQueryResponse.Fail($"支付宝退款查询异常: {ex.Message}"));
        }
    }

    /// <summary>
    /// 支付宝无消费信息上报能力，显式失败。
    /// </summary>
    public Task<PaymentChannelRefundResponse> SendConsumptionInformationAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        return Task.FromResult(PaymentChannelRefundResponse.Fail("支付宝不支持消费信息上报（该能力仅 Apple App Store Server API 提供）"));
    }

    /// <summary>
    /// 金额格式化（最小货币单位分 → 元字符串），与 AliPayPaymentHandler 下单同款口径。
    /// </summary>
    private static string FormatAmount(uint amountInCents)
    {
        return (amountInCents / 100m).ToString("0.00", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 商户私钥归一化（剥 PEM 头尾与空白），与 AliPayPaymentHandler 同款口径。
    /// </summary>
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
}
