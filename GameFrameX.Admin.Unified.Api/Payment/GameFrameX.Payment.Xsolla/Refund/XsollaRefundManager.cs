// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text;
using System.Text.Json;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Models;
using GameFrameX.Payment.Xsolla.Option;
using GameFrameX.Payment.Xsolla.Reports.Transactions;

namespace GameFrameX.Payment.Xsolla.Refund;

/// <summary>
/// Xsolla 退款管理器（C310 / GFX-718，恢复自 7c23840c 之前的 Reports/Transactions/RefundManager 并适配当前代码）。
/// <para>主动退款：PUT https://api.xsolla.com/merchant/v2/merchants/{merchantId}/reports/transactions/{transactionId}/refund（Basic 认证）。</para>
/// <para>与原实现的差异：鉴权 header 挂在 HttpRequestMessage 上（原实现写 HttpClient.DefaultRequestHeaders，
/// IHttpClientFactory 复用实例时会叠加 Authorization 头导致 401）。</para>
/// </summary>
public sealed class XsollaRefundManager : IRefundManager
{
    /// <summary>
    /// 渠道名称（与 XsollaPayOptionSetting 的 [PaymentChannel("xsolla")] 对齐）。
    /// </summary>
    public string ChannelName
    {
        get { return "xsolla"; }
    }

    /// <summary>
    /// Xsolla 支持商家主动发起退款。
    /// </summary>
    public bool SupportsProactiveRefund
    {
        get { return true; }
    }

    /// <summary>
    /// 发起 Xsolla 退款（全额）。
    /// </summary>
    public async Task<PaymentChannelRefundResponse> RefundAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(optionSetting);
        ArgumentNullException.ThrowIfNull(context);

        if (optionSetting is not XsollaPayOptionSetting settings)
        {
            return PaymentChannelRefundResponse.Fail("Xsolla 退款配置类型错误");
        }

        if (string.IsNullOrWhiteSpace(context.TransactionId))
        {
            return PaymentChannelRefundResponse.Fail("Xsolla 退款失败：渠道交易号（transactionId）为空");
        }

        var request = new HttpRequestMessage(HttpMethod.Put, $"https://api.xsolla.com/merchant/v2/merchants/{settings.XsollaPaymentMerchantId}/reports/transactions/{context.TransactionId}/refund");
        AddBasicAuthorization(request, settings);
        request.Content = new StringContent(new RefundRequest(context.Description).ToString(), Encoding.UTF8, "application/json");

        using (var response = await httpClient.SendAsync(request))
        {
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return PaymentChannelRefundResponse.Fail($"Xsolla 退款接口返回失败：HTTP {(int)response.StatusCode}, transactionId={context.TransactionId}, response={body}", body);
            }

            return PaymentChannelRefundResponse.Success(context.TransactionId, body);
        }
    }

    /// <summary>
    /// 查询 Xsolla 交易退款状态（Reports/Transactions/Search 按交易号查询，status 字段归一）。
    /// </summary>
    public async Task<PaymentChannelRefundQueryResponse> QueryRefundAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(optionSetting);
        ArgumentNullException.ThrowIfNull(context);

        if (optionSetting is not XsollaPayOptionSetting settings)
        {
            return PaymentChannelRefundQueryResponse.Fail("Xsolla 退款查询配置类型错误");
        }

        if (string.IsNullOrWhiteSpace(context.TransactionId))
        {
            return PaymentChannelRefundQueryResponse.Fail("Xsolla 退款查询失败：渠道交易号（transactionId）为空");
        }

        var findRequest = new Search.FindRequest
        {
            TransactionId = context.TransactionId,
        };
        var raw = await Search.FindAsync(httpClient, findRequest, settings);

        try
        {
            using (var document = JsonDocument.Parse(raw))
            {
                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    return PaymentChannelRefundQueryResponse.Fail($"Xsolla 退款查询返回非预期结构：{raw}", raw);
                }

                foreach (var element in document.RootElement.EnumerateArray())
                {
                    if (!element.TryGetProperty("status", out var statusElement))
                    {
                        continue;
                    }

                    var status = statusElement.GetString();
                    return PaymentChannelRefundQueryResponse.Success(MapStatus(status), raw);
                }

                return PaymentChannelRefundQueryResponse.Fail($"Xsolla 未查到该交易：transactionId={context.TransactionId}", raw);
            }
        }
        catch (JsonException ex)
        {
            return PaymentChannelRefundQueryResponse.Fail($"Xsolla 退款查询响应解析失败: {ex.Message}", raw);
        }
    }

    /// <summary>
    /// Xsolla 无消费信息上报能力，显式失败。
    /// </summary>
    public Task<PaymentChannelRefundResponse> SendConsumptionInformationAsync(RefundOrderContext context, IPaymentOptionSetting optionSetting, HttpClient httpClient)
    {
        return Task.FromResult(PaymentChannelRefundResponse.Fail("Xsolla 不支持消费信息上报（该能力仅 Apple App Store Server API 提供）"));
    }

    /// <summary>
    /// Xsolla 交易状态归一：refunded→Refunded；partiallyRefunded→PartiallyRefunded；canceled（取消或已退款口径）→Refunded；
    /// awaitingRefund→Pending；error→Failed；done→None；其余→Unknown。
    /// </summary>
    private static RefundChannelStatusEnum MapStatus(string? status)
    {
        switch (status)
        {
            case "refunded":
            case "canceled":
                return RefundChannelStatusEnum.Refunded;
            case "partiallyRefunded":
                return RefundChannelStatusEnum.PartiallyRefunded;
            case "awaitingRefund":
                return RefundChannelStatusEnum.Pending;
            case "error":
                return RefundChannelStatusEnum.Failed;
            case "done":
            case "created":
            case "processing":
            case "review":
                return RefundChannelStatusEnum.None;
            default:
                return RefundChannelStatusEnum.Unknown;
        }
    }

    /// <summary>
    /// Basic 认证头（merchantId:apiKey）挂请求消息，避免污染工厂复用的 HttpClient。
    /// </summary>
    private static void AddBasicAuthorization(HttpRequestMessage request, XsollaPayOptionSetting settings)
    {
        var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.XsollaPaymentMerchantId}:{settings.XsollaPaymentApiKey}"));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64String);
    }
}
