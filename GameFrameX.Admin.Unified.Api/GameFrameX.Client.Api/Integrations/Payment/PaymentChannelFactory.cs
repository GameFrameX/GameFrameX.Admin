using GameFrameX.Payment.Abstract;

namespace GameFrameX.Client.Api.Integrations.Payment;

/// <summary>
/// 支付渠道工厂实现
/// </summary>
public class PaymentChannelFactory : IPaymentChannelFactory
{
    private readonly Dictionary<string, IPaymentChannelHandler> _handlers;
    private readonly IPaymentOptionSettingProvider _optionSettingProvider;

    public PaymentChannelFactory(IEnumerable<IPaymentChannelHandler> handlers, IPaymentOptionSettingProvider optionSettingProvider)
    {
        _handlers = handlers.ToDictionary(h => h.ChannelName, h => h, StringComparer.OrdinalIgnoreCase);
        _optionSettingProvider = optionSettingProvider;
    }

    /// <summary>
    /// 获取支付渠道处理器
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付渠道处理器</returns>
    public IPaymentChannelHandler GetHandler(string channelName)
    {
        _handlers.TryGetValue(channelName, out var handler);
#pragma warning disable CS8603 // 接口 IPaymentChannelFactory.GetHandler 签名为非空（跨项目），调用方约定 not found 返回 null
        return handler;
#pragma warning restore CS8603
    }

    /// <summary>
    /// 检查是否支持指定的支付渠道
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>是否支持</returns>
    public bool IsSupported(string channelName)
    {
        return _handlers.ContainsKey(channelName);
    }

    /// <summary>
    /// 获取所有支持的支付渠道名称
    /// </summary>
    /// <returns>支付渠道名称列表</returns>
    public IEnumerable<string> GetSupportedChannels()
    {
        return _handlers.Keys;
    }

    /// <summary>
    /// 获取支付渠道配置
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置</returns>
    public IPaymentOptionSetting GetOptionSetting(string channelName)
    {
        return _optionSettingProvider.GetOptionSetting(channelName);
    }

    /// <summary>
    /// 获取支付渠道候选配置
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置列表</returns>
    public IEnumerable<IPaymentOptionSetting> GetOptionSettings(string channelName)
    {
        return _optionSettingProvider.GetOptionSettings(channelName);
    }
}