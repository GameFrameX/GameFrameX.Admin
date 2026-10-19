namespace GameFrameX.Application.Game.App.GameAppReleaseVersion.Dto;

/// <summary>
/// 游戏版本管理新增/更新共有输入参数
/// </summary>
public class GameAppReleaseVersionEditRequest : BaseAddRequest
{
    /// <summary>
    /// 应用作用域，0 / null 表示租户默认配置
    /// </summary>
    public virtual long? AppId { get; set; }

    /// <summary>
    /// Channel
    /// </summary>
    public virtual string? Channel { get; set; }

    /// <summary>
    /// 公告标题
    /// </summary>
    public virtual string? Title { get; set; }

    /// <summary>
    /// Announcement
    /// </summary>
    public virtual string? Announcement { get; set; }

    /// <summary>
    /// OldVersion
    /// </summary>
    public virtual string? OldVersion { get; set; }

    /// <summary>
    /// DownloadUrl
    /// </summary>
    public virtual string? DownloadUrl { get; set; }

    /// <summary>
    /// IsForce
    /// </summary>
    public virtual bool? IsForce { get; set; }

    /// <summary>
    /// Platform
    /// </summary>
    public virtual string? Platform { get; set; }

    /// <summary>
    /// AppVersion
    /// </summary>
    public virtual string? AppVersion { get; set; }

    /// <summary>
    /// Package
    /// </summary>
    public virtual string? Package { get; set; }

    /// <summary>
    /// Language
    /// </summary>
    public virtual string? Language { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public virtual string? Description { get; set; }
}