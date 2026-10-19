using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace GameFrameX.Core.Service.PlatformJwt;

/// <summary>
/// 平台 JWT 令牌签发与刷新服务。
/// </summary>
public sealed class PlatformJwtTokenService : IPlatformJwtTokenService
{
    private static readonly string[] DateTypeClaimTypes = { JwtRegisteredClaimNames.Iat, JwtRegisteredClaimNames.Nbf, JwtRegisteredClaimNames.Exp, };
    private static readonly string[] RefreshTokenClaims = { "f", "e", "s", "l", "k", PlatformJwtConfigConstants.TokenVersionClaim, };
    private readonly PlatformJwtRuntimeProvider _runtimeProvider;

    public PlatformJwtTokenService(PlatformJwtRuntimeProvider runtimeProvider)
    {
        _runtimeProvider = runtimeProvider;
    }

    /// <summary>
    /// 使用当前平台 JWT 配置签发 AccessToken，并写入当前全局令牌版本。
    /// </summary>
    public async Task<string> CreateAccessTokenAsync(IDictionary<string, object> payload, long expiredMinutes)
    {
        var settings = await _runtimeProvider.GetRequiredSettingsAsync();
        var now = TimerHelper.UnixTimeSeconds();
        payload[JwtRegisteredClaimNames.Iat] = now;
        payload[JwtRegisteredClaimNames.Nbf] = now;
        payload[JwtRegisteredClaimNames.Exp] = now + expiredMinutes * 60;
        payload[JwtRegisteredClaimNames.Iss] = settings.ValidIssuer;
        payload[JwtRegisteredClaimNames.Aud] = settings.ValidAudience;
        payload[PlatformJwtConfigConstants.TokenVersionClaim] = settings.TokenVersion;

        return CreateToken(settings, payload);
    }

    /// <summary>
    /// 使用当前平台 JWT 配置生成 RefreshToken；RefreshToken 同样携带全局令牌版本，旧版本不能换取新令牌。
    /// </summary>
    public async Task<string> GenerateRefreshTokenAsync(string accessToken, int expiredMinutes)
    {
        var settings = await _runtimeProvider.GetRequiredSettingsAsync();
        return GenerateRefreshToken(settings, accessToken, expiredMinutes);
    }

    /// <summary>
    /// 自动刷新过期 AccessToken。旧版本 AccessToken 或 RefreshToken 不会被续签。
    /// </summary>
    public async Task<bool> AutoRefreshTokenAsync(AuthorizationHandlerContext context, DefaultHttpContext httpContext, long expiredMinutes, int refreshTokenExpiredMinutes)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            if (RefreshTokenClaims.All(k => context.User.Claims.Any(c => c.Type == k)))
            {
                return false;
            }

            return await ValidateTokenVersionAsync(context.User);
        }

        if (httpContext.GetEndpoint()?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            return true;
        }

        var accessToken = GetJwtBearerToken(httpContext, "Authorization");
        var refreshToken = GetJwtBearerToken(httpContext, "X-Authorization");
        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        var newAccessToken = await ExchangeAsync(accessToken, refreshToken, expiredMinutes);
        if (string.IsNullOrWhiteSpace(newAccessToken))
        {
            return false;
        }

        var token = new JsonWebTokenHandler().ReadJsonWebToken(newAccessToken);
        var identity = new ClaimsIdentity("AuthenticationTypes.Federation");
        identity.AddClaims(token.Claims);
        httpContext.User = new ClaimsPrincipal(identity);

        var newRefreshToken = await GenerateRefreshTokenAsync(newAccessToken, refreshTokenExpiredMinutes);
        httpContext.SetTokensOfResponseHeaders(newAccessToken, newRefreshToken);
        var exposeHeaderKey = "Access-Control-Expose-Headers";
        httpContext.Response.Headers.TryGetValue(exposeHeaderKey, out var exposedHeaders);
        httpContext.Response.Headers[exposeHeaderKey] = string.Join(',', StringValues.Concat(exposedHeaders, new StringValues(new[] { "access-token", "x-access-token", })).Distinct());
        return true;
    }

    /// <summary>
    /// 校验当前 Principal 中的令牌版本是否等于持久化当前版本。
    /// </summary>
    public async Task<bool> ValidateTokenVersionAsync(ClaimsPrincipal principal)
    {
        var claimValue = principal.FindFirst(PlatformJwtConfigConstants.TokenVersionClaim)?.Value;
        if (!long.TryParse(claimValue, out var tokenVersion))
        {
            return false;
        }

        var settings = await _runtimeProvider.GetRequiredSettingsAsync();
        return tokenVersion == settings.TokenVersion;
    }

    private async Task<string?> ExchangeAsync(string expiredToken, string refreshToken, long expiredMinutes)
    {
        var settings = await _runtimeProvider.GetRequiredSettingsAsync();
        var validationParameters = PlatformJwtRuntimeProvider.CreateValidationParameters(settings);
        var handler = new JsonWebTokenHandler();

        var expiredTokenResult = await handler.ValidateTokenAsync(expiredToken, validationParameters);
        if (expiredTokenResult.IsValid)
        {
            return null;
        }

        var refreshTokenResult = await handler.ValidateTokenAsync(refreshToken, validationParameters);
        if (!refreshTokenResult.IsValid || refreshTokenResult.SecurityToken is not JsonWebToken refreshJsonWebToken)
        {
            return null;
        }

        if (!TryGetTokenVersion(refreshJsonWebToken.Claims, out var refreshTokenVersion) || refreshTokenVersion != settings.TokenVersion)
        {
            return null;
        }

        var accessParts = expiredToken.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (accessParts.Length != 3)
        {
            return null;
        }

        if (refreshJsonWebToken.GetPayloadValue<string>("f") != accessParts[0] ||
            refreshJsonWebToken.GetPayloadValue<string>("e") != accessParts[2])
        {
            return null;
        }

        var start = refreshJsonWebToken.GetPayloadValue<int>("s");
        var length = refreshJsonWebToken.GetPayloadValue<int>("l");
        if (start < 0 || length <= 0 || accessParts[1].Length < start + length)
        {
            return null;
        }

        if (accessParts[1].Substring(start, length) != refreshJsonWebToken.GetPayloadValue<string>("k"))
        {
            return null;
        }

        var securityToken = new JwtSecurityTokenHandler().ReadJwtToken(expiredToken);
        var payload = securityToken.Payload.ToDictionary(u => u.Key, u => u.Value);
        foreach (var key in DateTypeClaimTypes)
        {
            payload.Remove(key);
        }

        if (!TryGetTokenVersion(securityToken.Claims, out var accessTokenVersion) || accessTokenVersion != settings.TokenVersion)
        {
            return null;
        }

        return await CreateAccessTokenAsync(payload, expiredMinutes);
    }

    private static string GenerateRefreshToken(PlatformJwtSettings settings, string accessToken, int expiredMinutes)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw BusinessError.Oh("AccessToken不能为空");
        }

        var parts = accessToken.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 || parts.Any(string.IsNullOrWhiteSpace) || parts[1].Length < 27)
        {
            throw BusinessError.Oh("AccessToken格式无效");
        }

        var start = RandomNumberGenerator.GetInt32(10, parts[1].Length / 2 + 2);
        var length = RandomNumberGenerator.GetInt32(3, 13);
        var now = TimerHelper.UnixTimeSeconds();
        return CreateToken(settings, new Dictionary<string, object>
        {
            { "f", parts[0] },
            { "e", parts[2] },
            { "s", start },
            { "l", length },
            { "k", parts[1].Substring(start, length) },
            { PlatformJwtConfigConstants.TokenVersionClaim, settings.TokenVersion },
            { JwtRegisteredClaimNames.Iat, now },
            { JwtRegisteredClaimNames.Nbf, now },
            { JwtRegisteredClaimNames.Exp, now + expiredMinutes * 60 },
            { JwtRegisteredClaimNames.Iss, settings.ValidIssuer },
            { JwtRegisteredClaimNames.Aud, settings.ValidAudience },
        });
    }

    private static string CreateToken(PlatformJwtSettings settings, IDictionary<string, object> payload)
    {
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.IssuerSigningKey)), PlatformJwtConfigConstants.Algorithm);
        return new JsonWebTokenHandler().CreateToken(JsonSerializer.Serialize(payload), credentials);
    }

    private static string? GetJwtBearerToken(DefaultHttpContext httpContext, string headerKey)
    {
        var value = httpContext.Request.Headers[headerKey].ToString();
        const string prefix = "Bearer ";
        return string.IsNullOrWhiteSpace(value) || !value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                   ? null
                   : value[prefix.Length..].Trim();
    }

    private static bool TryGetTokenVersion(IEnumerable<Claim> claims, out long tokenVersion)
    {
        var claimValue = claims.FirstOrDefault(u => u.Type == PlatformJwtConfigConstants.TokenVersionClaim)?.Value;
        return long.TryParse(claimValue, out tokenVersion);
    }
}