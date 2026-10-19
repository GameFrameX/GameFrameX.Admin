using GameFrameX.Core.Service.PlatformJwt;

namespace GameFrameX.Core.Tests.System;

internal sealed class FakePlatformJwtTokenService : IPlatformJwtTokenService
{
    /// <summary>
    /// 最近一次 CreateAccessTokenAsync 收到的 payload（场景 ③ Claim 校验）。
    /// </summary>
    public IDictionary<string, object>? LastAccessTokenPayload { get; private set; }

    public Task<string> CreateAccessTokenAsync(IDictionary<string, object> payload, long expiredMinutes)
    {
        LastAccessTokenPayload = payload;
        return Task.FromResult("fake-access-token");
    }

    public Task<string> GenerateRefreshTokenAsync(string accessToken, int expiredMinutes)
    {
        return Task.FromResult("fake-refresh-token");
    }
}