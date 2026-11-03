namespace GameFrameX.Application.Game.App.GameAppReleaseVersion.Dto;

/// <summary>
/// 游戏版本管理分页查询输入参数
/// </summary>
public class PageGameAppReleaseVersionRequest : BaseAppFilterPageRequest
{
    /// <summary>
    /// OldVersion
    /// </summary>
    public string? OldVersion { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }
}