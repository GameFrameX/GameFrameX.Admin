using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 角色 + 用户解析结果。
/// </summary>
public sealed class RoleResolution
{
    public GameRoleEntity Role { get; set; } = null!;
    public long UserId { get; set; }
}