namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 支付渠道选择器接口
/// </summary>
public interface IPaymentChannelSelector
{
#pragma warning disable CS8625 // 可选渠道过滤参数，null 表示不限定
    /// <summary>
    /// 根据条件选择最佳支付渠道
    /// </summary>
    /// <param name="country">国家/地区代码</param>
    /// <param name="platform">平台</param>
    /// <param name="preferredChannel">首选渠道</param>
    /// <returns>推荐的支付渠道名称</returns>
    string SelectBestChannel(string country = null, string platform = null, string preferredChannel = null);

    /// <summary>
    /// 获取可用的支付渠道列表
    /// </summary>
    /// <param name="country">国家/地区代码</param>
    /// <param name="platform">平台</param>
    /// <returns>可用的支付渠道列表</returns>
    IEnumerable<string> GetAvailableChannels(string country = null, string platform = null);

    /// <summary>
    /// 检查指定渠道是否可用
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <param name="country">国家/地区代码</param>
    /// <param name="platform">平台</param>
    /// <returns>是否可用</returns>
    bool IsChannelAvailable(string channelName, string country = null, string platform = null);
#pragma warning restore CS8625
}