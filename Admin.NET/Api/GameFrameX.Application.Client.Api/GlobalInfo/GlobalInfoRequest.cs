namespace GameFrameX.Application.Api.GameApplicationVersion;

/// <summary>
/// 
/// </summary>
public class GlobalInfoRequest
{
    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// 模块名称
    /// </summary>
    public string PackageName { get; set; }

    /// <summary>
    /// 平台
    /// </summary>
    public string Platform { get; set; }

    /// <summary>
    /// 程序版本
    /// </summary>
    public string AppVersion { get; set; }

    /// <summary>
    /// 主渠道名称
    /// </summary>
    public string Channel { get; set; }

    /// <summary>
    /// 子渠道名称
    /// </summary>
    public string SubChannel { get; set; }
}