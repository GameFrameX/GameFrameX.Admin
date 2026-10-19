namespace GameFrameX.Application.Game.App.GameAppRemoteConfig.Dto;

/// <summary>
/// 远程配置管理基础输入参数
/// </summary>
public class GameAppRemoteConfigBaseRequest : GameAppRemoteConfigEditRequest
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