namespace GameFrameX.Application.Game.App.GameAppReleaseVersion.Dto;

/// <summary>
/// 游戏版本管理增加输入参数
/// </summary>
public class AddGameAppReleaseVersionRequest : GameAppReleaseVersionBaseRequest
{
    /// <summary>
    /// Channel
    /// </summary>
    [Required(ErrorMessage = "Channel不能为空")]
    public override string? Channel { get; set; }

    /// <summary>
    /// 公告标题
    /// </summary>
    [Required(ErrorMessage = "Title不能为空")]
    public override string? Title { get; set; }

    /// <summary>
    /// Announcement
    /// </summary>
    [Required(ErrorMessage = "Announcement不能为空")]
    public override string? Announcement { get; set; }

    /// <summary>
    /// OldVersion
    /// </summary>
    [Required(ErrorMessage = "OldVersion不能为空")]
    public override string? OldVersion { get; set; }

    /// <summary>
    /// DownloadUrl
    /// </summary>
    [Required(ErrorMessage = "DownloadUrl不能为空")]
    public override string? DownloadUrl { get; set; }

    /// <summary>
    /// IsForce
    /// </summary>
    [Required(ErrorMessage = "IsForce不能为空")]
    public override bool? IsForce { get; set; }

    /// <summary>
    /// Platform
    /// </summary>
    [Required(ErrorMessage = "Platform不能为空")]
    public override string? Platform { get; set; }

    /// <summary>
    /// AppVersion
    /// </summary>
    [Required(ErrorMessage = "Version不能为空")]
    public override string? AppVersion { get; set; }

    /// <summary>
    /// Package
    /// </summary>
    [Required(ErrorMessage = "Package不能为空")]
    public override string? Package { get; set; }

    /// <summary>
    /// Language
    /// </summary>
    [Required(ErrorMessage = "Language不能为空")]
    public override string? Language { get; set; }
}