// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Client.Api.Integrations.Payment.Dto;

/// <summary>
/// 商品订单增加输出参数
/// </summary>
public class AddTenantGameOrderEntityResponse : EntityBaseId
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 是否需要支付，true-需要支付，只有需要支付的时候才有
    /// </summary>
    public bool IsNeedPay { get; set; }

    /// <summary>
    /// 金额,只有需要支付的时候才有
    /// </summary>
    public uint Amount { get; set; }

    /// <summary>
    /// 第三方拉起支付的Token值,只有需要支付的时候才有
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// 商品ID
    /// </summary>
    public long Sku { get; set; }

    /// <summary>
    /// 第三方订单ID,只有需要支付的时候才有
    /// </summary>
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 商品价格Id
    /// </summary>
    public long PriceId { get; set; }
}