namespace GameFrameX.Application.Api.GameApplicationVersion;

/// <summary>
/// 
/// </summary>
public class GameAppVersionResponse
{
    /// <summary>
    /// 是否强制升级
    /// </summary>
    public bool IsForce { get; set; }

    /// <summary>
    /// 程序下载地址
    /// </summary>
    public string AppDownloadUrl { get; set; }

    /// <summary>
    /// 更新公告
    /// </summary>
    public string UpdateAnnouncement { get; set; }

    /// <summary>
    /// 是否需要更新
    /// </summary>
    public bool IsUpgrade { get; set; }
}