namespace GameFrameX.Client.Api.GameRole.Dto;

/// <summary>
/// 请求创建角色返回
/// </summary>
public sealed class PlayerCreateResponse
{
    /// <summary>
    /// 角色信息
    /// </summary>
    public PlayerInfoResponse PlayerInfo { get; set; } = null!;
}