namespace GameFrameX.Core.Service.Auth.Dto;

/// <summary>
/// 登录配置结果
/// </summary>
public class LoginConfigResponse
{
    /// <summary>
    /// 是否启用二次验证
    /// </summary>
    public bool SecondVerEnabled { get; set; }

    /// <summary>
    /// 是否启用图形验证码
    /// </summary>
    public bool CaptchaEnabled { get; set; }
    /// <summary>
    /// 是否启用邮箱自助注册
    /// </summary>
    public bool EmailRegisterEnabled { get; set; }

    /// <summary>
    /// 平台超管密码是否已完成初始化（false = 登录页需展示首装密码初始化向导）
    /// </summary>
    public bool SuperAdminPasswordConfigured { get; set; }
}