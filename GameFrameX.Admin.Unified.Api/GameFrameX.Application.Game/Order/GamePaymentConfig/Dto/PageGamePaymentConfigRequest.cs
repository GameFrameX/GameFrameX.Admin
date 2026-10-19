namespace GameFrameX.Application.Game.Order.GamePaymentConfig.Dto;

/// <summary>
/// 游戏支付配置分页查询输入参数
/// </summary>
public class PageGamePaymentConfigRequest : BaseAppActivePageRequest
{
    /// <summary>
    /// 游戏应用ID。支付配置为应用工作台页面，分页查询必须提供具体应用，值必须大于 0。
    /// </summary>
    public long? GameAppId { get; set; }

    /// <summary>
    /// 分包ID
    /// </summary>
    public long? PackageId { get; set; }

    /// <summary>
    /// 支付渠道
    /// </summary>
    public string PayChannel { get; set; } = null!;

    /// <summary>
    /// 是否沙盒配置
    /// </summary>
    public bool? IsSandbox { get; set; }
}