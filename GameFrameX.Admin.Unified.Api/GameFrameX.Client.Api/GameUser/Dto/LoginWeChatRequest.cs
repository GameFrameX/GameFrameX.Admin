using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// WeChat 账号登录输入参数
/// </summary>
public class LoginWeChatRequest
{
    /// <summary>
    /// WeChat 账号ID
    /// </summary>
    [Required(ErrorMessage = "WeChat账号ID不能为空")]
    public string WeChatId { get; set; } = null!;

    /// <summary>
    /// WeChat 账号名称
    /// </summary>
    public string WeChatName { get; set; } = null!;
}