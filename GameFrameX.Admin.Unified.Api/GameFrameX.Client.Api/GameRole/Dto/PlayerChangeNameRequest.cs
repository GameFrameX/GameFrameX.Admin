namespace GameFrameX.Client.Api.GameRole.Dto;

public sealed class PlayerChangeNameRequest
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public long PlayerId { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; set; } = null!;
}