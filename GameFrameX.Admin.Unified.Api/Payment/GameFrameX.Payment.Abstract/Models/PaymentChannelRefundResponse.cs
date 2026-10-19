// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Abstract.Models;

/// <summary>
/// 渠道退款发起 / 消费信息上报结果（C310 / GFX-718）。
/// </summary>
public sealed class PaymentChannelRefundResponse
{
    /// <summary>
    /// 是否成功。
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 失败时的错误消息（含渠道返回的错误码与描述）。
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// 渠道退款单号（如支付宝退款后的 trade_no；渠道未返回时为空）。
    /// </summary>
    public string ChannelRefundId { get; set; } = string.Empty;

    /// <summary>
    /// 渠道原始响应（审计留档）。
    /// </summary>
    public string RawResponse { get; set; } = string.Empty;

    /// <summary>
    /// 创建成功结果。
    /// </summary>
    /// <param name="channelRefundId">渠道退款单号</param>
    /// <param name="rawResponse">渠道原始响应</param>
    /// <returns>成功结果实例</returns>
    public static PaymentChannelRefundResponse Success(string channelRefundId, string rawResponse)
    {
        return new PaymentChannelRefundResponse
        {
            IsSuccess = true,
            ChannelRefundId = channelRefundId ?? string.Empty,
            RawResponse = rawResponse ?? string.Empty,
        };
    }

    /// <summary>
    /// 创建失败结果。
    /// </summary>
    /// <param name="errorMessage">错误消息</param>
    /// <param name="rawResponse">渠道原始响应（可选）</param>
    /// <returns>失败结果实例</returns>
    public static PaymentChannelRefundResponse Fail(string errorMessage, string rawResponse = "")
    {
        return new PaymentChannelRefundResponse
        {
            IsSuccess = false,
            ErrorMessage = errorMessage ?? string.Empty,
            RawResponse = rawResponse ?? string.Empty,
        };
    }
}
