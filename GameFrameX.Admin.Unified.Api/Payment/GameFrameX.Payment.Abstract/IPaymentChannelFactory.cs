namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 支付渠道工厂接口
/// </summary>
public interface IPaymentChannelFactory
{
    /// <summary>
    /// 获取支付渠道处理器
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付渠道处理器</returns>
    IPaymentChannelHandler GetHandler(string channelName);

    /// <summary>
    /// 检查是否支持指定的支付渠道
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>是否支持</returns>
    bool IsSupported(string channelName);

    /// <summary>
    /// 获取所有支持的支付渠道名称
    /// </summary>
    /// <returns>支付渠道名称列表</returns>
    IEnumerable<string> GetSupportedChannels();

    /// <summary>
    /// 获取支付渠道配置
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置</returns>
    IPaymentOptionSetting GetOptionSetting(string channelName);

    /// <summary>
    /// 获取支付渠道候选配置
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置列表</returns>
    IEnumerable<IPaymentOptionSetting> GetOptionSettings(string channelName);
}