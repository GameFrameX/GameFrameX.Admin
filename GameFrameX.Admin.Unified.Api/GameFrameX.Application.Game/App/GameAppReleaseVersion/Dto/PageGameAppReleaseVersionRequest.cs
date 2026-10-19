namespace GameFrameX.Application.Game.App.GameAppReleaseVersion.Dto;

/// <summary>
/// 游戏版本管理分页查询输入参数
/// </summary>
public class PageGameAppReleaseVersionRequest : BaseAppFilterPageRequest
{
    /// <summary>
    /// 应用作用域筛选；null 不筛，0 表示租户默认
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// OldVersion
    /// </summary>
    public string? OldVersion { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }
}