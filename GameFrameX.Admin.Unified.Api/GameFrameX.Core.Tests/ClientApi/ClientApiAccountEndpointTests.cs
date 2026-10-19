using GameFrameX.Client.Api.Services.Auth;
using GameFrameX.Client.Api.GameUser;
using GameFrameX.Client.Api.GameUser.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Application.Game.User.GameUser;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiAccountEndpointTests
{
    [Fact]
    public async Task GameUser_Register_ReturnsPascalCaseAccessTokenAndPersistsActiveUser()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Register(new GameUserGuestRegisterEntityRequest
        {
            UserName = "guest-c108",
            Password = "password",
        });

        var root = ClientApiResultReader.Read(result);
        var data = ClientApiResultReader.Data(root);
        var saved = await db.TenantDb.Queryable<TenantGameUserEntity>().FirstAsync(u => u.UserName == "guest-c108");
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("AccessToken").GetString()));
        Assert.Equal("Guest", data.GetProperty("Provider").GetString());
        Assert.Equal(GameUserStatus.Active, saved.Status);
    }

    [Fact]
    public async Task GameUser_Register_RejectsDuplicateUserName()
    {
        using var db = new ClientApiTestDb();
        db.SeedUser(1001, "guest-c108");
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Register(new GameUserGuestRegisterEntityRequest
        {
            UserName = "guest-c108",
            Password = "password",
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)AccountErrorCode.AccountExists, ClientApiResultReader.Code(root));
        Assert.Equal("账号已存在", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameUser_Login_RejectsWrongPassword()
    {
        using var db = new ClientApiTestDb();
        db.SeedUser(1001, "guest-c108", "right-password");
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Login(new LoginTenantGameUserEntityRequest
        {
            UserName = "guest-c108",
            Password = "wrong-password",
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)AccountErrorCode.AccountOrPasswordError, ClientApiResultReader.Code(root));
        Assert.Equal("账号或密码错误", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameUser_Get_RejectsAccessTokenFromAnotherUser()
    {
        using var db = new ClientApiTestDb();
        var tokenUser = db.SeedUser(1001, "guest-token");
        db.SeedUser(1002, "guest-target");
        var issuer = CreateTokenIssuer();
        var token = await issuer.IssueAsync(tokenUser, "Guest");
        var controller = CreateController(db, issuer);
        db.AttachContext(controller);

        var result = await controller.Get(new QueryByIdTenantGameUserEntityRequest
        {
            Id = 1002,
            AccessToken = token.Token,
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)AuthErrorCode.AuthorizeInvalid, ClientApiResultReader.Code(root));
        Assert.Equal("授权无效", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameUser_LogOff_WithValidTokenMarksAccountDeleting()
    {
        using var db = new ClientApiTestDb();
        var user = db.SeedUser(1001, "guest-c108");
        var issuer = CreateTokenIssuer();
        var token = await issuer.IssueAsync(user, "Guest");
        var controller = CreateController(db, issuer);
        db.AttachContext(controller);

        var result = await controller.LogOff(new GameUserLogOffRequest
        {
            Id = user.Id,
            AccessToken = token.Token,
        });

        var root = ClientApiResultReader.Read(result);
        var saved = await db.TenantDb.Queryable<TenantGameUserEntity>().FirstAsync(u => u.Id == user.Id);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(GameUserStatus.Deleted, saved.Status);
        Assert.NotNull(saved.DeletedTime);
    }

    [Fact]
    public async Task GameUser_EmailRegister_RequiresActiveVerificationCode()
    {
        using var db = new ClientApiTestDb();
        db.SeedEmailCode(2001, "player@example.test", "123456", true);
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.EmailRegister(new GameUserEmailRegisterEntityRequest
        {
            UserName = "player@example.test",
            Password = "password",
            Code = "123456",
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)AccountErrorCode.EmailCodeNotFoundOrExpired, ClientApiResultReader.Code(root));
        Assert.Equal("验证码错误或已过期，请重新获取验证码。", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameUser_EmailRegister_WithValidCodeReturnsAccessToken()
    {
        using var db = new ClientApiTestDb();
        db.SeedEmailCode(2001, "player@example.test", "123456");
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.EmailRegister(new GameUserEmailRegisterEntityRequest
        {
            UserName = "player@example.test",
            Password = "password",
            Code = "123456",
        });

        var root = ClientApiResultReader.Read(result);
        var data = ClientApiResultReader.Data(root);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal("Email", data.GetProperty("Provider").GetString());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("AccessToken").GetString()));
        // C7: 登录响应随 token 一起回传 per-session 会话密钥 kid/secret
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("SessionKeyId").GetString()));
        Assert.Matches("^[0-9a-f]{64}$", data.GetProperty("SessionKeySecret").GetString());
    }

    [Fact]
    public async Task GameUser_EmailLogin_AcceptsCorrectPassword_RejectsWrongAndMissingAccount()
    {
        using var db = new ClientApiTestDb();
        // SeedUser 用 Argon2id 写入 EmailPassword，与生产 TenantGameUserPasswordService 一致（C215）
        db.SeedUser(2001, "player@example.test", "right-password", email: "player@example.test");
        var controller = CreateController(db);
        db.AttachContext(controller);

        // 正确密码 -> 登录成功（Argon2id 常量时间比对通过）
        var ok = await controller.EmailLogin(new LoginTenantGameUserEntityRequest
        {
            UserName = "player@example.test",
            Password = "right-password",
        });
        Assert.Equal(0, ClientApiResultReader.Code(ClientApiResultReader.Read(ok)));

        // 错误密码 -> 账号或密码错误
        var bad = await controller.EmailLogin(new LoginTenantGameUserEntityRequest
        {
            UserName = "player@example.test",
            Password = "wrong-password",
        });
        Assert.Equal("账号或密码错误", ClientApiResultReader.Message(ClientApiResultReader.Read(bad)));

        // 账号不存在 -> 统一返回账号或密码错误（执行假 Verify 抵御账号枚举，不泄露账号是否存在）
        var none = await controller.EmailLogin(new LoginTenantGameUserEntityRequest
        {
            UserName = "nobody@example.test",
            Password = "anything",
        });
        Assert.Equal("账号或密码错误", ClientApiResultReader.Message(ClientApiResultReader.Read(none)));
    }

    private static GameUserController CreateController(ClientApiTestDb db, ClientApiTokenIssuer? issuer = null)
    {
        db.InitTenantTables(typeof(TenantGameUserEntity), typeof(TenantGameUserEmailEntity), typeof(TenantSessionKeyEntity));
        return new GameUserController(
            db.Repository<TenantGameUserEntity>(),
            issuer ?? CreateTokenIssuer(),
            new ClientApiSessionKeyIssuer(db.Repository<TenantSessionKeyEntity>()),
            db.Repository<TenantGameUserEmailEntity>(),
            new FakeTenantGamePenaltyEnforcementService(),
            new TenantGameUserPasswordService());
    }

    private static ClientApiTokenIssuer CreateTokenIssuer()
    {
        return new ClientApiTokenIssuer(FakeOptionsProvider.WithTokenPolicy("unit-test-signing-key", 60));
    }
}