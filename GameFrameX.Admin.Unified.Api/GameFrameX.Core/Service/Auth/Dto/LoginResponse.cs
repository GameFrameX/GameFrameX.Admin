namespace GameFrameX.Core.Service.Auth.Dto;

/// <summary>
/// 用户登录结果
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// 令牌Token
    /// </summary>
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// 刷新Token
    /// </summary>
    public string RefreshToken { get; set; } = null!;
}