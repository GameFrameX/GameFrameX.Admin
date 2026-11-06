using GameFrameX.Client.Api.Services.GameLoginTicket;
using GameFrameX.Client.Api.Services.Auth;
using GameFrameX.Client.Api.GameRole;
using System.Security.Cryptography;
using System.Text.Json;
using GameFrameX.Client.Api.GameRole.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiRoleEndpointTests
{
    [Fact]
    public async Task GameRole_Add_CreatesRoleWithoutAppContext()
    {
        using var db = new ClientApiTestDb();
        db.SeedUser(1001, "role-user");
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults(3001);
        var controller = CreateController(db, handler);
        db.AttachContext(controller, includeApp: false);

        var result = await controller.Add(new AddTenantGameRoleEntityRequest
        {
            UserId = 1001,
            AreaId = 2001,
            Name = "Hero",
            HairstyleConfigId = 7,
            HeroConfigId = 8,
        });

        var root = ClientApiResultReader.Read(result);
        var saved = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(1001, saved.UserId);
        Assert.Equal(1, handler.Count("/game/api/req_player_create"));
    }

    [Fact]
    public async Task GameRole_Add_RejectsMissingAccountWithoutAppContext()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);
        db.AttachContext(controller, includeApp: false);

        var result = await controller.Add(new AddTenantGameRoleEntityRequest { UserId = 1001, AreaId = 2001, Name = "Hero", });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)AccountErrorCode.AccountExists, ClientApiResultReader.Code(root));
        Assert.Equal("账号不存在", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameRole_ChangePlayerName_UpdatesNameAfterGameServerAccepts()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001, name: "Old");
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler);
        db.AttachContext(controller);

        var result = await controller.ChangePlayerName(new GamePlayerChangeNameRequest { PlayerId = 3001, AreaId = 2001, Name = "New", });

        var root = ClientApiResultReader.Read(result);
        var saved = await db.TenantDb.Queryable<TenantGameRoleEntity>().FirstAsync(u => u.Id == 3001);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal("New", saved.Name);
    }

    [Fact]
    public async Task GameRole_ChangePlayerName_RejectsBlankName()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.ChangePlayerName(new GamePlayerChangeNameRequest { PlayerId = 3001, AreaId = 2001, Name = " ", });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)GameRoleErrorCode.RoleNameNotEmpty, ClientApiResultReader.Code(root));
        Assert.Equal("角色名称不能为空", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameRole_GetList_ReturnsOnlyRequestedUserRoles()
    {
        using var db = new ClientApiTestDb();
        db.SeedRole(3001, 1001, 2001, name: "Mine");
        db.SeedRole(3002, 1002, 2001, name: "Other");
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.GetList(new GetListTenantGameRoleEntityRequest { UserId = 1001, });

        var root = ClientApiResultReader.Read(result);
        var data = ClientApiResultReader.Data(root);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Single(data.EnumerateArray());
        Assert.Equal(3001, data[0].GetProperty("Id").GetInt64());
    }

    [Fact]
    public async Task GameRole_GetAndGetById_ReturnRoleDetails()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001, name: "Mine");
        var controller = CreateController(db);
        db.AttachContext(controller);

        var get = await controller.Get(new GetTenantGameRoleEntityRequest { UserId = 1001, AreaId = 2001, });
        var getById = await controller.GetById(new GetByIdTenantGameRoleEntityRequest { Id = "3001", });

        Assert.Equal("Mine", ClientApiResultReader.Data(ClientApiResultReader.Read(get)).GetProperty("Name").GetString());
        Assert.Equal("Area-2001", ClientApiResultReader.Data(ClientApiResultReader.Read(getById)).GetProperty("ServerName").GetString());
    }

    [Fact]
    public async Task GameRole_GetById_RejectsInvalidId()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.GetById(new GetByIdTenantGameRoleEntityRequest { Id = "not-a-role-id", });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)GameRoleErrorCode.RoleNotFound, ClientApiResultReader.Code(root));
        Assert.Equal("角色不存在", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameRole_Delete_FakeDeletesExistingRole()
    {
        using var db = new ClientApiTestDb();
        db.SeedRole(3001, 1001, 2001);
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Delete(new DeleteTenantGameRoleEntityRequest { Id = 3001, });

        var root = ClientApiResultReader.Read(result);
        var saved = await db.TenantDb.Queryable<TenantGameRoleEntity>().Filter(null, true).FirstAsync(u => u.Id == 3001);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.True(saved.IsDeleted);
    }

    [Fact]
    public async Task GameRole_Login_IssuesGameTicketForOwnedRole()
    {
        using var db = new ClientApiTestDb(10, 20);
        db.SeedUser(1001, "ticket-user");
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        var issuer = new ClientApiTokenIssuer(FakeOptionsProvider.WithTokenPolicy("unit-test-signing-key", 60));
        var accessToken = await issuer.IssueAsync(await db.TenantDb.Queryable<TenantGameUserEntity>().FirstAsync(u => u.Id == 1001), "Guest");
        var controller = CreateController(db, ClientApiRecordingHttpHandler.WithGameServerDefaults(), issuer);
        db.AttachContext(controller);

        var result = await controller.Login(new GamePlayerLoginRequest
        {
            PlayerId = 3001,
            AreaId = 2001,
            AccessToken = accessToken.Token,
        });

        var root = ClientApiResultReader.Read(result);
        var data = ClientApiResultReader.Data(root);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("GameLoginTicket").GetString()));
        Assert.Equal("wss://game.example.test/ws", data.GetProperty("RouteTarget").GetString());
    }

    [Fact]
    public async Task GameRole_Login_IgnoresUnknownLegacyContextFields()
    {
        using var db = new ClientApiTestDb();
        db.SeedUser(1001, "legacy-fields-user");
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.InitTenantTables(typeof(TenantGameUserEntity));
        var issuer = new ClientApiTokenIssuer(FakeOptionsProvider.WithTokenPolicy("unit-test-signing-key", 60));
        var accessToken = await issuer.IssueAsync(await db.TenantDb.Queryable<TenantGameUserEntity>().FirstAsync(u => u.Id == 1001), "Guest");
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var controller = CreateController(db, handler, issuer);
        db.AttachContext(controller);

        var result = await controller.Login(new GamePlayerLoginRequest
        {
            PlayerId = 3001,
            AreaId = 2001,
            AccessToken = accessToken.Token,
            ExtraFields = new Dictionary<string, JsonElement>
            {
                ["tenantId"] = JsonDocument.Parse("10").RootElement.Clone(), // c406-keep
                ["appId"] = JsonDocument.Parse("20").RootElement.Clone(), // c406-keep
                ["gameAppId"] = JsonDocument.Parse("30").RootElement.Clone(), // c406-keep
            },
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Single(handler.RequestBodies);
        using (var body = JsonDocument.Parse(handler.RequestBodies.Single()))
        {
            Assert.DoesNotContain("tenantId", body.RootElement.EnumerateObject().Select(property => property.Name), StringComparer.OrdinalIgnoreCase); // c406-keep
            Assert.DoesNotContain("appId", body.RootElement.EnumerateObject().Select(property => property.Name), StringComparer.OrdinalIgnoreCase); // c406-keep
            Assert.DoesNotContain("gameAppId", body.RootElement.EnumerateObject().Select(property => property.Name), StringComparer.OrdinalIgnoreCase); // c406-keep
        }

        Assert.DoesNotContain(handler.RequestUris, uri => uri.Contains("tenantId", StringComparison.OrdinalIgnoreCase)); // c406-keep
        Assert.DoesNotContain(handler.RequestUris, uri => uri.Contains("appId", StringComparison.OrdinalIgnoreCase)); // c406-keep
        Assert.DoesNotContain(handler.RequestUris, uri => uri.Contains("gameAppId", StringComparison.OrdinalIgnoreCase)); // c406-keep
    }

    [Fact]
    public async Task GameRole_Login_RejectsIdentityOverrideUserId()
    {
        using (var db = new ClientApiTestDb())
        {
            db.SeedUser(1001, "identity-override-user");
            db.SeedAreaClassify(10);
            db.SeedArea(2001, 10);
            db.SeedRole(3001, 1001, 2001);
            db.InitTenantTables(typeof(TenantGameUserEntity));
            var issuer = new ClientApiTokenIssuer(FakeOptionsProvider.WithTokenPolicy("unit-test-signing-key", 60));
            var accessToken = await issuer.IssueAsync(await db.TenantDb.Queryable<TenantGameUserEntity>().FirstAsync(user => user.Id == 1001), "Guest");
            var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
            var controller = CreateController(db, handler, issuer);
            db.AttachContext(controller);

            var result = await controller.Login(new GamePlayerLoginRequest
            {
                PlayerId = 3001,
                AreaId = 2001,
                AccessToken = accessToken.Token,
                ExtraFields = new Dictionary<string, JsonElement>
                {
                    ["userId"] = JsonDocument.Parse("9999").RootElement.Clone(), // c406-keep
                },
            });

            var root = ClientApiResultReader.Read(result);
            Assert.Equal((int)AuthErrorCode.AuthorizeInvalid, ClientApiResultReader.Code(root));
            Assert.Equal("请求体不能包含身份覆盖字段", ClientApiResultReader.Message(root));
            Assert.Empty(handler.RequestBodies);
        }
    }

    [Fact]
    public async Task GameRole_Update_PropagatesOptimisticLockConflict()
    {
        using var db = new ClientApiTestDb();
        var role = db.SeedRole(3001, 1001, 2001, name: "Old");
        role.RowVersion = 7;
        await db.TenantDb.Updateable(role).ExecuteCommandAsync();
        var controller = CreateController(db);
        db.AttachContext(controller);

        await Assert.ThrowsAsync<VersionExceptions>(() => controller.Update(new UpdateTenantGameRoleEntityRequest
        {
            Id = 3001,
            UserId = 1001,
            AreaId = 2001,
            Name = "New",
        }));
    }

    private static GameRoleController CreateController(
        ClientApiTestDb db,
        ClientApiRecordingHttpHandler? handler = null,
        ClientApiTokenIssuer? issuer = null,
        GameLoginTicketIssuer? ticketIssuer = null,
        FakeGamePenaltyEnforcementService? penaltyEnforcement = null)
    {
        db.InitTenantTables(typeof(TenantGameRoleEntity), typeof(TenantGameAreaEntity), typeof(TenantGameUserEntity));
        return new GameRoleController(
            new HttpClient(handler ?? ClientApiRecordingHttpHandler.WithGameServerDefaults(), false),
            db.Repository<TenantGameRoleEntity>(),
            db.Repository<TenantGameAreaEntity>(),
            issuer ?? new ClientApiTokenIssuer(FakeOptionsProvider.WithTokenPolicy("unit-test-signing-key", 60)),
            ticketIssuer ?? new GameLoginTicketIssuer(TestGameLoginTicketKeyResolver.Create(), FakeOptionsProvider.WithTicketPolicy(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60)),
            penaltyEnforcement ?? new FakeGamePenaltyEnforcementService());
    }

    private sealed class TestGameLoginTicketKeyResolver : IGameLoginTicketKeyResolver
    {
        private readonly GameLoginTicketSigningKey _key;

        private TestGameLoginTicketKeyResolver(GameLoginTicketSigningKey key)
        {
            _key = key;
        }

        public static TestGameLoginTicketKeyResolver Create()
        {
            using var rsa = RSA.Create(2048);
            var privateKeyPem = rsa.ExportPkcs8PrivateKeyPem();
            var publicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();
            return new TestGameLoginTicketKeyResolver(GameLoginTicketSigningKey.CreateRsa("tenant-10-app-20-role-test", privateKeyPem, publicKeyPem));
        }

        public Task<GameLoginTicketSigningKey> ResolveSigningKeyAsync()
        {
            return Task.FromResult(_key);
        }

        public Task<IReadOnlyCollection<GameLoginTicketPublicKey>> GetPublicKeysAsync()
        {
            IReadOnlyCollection<GameLoginTicketPublicKey> keys = new[]
            {
                new GameLoginTicketPublicKey(_key.Kid, _key.Algorithm, _key.PublicKeyPem, null, null),
            };
            return Task.FromResult(keys);
        }
    }
}
