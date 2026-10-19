// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Abstract.Interface;

/// <summary>
/// 游戏订单基础契约（支付渠道下单 / 验票通用字段，非物理表映射）
/// </summary>
public sealed class GameOrderBase : IGameOrderBase
{
    /// <summary>
    /// 订单ID。必须设置，表示唯一标识一个订单。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 角色ID。必须设置，表示下单用户的角色标识。
    /// </summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// 账号ID
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// 区服ID。必须设置，表示下单用户所在的区服标识。
    /// </summary>
    public string AreaId { get; set; } = string.Empty;

    /// <summary>
    /// 商品SKU
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// 订单ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 货币类型。必须设置，表示下单使用的货币类型，如CNY、USD等。
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// 客户端IP地址。必须设置，表示下单用户的客户端IP地址。
    /// </summary>
    public string ClientIp { get; set; } = string.Empty;

    /// <summary>
    /// 是否为沙盒用户
    /// </summary>
    public bool IsSandbox { get; set; }

    /// <summary>
    /// 真实价格
    /// </summary>
    public long Amount { get; set; }
}