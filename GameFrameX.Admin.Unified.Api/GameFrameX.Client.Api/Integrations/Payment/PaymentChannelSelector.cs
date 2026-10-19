using GameFrameX.Payment.Abstract;

namespace GameFrameX.Client.Api.Integrations.Payment;

// 接口 IPaymentChannelSelector 定义在 GameFrameX.Payment.Abstract（跨项目，Nullable 未开启），其参数声明为 `string x = null` 非空带 null 默认值；本实现须与接口签名保持一致，CS8625 不可避免。
#pragma warning disable CS8625 // 接口参数 = null 默认值与跨项目签名对齐，Pay.Abstract 未启用 NRT
#pragma warning disable CS8603 // SelectBestChannel 未匹配时 FirstOrDefault 返回 null，与接口语义一致

/// <summary>
/// 支付渠道选择器实现
/// </summary>
public class PaymentChannelSelector : IPaymentChannelSelector
{
    private readonly IPaymentChannelFactory _factory;

    public PaymentChannelSelector(IPaymentChannelFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// 根据条件选择最佳支付渠道
    /// </summary>
    /// <param name="country">国家/地区代码</param>
    /// <param name="platform">平台</param>
    /// <param name="preferredChannel">首选渠道</param>
    /// <returns>推荐的支付渠道名称</returns>
    public string SelectBestChannel(string country = null, string platform = null, string preferredChannel = null)
    {
        if (!string.IsNullOrEmpty(preferredChannel) && IsChannelAvailable(preferredChannel, country, platform))
        {
            return preferredChannel;
        }

        return GetAvailableChannels(country, platform).FirstOrDefault();
    }

    /// <summary>
    /// 获取可用的支付渠道列表
    /// </summary>
    /// <param name="country">国家/地区代码</param>
    /// <param name="platform">平台</param>
    /// <returns>可用的支付渠道列表</returns>
    public IEnumerable<string> GetAvailableChannels(string country = null, string platform = null)
    {
        return _factory.GetSupportedChannels().Where(channelName => IsChannelAvailable(channelName, country, platform));
    }

    /// <summary>
    /// 检查指定渠道是否可用
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <param name="country">国家/地区代码</param>
    /// <param name="platform">平台</param>
    /// <returns>是否可用</returns>
    public bool IsChannelAvailable(string channelName, string country = null, string platform = null)
    {
        return _factory.IsSupported(channelName) && _factory.GetOptionSetting(channelName) != null;
    }
}