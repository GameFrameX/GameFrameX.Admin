namespace GameFrameX.Application.Api.GameApplicationVersion;

/// <summary>
/// 
/// </summary>
public class GlobalInfoResponse
{
    /// <summary>
    /// 检测程序版本地址
    /// </summary>
    public string CheckAppVersionUrl { get; set; }

    /// <summary>
    /// 检测资源版本地址
    /// </summary>
    public string CheckResourceVersionUrl { get; set; }

    /// <summary>
    /// 附加内容。JSON对象
    /// </summary>
    public string Content { get; set; }
}