using GameFrameX.Application.Game.Tenant.ClientApiPolicy.TenantClientApiPolicy;
using GameFrameX.Application.Game.Tenant.ClientApiPolicy.TenantClientApiPolicy.Dto;
using GameFrameX.Application.Game.Tenant.LoginTicket.TenantGameLoginTicketKey;
using GameFrameX.Application.Game.Tenant.LoginTicket.TenantGameLoginTicketKey.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Core.Extension;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.Tenant;

// C373（GFX-838）T1/T2/T3：密钥生命周期操作集。
// change：gfx-doc/dev/client-api/changes/C373-credential-key-lifecycle/change.md
// TicketKey Rotate：旧钥退场参数（默认 now+7d、上限 now+30d、<=now 拒）+ RotatedTime 标记 + ExpireTime 收窄；
// 策略合并表 SigningKey Rotate：新 64-hex 密文即刻替换（旧值作废）、UpdateTime 推进、响应无密钥；
// 审计：两个 Rotate 端点源码级断言含 [KeyAudit] 结构化审计行（DB 操作日志管线断链期的调用意图守护）。
public sealed class KeyLifecycleRotateTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();
    private readonly string? _originalKey;

    public KeyLifecycleRotateTests()
    {
        _originalKey = Environment.GetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        _db.InitTenantTables(typeof(TenantGameLoginTicketKeyEntity), typeof(TenantClientApiPolicyEntity));
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, _originalKey);
        _db.Dispose();
    }

    private TenantGameLoginTicketKeyEntityService CreateTicketKeyService()
    {
        return new TenantGameLoginTicketKeyEntityService(
            new Repository<TenantGameLoginTicketKeyEntity>(_db.MainDb, _db.TenantDbProvider, _db.TenantIdResolver),
            new GameFrameX.Core.Service.User.CurrentUserContext(new Microsoft.AspNetCore.Http.HttpContextAccessor()));
    }

    private TenantClientApiPolicyEntityService CreatePolicyService()
    {
        return new TenantClientApiPolicyEntityService(
            new Repository<TenantClientApiPolicyEntity>(_db.MainDb, _db.TenantDbProvider, _db.TenantIdResolver),
            new GameFrameX.Core.Service.User.CurrentUserContext(new Microsoft.AspNetCore.Http.HttpContextAccessor()));
    }

    /// <summary>
    /// 预置一把「当前生效」的旧钥（模拟既有密钥形态；密文与否不影响轮换标记逻辑，用密文形态贴近 C371 后库内现状）。
    /// </summary>
    private TenantGameLoginTicketKeyEntity SeedActiveTicketKey()
    {
        var oldKey = new TenantGameLoginTicketKeyEntity
        {
            AppId = _db.AppId,
            Kid = "kid-old-active",
            Algorithm = "RS256",
            PrivateKeyPem = TenantSecretCrypto.Encrypt("old-private-pem") ?? string.Empty,
            PublicKeyPem = "old-public-pem",
            NotBeforeTime = TimerHelper.UnixTimeSeconds() - 3600,
            ExpireTime = null,
            IsEnabled = true,
            Remark = "seed",
        };
        _db.TenantDb.Insertable(oldKey).ExecuteCommand();
        return _db.TenantDb.Queryable<TenantGameLoginTicketKeyEntity>().First(u => u.Kid == "kid-old-active");
    }

    [Fact]
    public async Task TicketKeyRotate_DefaultOldKeyExpiresIn7Days()
    {
        var oldKey = SeedActiveTicketKey();
        var before = TimerHelper.UnixTimeSeconds();

        await CreateTicketKeyService().RotateAsync(new TenantGameLoginTicketKeyRotateRequest { AppId = _db.AppId, });

        var after = TimerHelper.UnixTimeSeconds();
        var storedOld = _db.TenantDb.Queryable<TenantGameLoginTicketKeyEntity>().First(u => u.Id == oldKey.Id);
        Assert.NotNull(storedOld.RotatedTime);
        Assert.InRange(storedOld.RotatedTime.Value, before, after);
        Assert.InRange(storedOld.ExpireTime!.Value, before + TenantGameLoginTicketKeyEntityService.OldKeyDefaultExpireSeconds,
                       after + TenantGameLoginTicketKeyEntityService.OldKeyDefaultExpireSeconds);

        // 新钥立即生效（NotBefore ∈ [before, after]），且未被标记轮换
        var newKey = _db.TenantDb.Queryable<TenantGameLoginTicketKeyEntity>().First(u => u.Kid != "kid-old-active");
        Assert.InRange(newKey.NotBeforeTime!.Value, before, after);
        Assert.Null(newKey.RotatedTime);
    }

    [Fact]
    public async Task TicketKeyRotate_ExplicitOldKeyExpireWithin30Days_Applied()
    {
        var oldKey = SeedActiveTicketKey();
        var now = TimerHelper.UnixTimeSeconds();
        var explicitExpire = now + 30 * 24 * 3600; // 上限边界（恰好 30 天）应放行

        await CreateTicketKeyService().RotateAsync(new TenantGameLoginTicketKeyRotateRequest { AppId = _db.AppId, OldKeyExpireTime = explicitExpire, });

        var storedOld = _db.TenantDb.Queryable<TenantGameLoginTicketKeyEntity>().First(u => u.Id == oldKey.Id);
        Assert.Equal(explicitExpire, storedOld.ExpireTime);
        Assert.NotNull(storedOld.RotatedTime);
    }

    [Fact]
    public async Task TicketKeyRotate_OldKeyExpireBeyond30Days_Rejected()
    {
        SeedActiveTicketKey();
        var request = new TenantGameLoginTicketKeyRotateRequest
        {
            AppId = _db.AppId,
            OldKeyExpireTime = TimerHelper.UnixTimeSeconds() + TenantGameLoginTicketKeyEntityService.OldKeyMaxExpireSeconds + 1,
        };

        await Assert.ThrowsAsync<BusinessException>(() => CreateTicketKeyService().RotateAsync(request));
    }

    [Fact]
    public async Task TicketKeyRotate_OldKeyExpireNotAfterNow_Rejected()
    {
        SeedActiveTicketKey();
        var request = new TenantGameLoginTicketKeyRotateRequest
        {
            AppId = _db.AppId,
            OldKeyExpireTime = TimerHelper.UnixTimeSeconds(),
        };

        await Assert.ThrowsAsync<BusinessException>(() => CreateTicketKeyService().RotateAsync(request));
    }

    [Fact]
    public async Task TicketKeyRotate_NoActiveOldKey_IsNoop()
    {
        // 无旧钥场景（首把密钥轮换）：仅生成新钥，不抛错
        await CreateTicketKeyService().RotateAsync(new TenantGameLoginTicketKeyRotateRequest { AppId = _db.AppId, });

        var count = _db.TenantDb.Queryable<TenantGameLoginTicketKeyEntity>().Count();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task PolicyRotate_GeneratesNewCipher_OldValueRevokedImmediately()
    {
        var added = await CreatePolicyService().AddAsync(new AddTenantClientApiPolicyRequest
        {
            AppId = 0,
            SigningKey = string.Empty,
            LifetimeMinutes = 30,
            TicketExpireTime = 0,
        });
        var cipherBefore = _db.TenantDb.Queryable<TenantClientApiPolicyEntity>().Filter(null, true).First(u => u.Id == added.Id).SigningKey;
        var plainBefore = TenantSecretCrypto.Decrypt(cipherBefore);
        Assert.StartsWith(TenantSecretCrypto.EncryptedPrefix, cipherBefore, StringComparison.Ordinal);

        var before = TimerHelper.UnixTimeSeconds();
        await CreatePolicyService().RotateAsync(new BaseIdRequest { Id = added.Id, });
        var after = TimerHelper.UnixTimeSeconds();

        var stored = _db.TenantDb.Queryable<TenantClientApiPolicyEntity>().Filter(null, true).First(u => u.Id == added.Id);
        // 新密文 ≠ 旧密文（旧值即刻作废），仍为密文形态且可解密为新 64-hex
        Assert.NotEqual(cipherBefore, stored.SigningKey);
        Assert.StartsWith(TenantSecretCrypto.EncryptedPrefix, stored.SigningKey, StringComparison.Ordinal);
        var plainAfter = TenantSecretCrypto.Decrypt(stored.SigningKey);
        Assert.NotEqual(plainBefore, plainAfter);
        Assert.Equal(64, plainAfter!.Length);
        Assert.Matches("^[0-9a-f]{64}$", plainAfter);
        // UpdateTime 推进（审计/缓存信号可见）
        Assert.InRange(stored.UpdateTime!.Value, before, after);
    }

    [Fact]
    public async Task PolicyRotate_MissingRecord_Throws()
    {
        await Assert.ThrowsAsync<BusinessException>(() => CreatePolicyService().RotateAsync(new BaseIdRequest { Id = 41060401, }));
    }

    /// <summary>
    /// 审计调用意图守护（T3）：DB 操作日志管线断链期，两个新 Rotate 端点必须保留 [KeyAudit] 结构化审计行
    /// （含操作人/租户/应用/目标），管线修复后统一接入 DB 审计。删除审计行即失败。
    /// </summary>
    [Fact]
    public void RotateEndpoints_EmitKeyAuditLogLines()
    {
        var repositoryRoot = GetRepositoryRoot();
        var ticketKeySource = File.ReadAllText(Path.Combine(repositoryRoot,
            "GameFrameX.Application.Game/Tenant/LoginTicket/TenantGameLoginTicketKey/TenantGameLoginTicketKeyEntityService.cs"));
        var policySource = File.ReadAllText(Path.Combine(repositoryRoot,
            "GameFrameX.Application.Game/Tenant/ClientApiPolicy/TenantClientApiPolicy/TenantClientApiPolicyEntityService.cs"));

        Assert.Contains("[KeyAudit] action=TicketKeyRotate", ticketKeySource, StringComparison.Ordinal);
        Assert.Contains("operator={_currentUserContext.Account}", ticketKeySource, StringComparison.Ordinal);
        Assert.Contains("[KeyAudit] action=PolicySigningKeyRotate", policySource, StringComparison.Ordinal);
        Assert.Contains("operator={_currentUserContext.Account}", policySource, StringComparison.Ordinal);
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GameFrameX.Admin.Unified.slnx")))
        {
            directory = directory.Parent!;
        }

        return directory.FullName;
    }
}
