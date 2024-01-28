namespace GameFrameX.Application.Api.GameApplicationVersion;

/// <summary>
/// 
/// </summary>
public class GameAppVersionRequest
{
    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; }

    public string PackageName { get; set; }
    public string AppVersion { get; set; }
    public string Channel { get; set; }
    public string SubChannel { get; set; }

    /// <summary>
    /// 平台
    /// </summary>
    public string Platform { get; set; }
}