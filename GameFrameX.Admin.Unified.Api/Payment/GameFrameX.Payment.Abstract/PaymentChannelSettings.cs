namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 支付渠道设置
/// </summary>
public class PaymentChannelSettings
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 是否为沙盒环境
    /// </summary>
    public bool IsSandbox { get; set; } = false;

    /// <summary>
    /// 配置参数
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();

    /// <summary>
    /// 优先级 (数值越小优先级越高)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 支持的国家/地区代码列表
    /// </summary>
    public List<string> SupportedCountries { get; set; } = new();

    /// <summary>
    /// 支持的平台列表
    /// </summary>
    public List<string> SupportedPlatforms { get; set; } = new();
}