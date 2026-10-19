namespace GameFrameX.Core.Service.Auth.Dto;

/// <summary>
/// 图形验证码结果
/// </summary>
public class CaptchaResponse
{
    /// <summary>
    /// 验证码唯一标识（登录提交时与验证码输入值配对校验）
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// 验证码图片 Base64 内容
    /// </summary>
    public string Img { get; set; } = null!;
}