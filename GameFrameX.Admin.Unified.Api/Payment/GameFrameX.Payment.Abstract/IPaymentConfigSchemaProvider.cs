namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 支付配置 schema 提供器接口
/// </summary>
public interface IPaymentConfigSchemaProvider
{
    /// <summary>
    /// 获取指定渠道的字段 schema
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>字段 schema，渠道不存在返回 null</returns>
    PaymentConfigSchema GetSchema(string channelName);

    /// <summary>
    /// 获取所有支持渠道的清单
    /// </summary>
    /// <returns>渠道清单</returns>
    List<PaymentConfigChannel> GetChannels();
}