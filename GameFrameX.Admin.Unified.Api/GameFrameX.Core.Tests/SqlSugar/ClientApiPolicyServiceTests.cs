using GameFrameX.Application.ClientDelivery.ClientApiPolicy;
using GameFrameX.Application.ClientDelivery.ClientApiPolicy.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Business.ClientApiConfig;

namespace GameFrameX.Core.Tests.SqlSugar;

// C378（GFX-838）+ C368/C371 组合：合并表写路径语义。
// change：gfx-doc/dev/game-business/changes/C378-client-api-policy-merge/change.md
// AddAsync 空白 SigningKey → 服务端生成 64-hex；(Id, AppId) 作用域唯一；
// 票据固定到期时间 30 天上限（C142）；UpdateAsync <c>******</c> 与空白同义保留原值（C368）。
public sealed class ClientApiPolicyServiceTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();

    public ClientApiPolicyServiceTests()
    {
        _db.InitBusinessTables(typeof(ClientApiPolicyEntity));
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private ClientApiPolicyService CreateService()
    {
        return new ClientApiPolicyService(
            new Repository<ClientApiPolicyEntity>(_db.MainDb),
            new GameFrameX.Core.Service.User.CurrentUserContext(new Microsoft.AspNetCore.Http.HttpContextAccessor()));
    }

    [Fact]
    public async Task AddAsync_BlankSigningKey_Generates64Hex()
    {
        var service = CreateService();

        await service.AddAsync(new AddClientApiPolicyRequest
        {
            SigningKey = string.Empty,
            LifetimeMinutes = 60,
            TicketExpireTime = 0,
        });

        var row = _db.Db.Queryable<ClientApiPolicyEntity>().Single();
        Assert.Matches("^[0-9a-f]{64}$", row.SigningKey);
    }

    [Fact]
    public async Task AddAsync_DuplicateScope_ThrowsRecordAlreadyExists()
    {
        var service = CreateService();

        await service.AddAsync(new AddClientApiPolicyRequest { SigningKey = "k1", });

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddClientApiPolicyRequest { SigningKey = "k3", }));
    }

    [Fact]
    public async Task AddAsync_TicketExpireBeyond30Days_ThrowsValidation()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddClientApiPolicyRequest
        {
            SigningKey = "k1",
            TicketExpireTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 31L * 24 * 60 * 60,
        }));
    }

    [Fact]
    public async Task UpdateAsync_MaskedSigningKey_PreservesStoredValue()
    {
        var service = CreateService();
        var created = await service.AddAsync(new AddClientApiPolicyRequest { SigningKey = "original-key", });

        await service.UpdateAsync(new UpdateClientApiPolicyRequest
        {
            Id = created.Id,
            SigningKey = SensitiveFieldMask.MaskValue,
            LifetimeMinutes = 90,
            TicketExpireTime = 0,
            RowVersion = 0,
        });

        var row = _db.Db.Queryable<ClientApiPolicyEntity>().Single();
        Assert.Equal("original-key", row.SigningKey);
        Assert.Equal(90, row.LifetimeMinutes);
    }

    [Fact]
    public async Task UpdateAsync_BlankSigningKey_PreservesStoredValue()
    {
        var service = CreateService();
        var created = await service.AddAsync(new AddClientApiPolicyRequest { SigningKey = "original-key", });

        // 空白与掩码同义（C368）
        await service.UpdateAsync(new UpdateClientApiPolicyRequest
        {
            Id = created.Id,
            SigningKey = string.Empty,
            LifetimeMinutes = 120,
            TicketExpireTime = 0,
            RowVersion = 0,
        });

        var row = _db.Db.Queryable<ClientApiPolicyEntity>().Single();
        Assert.Equal("original-key", row.SigningKey);
        Assert.Equal(120, row.LifetimeMinutes);
    }
}
