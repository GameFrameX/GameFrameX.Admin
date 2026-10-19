namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 游戏数据域角色错误码（段 602，见 <see cref="ErrorSegmentRegistry"/>）。
/// 旧段位 6021xxxx（Client.Api/Hub.Api ResultCode）按 ADR 根治重排为 602 段。
/// </summary>
public enum GameRoleErrorCode
{
    /// <summary>
    /// 角色不存在
    /// </summary>
    [ErrorText("角色不存在", "Role not found")] RoleNotFound = 60200001,

    /// <summary>
    /// 角色名称不能为空
    /// </summary>
    [ErrorText("角色名称不能为空", "Role name cannot be empty")]
    RoleNameNotEmpty = 60200002,

    /// <summary>
    /// 角色已被封禁/限制登录
    /// </summary>
    [ErrorText("角色已被封禁/限制登录", "Role is banned or restricted from logging in")]
    RoleBanned = 60200003,
}