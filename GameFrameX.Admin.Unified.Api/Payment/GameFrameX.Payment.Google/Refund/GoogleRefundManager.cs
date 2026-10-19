// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Models;
using GameFrameX.Payment.Google.Option;

namespace GameFrameX.Payment.Google.Refund;

/// <summary>
/// Google Play 退款管理器（C310 / GFX-718）。
/// <para>官方能力：Android Publisher API 无商家主动退款端点（退款在 Google Play Console 或由 Google 支持处理）——
/// RefundAsync 显式失败（不静默缺失）；退款状态查询走 purchases.voidedpurchases.list（<see cref="GooglePlayApiManager.GetVoidedPurchasesAsync"/>）。</para>
/// </summary>
public sealed class GoogleRefundManager : IRefundManager
{
    /// <summary>
    /// 渠道名称（与 GooglePaymentOptionSetting 的 [PaymentChannel("google")] 对齐）。
    /// </summary>
    public string ChannelName
    {
        get { return "google"; }
    }

    /// <summary>
    /// Google Play 官方未提供商家主动退款 API。
    /// </summary>
    public bool SupportsProactiveRefund
    {
        get { return false; }
    }

    /// <summary>
    /// 商家主动退款：官方无 API，显式失败。
    /// </summary>
    public Task<PaymentChannelRefundResponse> RefundAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(context);

        return Task.FromResult(PaymentChannelRefundResponse.Fail(
            "Google Play 不支持商家主动发起退款（官方未提供退款 API，需在 Google Play Console 或由 Google 用户支持处理）；可通过作废交易查询确认退款状态"));
    }

    /// <summary>
    /// 查询 Google 已作废（退款）交易：voided purchases 仅支持按 purchaseToken 匹配；
    /// 订单侧 ProviderTransactionId 持久化的是 Google 订单号（GPA.*），未持久化 purchaseToken 时无法精确判断，查询结果会如实注明口径。
    /// </summary>
    public async Task<PaymentChannelRefundQueryResponse> QueryRefundAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(optionSetting);
        ArgumentNullException.ThrowIfNull(context);

        if (optionSetting is not GooglePaymentOptionSetting settings)
        {
            return PaymentChannelRefundQueryResponse.Fail("Google 退款查询配置类型错误");
        }

        if (string.IsNullOrWhiteSpace(settings.GooglePaymentPackageName))
        {
            return PaymentChannelRefundQueryResponse.Fail("Google 退款查询失败：应用包名（GooglePaymentPackageName）未配置");
        }

        try
        {
            await GooglePlayApiManager.InitializeAsync(settings);
            var voidedPurchases = await GooglePlayApiManager.GetVoidedPurchasesAsync(settings.GooglePaymentPackageName);

            foreach (var voidedPurchase in voidedPurchases)
            {
                if (voidedPurchase.PurchaseToken == context.TransactionId)
                {
                    return PaymentChannelRefundQueryResponse.Success(RefundChannelStatusEnum.Refunded,
                        $"purchaseToken 命中已作废交易：transactionId={context.TransactionId}, voidedTimeMillis={voidedPurchase.VoidedTimeMillis}, voidedReason={voidedPurchase.VoidedReason}");
                }
            }

            return PaymentChannelRefundQueryResponse.Success(RefundChannelStatusEnum.None,
                $"未在已作废交易中命中 transactionId={context.TransactionId}（注意：Google 按.purchaseToken 匹配，订单未持久化 purchaseToken 时该查询无法精确判断）");
        }
        catch (Exception ex)
        {
            return PaymentChannelRefundQueryResponse.Fail($"Google 作废交易查询失败: {ex.Message}");
        }
    }

    /// <summary>
    /// Google Play 无消费信息上报能力，显式失败。
    /// </summary>
    public Task<PaymentChannelRefundResponse> SendConsumptionInformationAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        return Task.FromResult(PaymentChannelRefundResponse.Fail("Google Play 不支持消费信息上报（该能力仅 Apple App Store Server API 提供）"));
    }
}
