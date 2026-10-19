// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Models;
using GameFrameX.Payment.Apple.AppStoreServer;
using GameFrameX.Payment.Apple.AppStoreServer.Models;
using GameFrameX.Payment.Apple.Option;

namespace GameFrameX.Payment.Apple.Refund;

/// <summary>
/// Apple 退款管理器（C310 / GFX-718）。
/// <para>Apple 政策：退款由用户发起、App Store 裁决，商家无主动退款 API——RefundAsync 显式失败（不静默缺失）。</para>
/// <para>可用能力：退款历史查询（App Store Server API v2 /inApps/v2/refund/lookup）与消费信息上报
/// （/inApps/v2/transactions/consumption，用户申请退款时辅助 Apple 裁决）。</para>
/// <para>苹果双轨架构不变（C271：旧 verifyReceipt 与 AppStoreServer 并行），本实现只走 AppStoreServer 轨道。</para>
/// </summary>
public sealed class AppleRefundManager : IRefundManager
{
    /// <summary>
    /// 渠道名称（与 ApplePaymentOptionSetting 的 [PaymentChannel("apple")] 对齐）。
    /// </summary>
    public string ChannelName
    {
        get { return "apple"; }
    }

    /// <summary>
    /// Apple 不支持商家主动发起退款（官方无此 API，退款由用户发起、App Store 裁决）。
    /// </summary>
    public bool SupportsProactiveRefund
    {
        get { return false; }
    }

    /// <summary>
    /// 商家主动退款：Apple 政策不支持，显式失败。
    /// </summary>
    public Task<PaymentChannelRefundResponse> RefundAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(context);

        return Task.FromResult(PaymentChannelRefundResponse.Fail(
            "Apple 不支持商家主动发起退款（Apple 政策：退款由用户发起、App Store 裁决）；可通过退款历史查询确认退款状态，或调用消费信息上报辅助 Apple 裁决"));
    }

    /// <summary>
    /// 查询 Apple 退款历史（按 originalTransactionId 查 /inApps/v2/refund/lookup，有记录即已退款）。
    /// </summary>
    public async Task<PaymentChannelRefundQueryResponse> QueryRefundAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(optionSetting);
        ArgumentNullException.ThrowIfNull(context);

        if (optionSetting is not ApplePaymentOptionSetting settings)
        {
            return PaymentChannelRefundQueryResponse.Fail("Apple 退款查询配置类型错误");
        }

        if (string.IsNullOrWhiteSpace(context.TransactionId))
        {
            return PaymentChannelRefundQueryResponse.Fail("Apple 退款查询失败：交易号（originalTransactionId）为空");
        }

        var apiClient = CreateApiClient(settings, context.IsSandbox, httpClient);

        try
        {
            var history = await apiClient.GetRefundHistory(context.TransactionId, null);
            var signedTransactionCount = history?.SignedTransactions?.Count ?? 0;
            var status = signedTransactionCount > 0 ? RefundChannelStatusEnum.Refunded : RefundChannelStatusEnum.None;
            var raw = $"transactionId={context.TransactionId}, refundCount={signedTransactionCount}, hasMore={history?.HasMore}";
            return PaymentChannelRefundQueryResponse.Success(status, raw);
        }
        catch (ApiException ex)
        {
            return PaymentChannelRefundQueryResponse.Fail($"Apple 退款历史查询失败：transactionId={context.TransactionId}, httpStatus={ex.HttpStatusCode}, apiError={ex.ApiError}, {ex.ErrorMessage}");
        }
    }

    /// <summary>
    /// 发送消费信息（/inApps/v2/transactions/consumption）：用户申请退款时上报道具已正常发放，辅助 Apple 裁决。
    /// </summary>
    public async Task<PaymentChannelRefundResponse> SendConsumptionInformationAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(optionSetting);
        ArgumentNullException.ThrowIfNull(context);

        if (optionSetting is not ApplePaymentOptionSetting settings)
        {
            return PaymentChannelRefundResponse.Fail("Apple 消费信息上报配置类型错误");
        }

        if (string.IsNullOrWhiteSpace(context.TransactionId))
        {
            return PaymentChannelRefundResponse.Fail("Apple 消费信息上报失败：交易号为空");
        }

        var apiClient = CreateApiClient(settings, context.IsSandbox, httpClient);
        var consumptionRequest = new ConsumptionRequest
        {
            CustomerConsented = true,
            ConsumptionPercentage = 100,
            DeliveryStatus = DeliveryStatus.DELIVERED,
            SampleContentProvided = false,
        };

        try
        {
            var accepted = await apiClient.SendConsumptionInformation(context.TransactionId, consumptionRequest);
            if (!accepted)
            {
                return PaymentChannelRefundResponse.Fail($"Apple 消费信息上报未确认成功：transactionId={context.TransactionId}");
            }

            return PaymentChannelRefundResponse.Success(context.TransactionId, $"transactionId={context.TransactionId}, consumptionReported");
        }
        catch (ApiException ex)
        {
            return PaymentChannelRefundResponse.Fail($"Apple 消费信息上报失败：transactionId={context.TransactionId}, httpStatus={ex.HttpStatusCode}, apiError={ex.ApiError}, {ex.ErrorMessage}");
        }
    }

    /// <summary>
    /// 构造 App Store Server API 客户端（与 ApplePaymentHandler 双轨验签同款参数来源；沙箱标记来自订单）。
    /// </summary>
    private static AppStoreServerApiClient CreateApiClient(ApplePaymentOptionSetting settings, bool isSandbox, HttpClient httpClient)
    {
        return new AppStoreServerApiClient(
            settings.ApplePaymentPrivateKey,
            settings.ApplePaymentApiKeyId,
            settings.ApplePaymentIssuerId,
            settings.ApplePaymentBundleId,
            isSandbox ? ApplePayEnvironment.Sandbox : ApplePayEnvironment.Production,
            httpClient);
    }
}
