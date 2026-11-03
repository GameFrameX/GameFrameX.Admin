namespace GameFrameX.Application.Game.Order.GamePaymentConfig.Dto;

/// <summary>
/// 游戏支付配置分页查询输入参数
/// </summary>
public class PageGamePaymentConfigRequest : BaseAppActivePageRequest
{
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