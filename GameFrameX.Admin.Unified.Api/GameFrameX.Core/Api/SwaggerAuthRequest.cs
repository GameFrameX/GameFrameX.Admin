namespace GameFrameX.Core.Api;

/// <summary>
/// Swagger 登录表单输入。
/// </summary>
public sealed class SwaggerAuthRequest
{
    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}