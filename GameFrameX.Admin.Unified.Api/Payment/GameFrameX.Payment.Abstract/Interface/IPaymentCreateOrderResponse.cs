// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Abstract.Interface;

/// <summary>
/// 支付下单结果接口，定义下单后返回的关键信息。
/// </summary>
public interface IPaymentCreateOrderResponse
{
    /// <summary>
    /// 是否需要支付（如为 false，表示无需支付直接完成）。
    /// </summary>
    bool IsNeedPay { get; set; }

    /// <summary>
    /// 商品 SKU（库存单位标识）。
    /// </summary>
    string Sku { get; set; }

    /// <summary>
    /// 角色 ID（下单用户的角色标识）。
    /// </summary>
    string RoleId { get; set; }

    /// <summary>
    /// 支付令牌（用于后续支付验证或处理的令牌）。
    /// </summary>
    string Token { get; set; }

    /// <summary>
    /// 账号 ID（下单用户的账号标识）。
    /// </summary>
    string AccountId { get; set; }

    /// <summary>
    /// 订单 ID（下单后生成的唯一订单标识）。可能不存在于某些支付方式中。
    /// </summary>
    string OrderId { get; set; }
}