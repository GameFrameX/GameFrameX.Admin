using GameFrameX.Application.ClientDelivery.ClientApiPolicy;
using GameFrameX.Application.ClientDelivery.ClientApiPolicy.Dto;
using GameFrameX.Application.ClientDelivery.LoginTicket.GameLoginTicketKey;
using GameFrameX.Application.ClientDelivery.LoginTicket.GameLoginTicketKey.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Core.Extension;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Business;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Business.ClientApiConfig;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Core.Tests.Business;

// //：密钥生命周期操作集。
// change：gfx-doc/dev/client-api/changes/-credential-key-lifecycle/change.md
// TicketKey Rotate：旧钥退场参数（默认 now+7d、上限 now+30d、<=now 拒）+ RotatedTime 标记 + ExpireTime 收窄；
// 策略合并表 SigningKey Rotate：新 64-hex 密文即刻替换（旧值作废）、UpdateTime 推进、响应无密钥；
// 审计：两个 Rotate 端点源码级断言含 [KeyAudit] 结构化审计行（DB 操作日志管线断链期的调用意图守护）。
public sealed class KeyLifecycleRotateTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();
    private readonly string? _originalKey;

    public KeyLifecycleRotateTests()
    {
        _originalKey = Environment.GetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        _db.InitBusinessTables(typeof(GameLoginTicketKeyEntity), typeof(ClientApiPolicyEntity));
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, _originalKey);
        _db.Dispose();
    }

    private GameLoginTicketKeyService CreateTicketKeyService()
    {
        return new GameLoginTicketKeyService(
            new Repository<GameLoginTicketKeyEntity>(_db.MainDb),
            new GameFrameX.Core.Service.User.CurrentUserContext(new Microsoft.AspNetCore.Http.HttpContextAccessor()));
    }

    private ClientApiPolicyService CreatePolicyService()
    {
        return new ClientApiPolicyService(
            new Repository<ClientApiPolicyEntity>(_db.MainDb),
            new GameFrameX.Core.Service.User.CurrentUserContext(new Microsoft.AspNetCore.Http.HttpContextAccessor()));
    }

    /// <summary>
    /// 预置一把「当前生效」的旧钥（模拟既有密钥形态；密文与否不影响轮换标记逻辑，用密文形态贴近 后库内现状）。
    /// </summary>
    private GameLoginTicketKeyEntity SeedActiveTicketKey()
    {
        var oldKey = new GameLoginTicketKeyEntity
        {
           
            Kid = "kid-old-active",
            Algorithm = "RS256",
            PrivateKeyPem = SecretCrypto.Encrypt("old-private-pem") ?? string.Empty,
            PublicKeyPem = "old-public-pem",
            NotBeforeTime = TimerHelper.UnixTimeSeconds() - 3600,
            ExpireTime = null,
            IsEnabled = true,
            Remark = "seed",
        };
        _db.MainDb.Insertable(oldKey).ExecuteCommand();
        return _db.MainDb.Queryable<GameLoginTicketKeyEntity>().First(u => u.Kid == "kid-old-active");
    }

    [Fact]
    public async Task TicketKeyRotate_DefaultOldKeyExpiresIn7Days()
    {
        var oldKey = SeedActiveTicketKey();
        var before = TimerHelper.UnixTimeSeconds();

        await CreateTicketKeyService().RotateAsync(new GameLoginTicketKeyRotateRequest { });

        var after = TimerHelper.UnixTimeSeconds();
        var storedOld = _db.MainDb.Queryable<GameLoginTicketKeyEntity>().First(u => u.Id == oldKey.Id);
        Assert.NotNull(storedOld.RotatedTime);
        Assert.InRange(storedOld.RotatedTime.Value, before, after);
        Assert.InRange(storedOld.ExpireTime!.Value, before + GameLoginTicketKeyService.OldKeyDefaultExpireSeconds,
                       after + GameLoginTicketKeyService.OldKeyDefaultExpireSeconds);

        // 新钥立即生效（NotBefore ∈ [before, after]），且未被标记轮换
        var newKey = _db.MainDb.Queryable<GameLoginTicketKeyEntity>().First(u => u.Kid != "kid-old-active");
        Assert.InRange(newKey.NotBeforeTime!.Value, before, after);
        Assert.Null(newKey.RotatedTime);
    }

    [Fact]
    public async Task TicketKeyRotate_ExplicitOldKeyExpireWithin30Days_Applied()
    {
        var oldKey = SeedActiveTicketKey();
        var now = TimerHelper.UnixTimeSeconds();
        var explicitExpire = now + 30 * 24 * 3600; // 上限边界（恰好 30 天）应放行

        await CreateTicketKeyService().RotateAsync(new GameLoginTicketKeyRotateRequest { OldKeyExpireTime = explicitExpire, });

        var storedOld = _db.MainDb.Queryable<GameLoginTicketKeyEntity>().First(u => u.Id == oldKey.Id);
        Assert.Equal(explicitExpire, storedOld.ExpireTime);
        Assert.NotNull(storedOld.RotatedTime);
    }

    [Fact]
    public async Task TicketKeyRotate_OldKeyExpireBeyond30Days_Rejected()
    {
        SeedActiveTicketKey();
        var request = new GameLoginTicketKeyRotateRequest
        {
           
            OldKeyExpireTime = TimerHelper.UnixTimeSeconds() + GameLoginTicketKeyService.OldKeyMaxExpireSeconds + 1,
        };

        await Assert.ThrowsAsync<BusinessException>(() => CreateTicketKeyService().RotateAsync(request));
    }

    [Fact]
    public async Task TicketKeyRotate_OldKeyExpireNotAfterNow_Rejected()
    {
        SeedActiveTicketKey();
        var request = new GameLoginTicketKeyRotateRequest
        {
           
            OldKeyExpireTime = TimerHelper.UnixTimeSeconds(),
        };

        await Assert.ThrowsAsync<BusinessException>(() => CreateTicketKeyService().RotateAsync(request));
    }

    [Fact]
    public async Task TicketKeyRotate_NoActiveOldKey_IsNoop()
    {
        // 无旧钥场景（首把密钥轮换）：仅生成新钥，不抛错
        await CreateTicketKeyService().RotateAsync(new GameLoginTicketKeyRotateRequest { });

        var count = _db.MainDb.Queryable<GameLoginTicketKeyEntity>().Count();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task PolicyRotate_GeneratesNewCipher_OldValueRevokedImmediately()
    {
        var added = await CreatePolicyService().AddAsync(new AddClientApiPolicyRequest
        {
            SigningKey = string.Empty,
            LifetimeMinutes = 30,
            TicketExpireTime = 0,
        });
        var cipherBefore = _db.MainDb.Queryable<ClientApiPolicyEntity>().Filter(null, true).First(u => u.Id == added.Id).SigningKey;
        var plainBefore = SecretCrypto.Decrypt(cipherBefore);
        Assert.StartsWith(SecretCrypto.EncryptedPrefix, cipherBefore, StringComparison.Ordinal);

        var before = TimerHelper.UnixTimeSeconds();
        await CreatePolicyService().RotateAsync(new BaseIdRequest { Id = added.Id, });
        var after = TimerHelper.UnixTimeSeconds();

        var stored = _db.MainDb.Queryable<ClientApiPolicyEntity>().Filter(null, true).First(u => u.Id == added.Id);
        // 新密文 ≠ 旧密文（旧值即刻作废），仍为密文形态且可解密为新 64-hex
        Assert.NotEqual(cipherBefore, stored.SigningKey);
        Assert.StartsWith(SecretCrypto.EncryptedPrefix, stored.SigningKey, StringComparison.Ordinal);
        var plainAfter = SecretCrypto.Decrypt(stored.SigningKey);
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
    /// 审计调用意图守护：DB 操作日志管线断链期，两个新 Rotate 端点必须保留 [KeyAudit] 结构化审计行
    /// （含操作人/目标键），管线修复后统一接入 DB 审计。删除审计行即失败。
    /// </summary>
    [Fact]
    public void RotateEndpoints_EmitKeyAuditLogLines()
    {
        var repositoryRoot = GetRepositoryRoot();
        var ticketKeySource = File.ReadAllText(Path.Combine(repositoryRoot,
            "GameFrameX.Application/ClientDelivery/LoginTicket/GameLoginTicketKey/GameLoginTicketKeyService.cs"));
        var policySource = File.ReadAllText(Path.Combine(repositoryRoot,
            "GameFrameX.Application/ClientDelivery/ClientApiPolicy/ClientApiPolicyService.cs"));

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
