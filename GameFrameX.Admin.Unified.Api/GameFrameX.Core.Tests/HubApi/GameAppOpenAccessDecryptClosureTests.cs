using System.Text;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Business;
using GameFrameX.Core.SignatureAuth;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Business;
using GameFrameX.Entity.Business.Game.App;
using GameFrameX.Hub.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace GameFrameX.Core.Tests.HubApi;

// C372（GFX-838）T3/T5/T6：Hub.Api 开放访问中间件凭证解密收口——密文凭证（C371 起库内形态）
// 经完整 InvokeAsync 管线验签通过；密文存在而密钥缺失 fail-closed 抛错。
// change：gfx-doc/dev/client-api/changes/C372-runtime-credential-decrypt-closure/change.md
public sealed class GameAppOpenAccessDecryptClosureTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();
    private readonly string? _originalKey;

    public GameAppOpenAccessDecryptClosureTests()
    {
        _originalKey = Environment.GetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        _db.InitBusinessTables(typeof(GameAppOpenAccessEntity), typeof(UserEntity));
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, _originalKey);
        _db.Dispose();
    }

    [Fact]
    public async Task EncryptedCredential_SignedRequest_PassesFullPipeline()
    {
        SeedCredential(SecretCrypto.Encrypt("hub-open-access-secret")!);
        var httpContext = CreateSignedRequest("hub-open-access-secret");
        var nextCalled = false;

        var middleware = new GameAppOpenAccessAuthMiddleware(context =>
        {
            nextCalled = true;
            context.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(httpContext, CreateCacheService());

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task EncryptedCredential_MissingKey_FailsClosed()
    {
        SeedCredential(SecretCrypto.Encrypt("hub-open-access-secret")!);
        var httpContext = CreateSignedRequest("hub-open-access-secret");
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, null);

        var middleware = new GameAppOpenAccessAuthMiddleware(_ => Task.CompletedTask);

        // 密文存在而密钥缺失：fail-closed 抛 InvalidOperationException（指向环境变量，不回显密文）
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => middleware.InvokeAsync(httpContext, CreateCacheService()));
        Assert.Contains(SecretCrypto.KeyEnvironmentVariable, exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("disabled", 401)]
    [InlineData("revoked", 401)]
    [InlineData("expired", 401)]
    [InlineData("scope", 403)]
    [InlineData("user", 401)]
    [InlineData("tampered", 401)]
    public async Task MainDatabaseCredential_InvalidStateOrRequest_IsRejected(string scenario, int expectedStatus)
    {
        SeedCredential(SecretCrypto.Encrypt("hub-open-access-secret")!);
        var credential = _db.MainDb.Queryable<GameAppOpenAccessEntity>().First();
        switch (scenario)
        {
            case "disabled":
                credential.IsEnabled = false;
                break;
            case "revoked":
                credential.RevokedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                break;
            case "expired":
                credential.ExpiresTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                break;
            case "scope":
                credential.ScopesJson = "[\"FeatureSwitchSync\"]";
                break;
            case "user":
                credential.BindUserId = 9999;
                break;
        }

        _db.MainDb.Updateable(credential).ExecuteCommand();
        var context = CreateSignedRequest("hub-open-access-secret");
        if (scenario == "tampered")
        {
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"tampered\":true}"));
        }

        var nextCalled = false;
        var middleware = new GameAppOpenAccessAuthMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        await middleware.InvokeAsync(context, CreateCacheService());

        Assert.False(nextCalled);
        Assert.Equal(expectedStatus, context.Response.StatusCode);
    }

    [Fact]
    public async Task MainDatabaseCredential_ReplayIsRejectedAndOperatorHasNoTenantClaim()
    {
        SeedCredential(SecretCrypto.Encrypt("hub-open-access-secret")!);
        var first = CreateSignedRequest("hub-open-access-secret");
        var cache = CreateCacheService();
        var nextCount = 0;
        var middleware = new GameAppOpenAccessAuthMiddleware(context =>
        {
            nextCount++;
            Assert.Equal("7801", context.User.FindFirst(GameFrameX.Core.Const.ClaimConst.UserId)?.Value);
            Assert.Null(context.User.FindFirst("TenantId"));
            return Task.CompletedTask;
        });
        await middleware.InvokeAsync(first, cache);
        var replay = CreateSignedRequest("hub-open-access-secret");
        replay.Request.Headers["Signature"] = first.Request.Headers["Signature"];
        await middleware.InvokeAsync(replay, cache);

        Assert.Equal(1, nextCount);
        Assert.Equal(StatusCodes.Status401Unauthorized, replay.Response.StatusCode);
    }

    private void SeedCredential(string storedAccessSecret)
    {
        var bindUser = new UserEntity
        {
            Id = 7801, Account = "ci-user", RealName = "CI", Password = "not-a-real-password",
        };
        UserService.NormalizeNewUser(bindUser); // 补齐 NOT NULL 字符串列默认值
        _db.MainDb.Insertable(bindUser).ExecuteCommand();
        _db.MainDb.Insertable(new GameAppOpenAccessEntity
        {
            Id = 7802, BindUserId = 7801,
            Scenario = "ci", Purpose = "unit-test", SubjectName = "unit-test",
            ScopesJson = "[\"PackageBuildReport\"]",
            AccessKey = "ak-c372-vector", AccessSecret = storedAccessSecret,
        }).ExecuteCommand();
    }

    /// <summary>
    /// 构造按 C267 规范签名的开放访问请求（v2 单头 + body 入签 + canonical query 为空），
    /// 客户端以<b>明文</b> secret 计算 HMAC——中间件必须先解密库内密文才能验签通过。
    /// </summary>
    private DefaultHttpContext CreateSignedRequest(string plaintextSecret)
    {
        var body = Encoding.UTF8.GetBytes("{\"package\":\"pkg-1001\",\"status\":\"ok\"}");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nonce = "n-c372-" + Guid.NewGuid().ToString("N")[..8];
        var message = CanonicalRequestBuilder.BuildV2Message("POST", "/api/PackageBuildReport", string.Empty, "ak-c372-vector", timestamp, nonce, body);
        var signature = CanonicalRequestBuilder.HmacSha256(Encoding.UTF8.GetBytes(plaintextSecret), message);

        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Request.Method = "POST";
        httpContext.Request.Path = "/api/PackageBuildReport";
        httpContext.Request.Body = new MemoryStream(body);
        httpContext.Request.ContentLength = body.Length;
        httpContext.Request.Headers["Signature"] = "v2;ak-c372-vector;" + timestamp + ";" + nonce + ";" + Convert.ToBase64String(signature);
        httpContext.RequestServices = CreateRequestServices();
        return httpContext;
    }

    private IServiceProvider CreateRequestServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISqlSugarClient>(_db.MainDb);
        services.AddSingleton<ISqlSugarClient>(_db.DbProvider);
        services.AddScoped(typeof(Repository<>));
        return services.BuildServiceProvider();
    }

    private static SystemCacheService CreateCacheService()
    {
        return new SystemCacheService(new MemoryCacheService(), Options.Create(new CacheOptions()));
    }
}
