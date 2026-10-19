namespace GameFrameX.Client.Api.GameRole.Dto;

/// <summary>
/// 请求获取玩家信息
/// </summary>
public sealed class PlayerGetInfoRequest
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.Range(1, long.MaxValue)]
    public long PlayerId { get; set; }
}