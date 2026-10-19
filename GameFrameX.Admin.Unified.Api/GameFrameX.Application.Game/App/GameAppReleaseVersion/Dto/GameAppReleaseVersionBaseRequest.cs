namespace GameFrameX.Application.Game.App.GameAppReleaseVersion.Dto;

/// <summary>
/// 游戏版本管理基础输入参数
/// </summary>
public class GameAppReleaseVersionBaseRequest : GameAppReleaseVersionEditRequest
{
    /// <summary>
    /// CreatedName
    /// </summary>
    public virtual string? CreatedName { get; set; }

    /// <summary>
    /// UpdatedName
    /// </summary>
    public virtual string? UpdatedName { get; set; }
}