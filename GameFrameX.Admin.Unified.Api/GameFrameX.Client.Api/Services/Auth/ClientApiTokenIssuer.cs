using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GameFrameX.Client.Api.Services.Auth;

/// <summary>
/// Client API 账号访问令牌签发服务。
/// <para>签名密钥与有效期按当前请求租户从 <see cref="IClientApiTenantOptionsProvider"/> 读取；密钥按 TenantId 隔离，未配置时抛出异常。</para>
/// </summary>
public sealed class ClientApiTokenIssuer
{
    public const string UserIdClaim = "userId";
    public const string TokenUseClaim = "tokenUse";
    public const string ClientApiAccessTokenUse = "client_api_access";
    public const string ClientApiTokenAudience = "client-api";

    private readonly IClientApiTenantOptionsProvider _optionsProvider;
    private readonly JsonWebTokenHandler _handler = new();

    /// <summary>
    /// </summary>
    public ClientApiTokenIssuer(IClientApiTenantOptionsProvider optionsProvider)
    {
        _optionsProvider = optionsProvider;
    }

    /// <summary>
    /// 为登录后的游戏账号签发 Client API 访问令牌。
    /// </summary>
    public async Task<ClientApiTokenResponse> IssueAsync(TenantGameUserEntity user, string provider)
    {
        var policy = await _optionsProvider.GetTokenPolicyAsync();
        var signingKey = policy.SigningKey
                         ?? throw new InvalidOperationException("当前租户未配置 Client API 访问令牌签名密钥，禁止回落到进程级或平台全局密钥。");

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(Math.Max(1, policy.LifetimeMinutes));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = ClientApiPlatformDefaults.GameLoginTicketIssuer,
            Audience = ClientApiTokenAudience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(UserIdClaim, user.Id.ToString()),
                new Claim(TokenUseClaim, ClientApiAccessTokenUse),
                new Claim("status", user.Status.ToString()),
                new Claim("provider", provider ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            }),
            SigningCredentials = CreateSigningCredentials(signingKey, "client-api-hmac-v1"),
        };

        return new ClientApiTokenResponse(_handler.CreateToken(descriptor), TimerHelper.DateTimeToSeconds(expiresAt.UtcDateTime, true));
    }

    /// <summary>
    /// 校验 Client API 访问令牌并返回账号 ID。
    /// </summary>
    public async Task<ClientApiTokenValidationResponse> ValidateAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return ClientApiTokenValidationResponse.Fail;
        }

        var policy = await _optionsProvider.GetTokenPolicyAsync();
        if (policy.SigningKey == null)
        {
            // 当前租户未配置签名密钥，无法校验任何令牌
            return ClientApiTokenValidationResponse.Fail;
        }

        var result = await _handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = ClientApiPlatformDefaults.GameLoginTicketIssuer,
            ValidAudience = ClientApiTokenAudience,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = CreateSecurityKey(policy.SigningKey),
            ClockSkew = TimeSpan.FromSeconds(10),
        });

        if (!result.IsValid)
        {
            return ClientApiTokenValidationResponse.Fail;
        }

        var claims = result.ClaimsIdentity.Claims.ToArray();
        var tokenUse = claims.FirstOrDefault(c => c.Type == TokenUseClaim)?.Value;
        var userIdText = claims.FirstOrDefault(c => c.Type == UserIdClaim)?.Value ??
                         claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        return tokenUse == ClientApiAccessTokenUse && long.TryParse(userIdText, out var userId)
                   ? new ClientApiTokenValidationResponse(true, userId)
                   : ClientApiTokenValidationResponse.Fail;
    }

    internal SigningCredentials CreateSigningCredentials(string configuredKey, string keyId)
    {
        var key = CreateSecurityKey(configuredKey);
        key.KeyId = keyId;
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    private static SymmetricSecurityKey CreateSecurityKey(string configuredKey)
    {
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            throw new InvalidOperationException("Client API token signing key is not configured.");
        }

        return new SymmetricSecurityKey(SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey)));
    }
}