namespace GameFrameX.Application.System.OpenAccess.Dto;

public class AddOpenAccessRequest : SystemOpenAccessEntity
{
    /// <summary>
    /// 身份标识
    /// </summary>
    [Required(ErrorMessage = "身份标识不能为空")]
    public override string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// 密钥
    /// </summary>
    [Required(ErrorMessage = "密钥不能为空")]
    public override string AccessSecret { get; set; } = string.Empty;

    /// <summary>
    /// 绑定用户Id
    /// </summary>
    [Required(ErrorMessage = "绑定用户不能为空")]
    public override long BindUserId { get; set; }
}