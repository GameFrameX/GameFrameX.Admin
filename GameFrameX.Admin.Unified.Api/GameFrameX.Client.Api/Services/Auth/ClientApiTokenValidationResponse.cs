namespace GameFrameX.Client.Api.Services.Auth;

/// <summary>
/// Client API 访问令牌校验结果。
/// </summary>
public sealed record ClientApiTokenValidationResponse(bool IsValid, long UserId)
{
    public static readonly ClientApiTokenValidationResponse Fail = new(false, 0);
}