using GameFrameX.Client.Api.Services.Auth;
using System.Text.RegularExpressions;
using GameFrameX.Client.Api.Services;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// C7 登录会话密钥签发：kid/secret 契约、租约落库、同应用过期行懒清理（SQLite 租户库，复用 ClientApiTestDb 模式）。
/// </summary>
public sealed class ClientApiSessionKeyIssuerTests
{
    private static readonly Regex KidWirePattern = new("^[A-Za-z0-9._-]{1,64}$", RegexOptions.Compiled);
    private static readonly Regex KidShapePattern = new(@"^sess-app-\d+-\d{14}-[0-9a-f]{4}$", RegexOptions.Compiled);

    [Fact]
    public async Task Issue_PersistsKeyWithLeaseAlignedToExpire()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantSessionKeyEntity));
        var issuer = new ClientApiSessionKeyIssuer(db.Repository<TenantSessionKeyEntity>());
        var before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var expireAt = before + 3600;

        var result = await issuer.IssueAsync(db.AppId, expireAt);

        Assert.Matches(KidShapePattern, result.Kid);
        Assert.Matches(KidWirePattern, result.Kid);
        Assert.Matches("^[0-9a-f]{64}$", result.Secret);
        Assert.Equal(expireAt, result.ExpireTime);

        var saved = await db.TenantDb.Queryable<TenantSessionKeyEntity>().FirstAsync(e => e.Kid == result.Kid);
        Assert.NotNull(saved);
        Assert.Equal(db.AppId, saved.AppId);
        Assert.Equal(result.Secret, saved.Secret);
        Assert.Equal(expireAt, saved.ExpireTime);
        Assert.NotNull(saved.NotBeforeTime);
        Assert.InRange(saved.NotBeforeTime.Value, before - 5, DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 5);
        Assert.True(saved.IsEnabled);
        Assert.Contains("auto-issue", saved.Remark);
    }

    [Fact]
    public async Task Issue_DeletesExpiredKeysOfSameApp_KeepsLiveAndOtherAppKeys()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantSessionKeyEntity));
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        db.TenantDb.Insertable(new[]
        {
            Seed(db.AppId, "sess-app-1-expired", now - 10),
            Seed(db.AppId, "sess-app-1-live", now + 3600),
            Seed(db.AppId + 1, "sess-app-2-expired", now - 10),
        }).ExecuteCommand();

        var issuer = new ClientApiSessionKeyIssuer(db.Repository<TenantSessionKeyEntity>());
        await issuer.IssueAsync(db.AppId, now + 120);

        var kids = await db.TenantDb.Queryable<TenantSessionKeyEntity>().Select(e => e.Kid).ToListAsync();
        Assert.DoesNotContain("sess-app-1-expired", kids);
        Assert.Contains("sess-app-1-live", kids);
        // 其它 AppId 的过期行不在本次签发清理范围
        Assert.Contains("sess-app-2-expired", kids);
    }

    private static TenantSessionKeyEntity Seed(long appId, string kid, long expireTime)
    {
        return new TenantSessionKeyEntity
        {
            AppId = appId,
            Kid = kid,
            Secret = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
            NotBeforeTime = 0,
            ExpireTime = expireTime,
            IsEnabled = true,
            IsDeleted = false,
        };
    }
}