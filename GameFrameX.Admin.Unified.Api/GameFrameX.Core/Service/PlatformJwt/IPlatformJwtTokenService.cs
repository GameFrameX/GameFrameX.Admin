namespace GameFrameX.Core.Service.PlatformJwt;

/// <summary>
/// 平台 JWT 令牌签发与刷新服务接口（C217 可测性重构：SystemAuthService 注入此接口，便于测试 fake）。
/// </summary>
public interface IPlatformJwtTokenService
{
    /// <summary>
    /// 使用当前平台 JWT 配置签发 AccessToken，并写入当前全局令牌版本。
    /// </summary>
    Task<string> CreateAccessTokenAsync(IDictionary<string, object> payload, long expiredMinutes);

    /// <summary>
    /// 使用当前平台 JWT 配置生成 RefreshToken。
    /// </summary>
    Task<string> GenerateRefreshTokenAsync(string accessToken, int expiredMinutes);
}