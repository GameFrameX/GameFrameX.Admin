namespace GameFrameX.Application.Game.Order.GamePaymentConfig.Dto;

/// <summary>
/// 游戏支付配置更新输入参数
/// </summary>
public class UpdateGamePaymentConfigRequest : BaseUpdateRequest
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
    /// 支付配置JSON。传 ****** 时保留原配置。
    /// </summary>
    [Required(ErrorMessage = "支付配置不能为空")]
    public string ConfigJson { get; set; } = null!;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;
}