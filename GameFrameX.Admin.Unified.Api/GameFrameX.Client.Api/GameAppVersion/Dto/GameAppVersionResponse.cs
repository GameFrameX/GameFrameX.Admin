namespace GameFrameX.Client.Api.GameAppVersion.Dto;

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
    public string AppDownloadUrl { get; set; } = null!;

    /// <summary>
    /// 更新公告
    /// </summary>
    public string UpdateAnnouncement { get; set; } = null!;

    /// <summary>
    /// 更新标题
    /// </summary>
    public string UpdateTitle { get; set; } = null!;

    /// <summary>
    /// 是否需要更新
    /// </summary>
    public bool IsUpgrade { get; set; }
}