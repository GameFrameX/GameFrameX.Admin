namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 支付配置提供器接口
/// </summary>
public interface IPaymentOptionSettingProvider
{
    /// <summary>
    /// 获取指定支付渠道的配置
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置</returns>
    IPaymentOptionSetting GetOptionSetting(string channelName);

    /// <summary>
    /// 获取指定支付渠道的候选配置
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置列表</returns>
    IEnumerable<IPaymentOptionSetting> GetOptionSettings(string channelName);

    /// <summary>
    /// 获取指定类型的支付配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置</returns>
    T GetOptionSetting<T>(string channelName) where T : class, IPaymentOptionSetting;

    /// <summary>
    /// 获取指定类型的候选支付配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置列表</returns>
    IEnumerable<T> GetOptionSettings<T>(string channelName) where T : class, IPaymentOptionSetting;
}