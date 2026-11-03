namespace GameFrameX.Application.Game.Order.GamePaymentConfig.Dto;

/// <summary>
/// 游戏支付配置新增输入参数
/// </summary>
public class AddGamePaymentConfigRequest : BaseAddRequest
{
    /// <summary>
    /// 分包ID。空表示默认配置。
    /// </summary>
    public long? PackageId { get; set; }

    /// <summary>
    /// 支付渠道
    /// </summary>
    [Required(ErrorMessage = "支付渠道不能为空")]
    public string PayChannel { get; set; } = null!;

    /// <summary>
    /// 是否沙盒配置
    /// </summary>
    public bool IsSandbox { get; set; }

    /// <summary>
    /// 支付配置JSON
    /// </summary>
    [Required(ErrorMessage = "支付配置不能为空")]
    public string ConfigJson { get; set; } = null!;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;
}