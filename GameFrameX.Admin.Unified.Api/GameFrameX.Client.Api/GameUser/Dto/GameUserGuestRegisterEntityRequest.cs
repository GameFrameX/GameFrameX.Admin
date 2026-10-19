using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// 游戏账号增加输入参数
/// </summary>
public class GameUserGuestRegisterEntityRequest : TenantGameUserEntityBaseRequest
{
    /// <summary>
    /// 账号
    /// </summary>
    [Required(ErrorMessage = "账号不能为空")]
    public override string UserName { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    public override string Password { get; set; } = null!;
}