namespace GameFrameX.Client.Api.GameRole.Dto;

/// <summary>
/// 角色信息
/// </summary>
public sealed class PlayerInfoResponse
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 角色名
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 角色等级
    /// </summary>
    public uint Level { get; set; }

    /// <summary>
    /// 角色状态
    /// </summary>
    public int State { get; set; }

    /// <summary>
    /// 角色头像
    /// </summary>
    public uint Avatar { get; set; }

    /// <summary>
    /// 角色当前经验
    /// </summary>
    public ulong CurrentExp { get; set; }
}