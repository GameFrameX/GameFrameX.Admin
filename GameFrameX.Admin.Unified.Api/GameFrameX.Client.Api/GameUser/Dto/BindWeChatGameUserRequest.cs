using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// WeChat账号绑定输入参数
/// </summary>
public sealed class BindWeChatGameUserRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required(ErrorMessage = "用户ID不能为空")]
    public long UserId { get; set; }

    /// <summary>
    /// WeChat账号ID
    /// </summary>
    [Required(ErrorMessage = "WeChat账号ID不能为空")]
    public string WeChatId { get; set; } = null!;

    /// <summary>
    /// WeChat账号名称
    /// </summary>
    public string WeChatName { get; set; } = null!;
}