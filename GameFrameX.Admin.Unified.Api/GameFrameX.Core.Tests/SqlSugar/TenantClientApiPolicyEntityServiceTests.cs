using GameFrameX.Application.Game.Tenant.ClientApiPolicy.TenantClientApiPolicy;
using GameFrameX.Application.Game.Tenant.ClientApiPolicy.TenantClientApiPolicy.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.ClientApiConfig;

namespace GameFrameX.Core.Tests.SqlSugar;

// C378（GFX-838）+ C368/C371 组合：合并表写路径语义。
// change：gfx-doc/dev/game-business/changes/C378-client-api-policy-merge/change.md
// AddAsync 空白 SigningKey → 服务端生成 64-hex；(TenantId, AppId) 作用域唯一；
// 票据固定到期时间 30 天上限（C142）；UpdateAsync <c>******</c> 与空白同义保留原值（C368）。
public sealed class TenantClientApiPolicyEntityServiceTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();

    public TenantClientApiPolicyEntityServiceTests()
    {
        _db.InitTenantTables(typeof(TenantClientApiPolicyEntity));
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private TenantClientApiPolicyEntityService CreateService()
    {
        return new TenantClientApiPolicyEntityService(
            new Repository<TenantClientApiPolicyEntity>(_db.MainDb, _db.TenantDbProvider, _db.TenantIdResolver),
            new GameFrameX.Core.Service.User.CurrentUserContext(new Microsoft.AspNetCore.Http.HttpContextAccessor()));
    }

    [Fact]
    public async Task AddAsync_BlankSigningKey_Generates64Hex()
    {
        var service = CreateService();

        await service.AddAsync(new AddTenantClientApiPolicyRequest
        {
            AppId = 0,
            SigningKey = string.Empty,
            LifetimeMinutes = 60,
            TicketExpireTime = 0,
        });

        var row = _db.TenantDb.Queryable<TenantClientApiPolicyEntity>().Single(x => x.AppId == 0);
        Assert.Matches("^[0-9a-f]{64}$", row.SigningKey);
    }

    [Fact]
    public async Task AddAsync_DuplicateScope_ThrowsRecordAlreadyExists()
    {
        var service = CreateService();

        await service.AddAsync(new AddTenantClientApiPolicyRequest { AppId = 0, SigningKey = "k1", });
        await service.AddAsync(new AddTenantClientApiPolicyRequest { AppId = 5, SigningKey = "k2", });

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddTenantClientApiPolicyRequest { AppId = 0, SigningKey = "k3", }));
    }

    [Fact]
    public async Task AddAsync_TicketExpireBeyond30Days_ThrowsValidation()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddTenantClientApiPolicyRequest
        {
            AppId = 0,
            SigningKey = "k1",
            TicketExpireTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 31L * 24 * 60 * 60,
        }));
    }

    [Fact]
    public async Task UpdateAsync_MaskedSigningKey_PreservesStoredValue()
    {
        var service = CreateService();
        var created = await service.AddAsync(new AddTenantClientApiPolicyRequest { AppId = 0, SigningKey = "original-key", });

        await service.UpdateAsync(new UpdateTenantClientApiPolicyRequest
        {
            Id = created.Id,
            AppId = 0,
            SigningKey = SensitiveFieldMask.MaskValue,
            LifetimeMinutes = 90,
            TicketExpireTime = 0,
            RowVersion = 0,
        });

        var row = _db.TenantDb.Queryable<TenantClientApiPolicyEntity>().Single(x => x.AppId == 0);
        Assert.Equal("original-key", row.SigningKey);
        Assert.Equal(90, row.LifetimeMinutes);
    }

    [Fact]
    public async Task UpdateAsync_BlankSigningKey_PreservesStoredValue()
    {
        var service = CreateService();
        var created = await service.AddAsync(new AddTenantClientApiPolicyRequest { AppId = 0, SigningKey = "original-key", });

        // 空白与掩码同义（C368）
        await service.UpdateAsync(new UpdateTenantClientApiPolicyRequest
        {
            Id = created.Id,
            AppId = 0,
            SigningKey = string.Empty,
            LifetimeMinutes = 120,
            TicketExpireTime = 0,
            RowVersion = 0,
        });

        var row = _db.TenantDb.Queryable<TenantClientApiPolicyEntity>().Single(x => x.AppId == 0);
        Assert.Equal("original-key", row.SigningKey);
        Assert.Equal(120, row.LifetimeMinutes);
    }
}
