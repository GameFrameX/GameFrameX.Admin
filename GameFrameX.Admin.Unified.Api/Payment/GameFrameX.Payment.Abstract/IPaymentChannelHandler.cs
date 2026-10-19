using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Payment.Abstract.Models;

namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 支付渠道处理器接口
/// </summary>
public interface IPaymentChannelHandler
{
    /// <summary>
    /// 支付渠道名称
    /// </summary>
    string ChannelName { get; }

    /// <summary>
    /// 创建订单
    /// </summary>
    /// <param name="entity">订单实体</param>
    /// <param name="input">输入参数</param>
    /// <param name="user">用户信息</param>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="optionSetting">支付配置</param>
    /// <returns>订单创建结果</returns>
    Task<PaymentChannelCreateOrderResponse> CreateOrderAsync(IGameOrderBase entity, object input, object user, HttpClient httpClient, IPaymentOptionSetting optionSetting);

    /// <summary>
    /// 验证支付回执（如支付通知或收据）
    /// </summary>
    /// <param name="entity">订单实体</param>
    /// <param name="transactionData">交易数据</param>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="optionSetting">支付配置</param>
    /// <returns>验证结果</returns>
    Task<PaymentChannelVerifyReceiptResponse> VerifyReceiptAsync(IGameOrderBase entity, ITransactionData transactionData, HttpClient httpClient, IPaymentOptionSetting optionSetting);
}