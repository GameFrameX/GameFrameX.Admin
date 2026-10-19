using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.User.GameUser.Dto;

/// <summary>
/// 根据邮箱添加账号
/// </summary>
public class AddTenantGameUserEntityByMailRequest : TenantGameUserEntityBaseRequest
{
    /// <summary>
    /// 账号
    /// </summary>
    [Required(ErrorMessage = "账号不能为空")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    public string EmailPassword { get; set; } = null!;

    /// <summary>
    /// 昵称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 状态
    /// </summary>
    [Required(ErrorMessage = "状态不能为空")]
    public override GameUserStatus Status { get; set; }
}