namespace GameFrameX.Client.Api.Services.Auth;

/// <summary>
/// Client API 访问令牌签发结果。
/// </summary>
public sealed record ClientApiTokenResponse(string Token, long ExpireTime);