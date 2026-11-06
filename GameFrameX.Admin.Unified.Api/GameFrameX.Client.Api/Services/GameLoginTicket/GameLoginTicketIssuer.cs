using System.Security.Claims;
using GameFrameX.Client.Api.Services.ClientApiOptions;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GameFrameX.Client.Api.Services.GameLoginTicket;

/// <summary>
/// Game 登录票据签发服务。
/// <para>issuer 固定为 <c>gameframex-client-api</c>，audience 固定为 <c>game-server</c>；票据到期时间从运行票据策略读取，0 表示永不过期，非 0 值受平台 30 天上限保护；签名 key 由 <see cref="IGameLoginTicketKeyResolver"/> 管理。</para>
/// </summary>
public sealed class GameLoginTicketIssuer
{
    private readonly IGameLoginTicketKeyResolver _keyResolver;
    private readonly IClientApiOptionsProvider _optionsProvider;
    private readonly JsonWebTokenHandler _handler = new() { SetDefaultTimesOnTokenCreation = false, };

    /// <summary>
    /// </summary>
    public GameLoginTicketIssuer(IGameLoginTicketKeyResolver keyResolver, IClientApiOptionsProvider optionsProvider)
    {
        _keyResolver = keyResolver;
        _optionsProvider = optionsProvider;
    }

    /// <summary>
    /// 签发 Game Server 准入票据。
    /// </summary>
    public async Task<GameLoginTicketIssueResponse> IssueAsync(GameLoginTicketIssueContext context)
    {
        var now = DateTimeOffset.UtcNow;
        var policy = await _optionsProvider.GetGameLoginTicketPolicyAsync();
        var nowUnix = TimerHelper.DateTimeToSeconds(now.UtcDateTime, true);
        var normalizedExpireTime = ClientApiPlatformDefaults.NormalizeGameLoginTicketExpireTime(policy.TicketExpireTime, nowUnix);
        long? expiresAt = normalizedExpireTime > 0 ? normalizedExpireTime : null;
        var jti = Guid.NewGuid().ToString("N");
        var signingKey = await _keyResolver.ResolveSigningKeyAsync();
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = ClientApiPlatformDefaults.GameLoginTicketIssuer,
            Audience = ClientApiPlatformDefaults.GameLoginTicketAudience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = TimerHelper.TimestampSecondsToDateTime(expiresAt, true),
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(JwtRegisteredClaimNames.Iat, nowUnix.ToString(), ClaimValueTypes.Integer64),
                new Claim("tokenUse", "game_login_ticket"),
                new Claim("userId", context.UserId.ToString()),
                new Claim("playerId", context.PlayerId.ToString()),
                new Claim("areaId", context.AreaId.ToString()),
                new Claim("serverId", context.AreaId.ToString()),
                new Claim("routeTarget", context.RouteTarget),
            }),
            SigningCredentials = signingKey.SigningCredentials,
        };

        var token = _handler.CreateToken(descriptor);
        var audit = new GameLoginTicketAudit(context.UserId, context.PlayerId, context.AreaId, context.RouteTarget, jti, true, string.Empty, nowUnix);
        LogHelper.Info($"GameLoginTicketIssued userId={audit.UserId} playerId={audit.PlayerId} areaId={audit.AreaId} kid={signingKey.Kid} jti={audit.Jti}");
        return new GameLoginTicketIssueResponse(token, expiresAt, jti, signingKey.Kid);
    }

    /// <summary>
    /// 记录拒绝签发票据的审计事件。
    /// </summary>
    public void AuditRejected(long userId, long playerId, long areaId, string reason)
    {
        var audit = new GameLoginTicketAudit(userId, playerId, areaId, string.Empty, string.Empty, false, reason, TimerHelper.UnixTimeSeconds());
        LogHelper.Warning($"GameLoginTicketRejected userId={audit.UserId} playerId={audit.PlayerId} areaId={audit.AreaId} reason={audit.Reason}");
    }
}