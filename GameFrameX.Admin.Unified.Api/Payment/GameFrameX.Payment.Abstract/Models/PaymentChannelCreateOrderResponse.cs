// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Payment.Abstract.Interface;

namespace GameFrameX.Payment.Abstract.Models;

/// <summary>
/// 支付渠道处理结果
/// </summary>
public sealed class PaymentChannelCreateOrderResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// 订单实体
    /// </summary>
    public IGameOrderBase OrderEntity { get; set; } = new GameOrderBase();

    /// <summary>
    /// 输出结果
    /// </summary>
    public IPaymentCreateOrderResponse Output { get; set; } = new EmptyPaymentCreateOrderResponse();

    /// <summary>
    /// 创建成功结果
    /// </summary>
    public static PaymentChannelCreateOrderResponse Success(IGameOrderBase entity, IPaymentCreateOrderResponse output)
    {
        return new PaymentChannelCreateOrderResponse
        {
            IsSuccess = true,
            OrderEntity = entity,
            Output = output,
        };
    }

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static PaymentChannelCreateOrderResponse Fail(string errorMessage)
    {
        return new PaymentChannelCreateOrderResponse
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
        };
    }
}