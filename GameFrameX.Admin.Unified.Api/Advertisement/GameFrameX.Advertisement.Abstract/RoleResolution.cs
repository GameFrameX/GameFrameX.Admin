using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 角色 + 用户解析结果。
/// </summary>
public sealed class RoleResolution
{
    public TenantGameRoleEntity Role { get; set; } = null!;
    public long UserId { get; set; }
}