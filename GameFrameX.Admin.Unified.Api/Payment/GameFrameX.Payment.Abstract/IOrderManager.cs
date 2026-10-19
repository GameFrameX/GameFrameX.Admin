// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Abstract;

public interface IOrderManager
{
    /// <summary>
    /// 异步获取指定订单的详细信息
    /// </summary>
    /// <param name="httpClient">HTTP客户端实例</param>
    /// <param name="orderParamsGet">订单查询参数</param>
    /// <param name="optionSetting">支付选项设置</param>
    /// <returns>返回订单详细信息的JSON字符串</returns>
    /// <exception cref="ArgumentException">当orderParamsGet为空时抛出</exception>
    /// <exception cref="ArgumentNullException">当httpClient参数为null时抛出此异常</exception>
    Task<string> GetAsync(HttpClient httpClient, IOrderParamsGet orderParamsGet, IPaymentOptionSetting optionSetting);

    /// <summary>
    /// 异步获取指定时间范围内的订单列表
    /// </summary>
    /// <param name="httpClient">HTTP客户端实例</param>
    /// <param name="orderParamsGetList">订单查询参数集合</param>
    /// <param name="optionSetting">支付选项设置</param>
    /// <returns>返回订单列表的JSON字符串</returns>
    /// <exception cref="ArgumentException">当orderParamsGetList为空时抛出</exception>
    /// <exception cref="ArgumentNullException">当httpClient参数为null时抛出此异常</exception>
    Task<string> GetListAsync(HttpClient httpClient, IOrderParamsGetList orderParamsGetList, IPaymentOptionSetting optionSetting);
}