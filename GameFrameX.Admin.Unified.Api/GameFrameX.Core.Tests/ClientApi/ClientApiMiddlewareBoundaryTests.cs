using System.Text.Json;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Core.Middleware;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Enum;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiMiddlewareBoundaryTests : IDisposable
{
    private readonly List<string> _dbPaths = new();

    [Fact]
    public async Task TenantResolution_ValidTenantHeader_SetsTenantContext()
    {
        const long tenantId = 108001;
        var context = new DefaultHttpContext();
        context.Request.Headers[GlobalConst.TenantIdHeader] = tenantId.ToString();
        var middleware = new TenantResolutionMiddleware(nextContext =>
        {
            Assert.Equal(tenantId, nextContext.Items[TenantContextAccessor.TenantIdKey]);
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);
    }

    [Fact]
    public async Task TenantResolution_InvalidTenantHeader_DoesNotSetTenantContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[GlobalConst.TenantIdHeader] = "not-a-tenant";
        var middleware = new TenantResolutionMiddleware(nextContext =>
        {
            Assert.False(nextContext.Items.ContainsKey(TenantContextAccessor.TenantIdKey));
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);
    }

    [Fact]
    public async Task AppResolution_AppIdWithoutSecret_Returns400AndStopsPipeline()
    {
        var nextCalled = false;
        var context = CreateJsonResponseContext();
        context.Request.Headers[GlobalConst.AppIdHeader] = "216998926455275520";
        var middleware = new AppResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, null!);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("应用凭证不完整", await ReadMessageAsync(context));
    }

    [Fact]
    public async Task AppResolution_SecretWithoutAppId_Returns400AndStopsPipeline()
    {
        var nextCalled = false;
        var context = CreateJsonResponseContext();
        context.Request.Headers[GlobalConst.AppSecretHeader] = "secret";
        var middleware = new AppResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, null!);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("应用凭证不完整", await ReadMessageAsync(context));
    }

    [Fact]
    public async Task AppResolution_NoAppHeaders_PassesThroughWithoutAppContext()
    {
        var nextCalled = false;
        var context = CreateJsonResponseContext();
        var middleware = new AppResolutionMiddleware(nextContext =>
        {
            nextCalled = true;
            Assert.False(nextContext.Items.ContainsKey(AppResolutionMiddleware.AppIdKey));
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, null!);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task AppResolution_InvalidAppId_Returns401AndStopsPipeline()
    {
        var nextCalled = false;
        var context = CreateJsonResponseContext();
        context.Request.Headers[GlobalConst.AppIdHeader] = "not-a-number";
        context.Request.Headers[GlobalConst.AppSecretHeader] = "secret";
        var middleware = new AppResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, null!);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal("无效的应用 ID", await ReadMessageAsync(context));
    }

    [Fact]
    public async Task AppResolution_InvalidSecret_Returns401AndStopsPipeline()
    {
        using var fixture = CreateAppFixture(108101, 108201, "right-secret");
        var nextCalled = false;
        var context = CreateJsonResponseContext();
        context.Items[TenantContextAccessor.TenantIdKey] = fixture.TenantId;
        context.Request.Headers[GlobalConst.AppIdHeader] = fixture.AppId.ToString();
        context.Request.Headers[GlobalConst.AppSecretHeader] = "wrong-secret";
        var middleware = new AppResolutionMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, fixture.AppCache);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal("应用凭证无效", await ReadMessageAsync(context));
    }

    [Fact]
    public async Task AppResolution_DisabledApp_Returns401AndStopsPipeline()
    {
        using var fixture = CreateAppFixture(108102, 108202, "secret", false);
        var context = CreateJsonResponseContext();
        context.Items[TenantContextAccessor.TenantIdKey] = fixture.TenantId;
        context.Request.Headers[GlobalConst.AppIdHeader] = fixture.AppId.ToString();
        context.Request.Headers[GlobalConst.AppSecretHeader] = "secret";
        var middleware = new AppResolutionMiddleware(_ => throw new InvalidOperationException("不应进入下游"));

        await middleware.InvokeAsync(context, fixture.AppCache);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal("应用已被禁用", await ReadMessageAsync(context));
    }

    [Fact]
    public async Task AppResolution_AppTenantMismatch_Returns403AndDoesNotSetAppContext()
    {
        using var fixture = CreateAppFixture(108103, 108203, "secret");
        var context = CreateJsonResponseContext();
        context.Items[TenantContextAccessor.TenantIdKey] = 999999L;
        context.Request.Headers[GlobalConst.AppIdHeader] = fixture.AppId.ToString();
        context.Request.Headers[GlobalConst.AppSecretHeader] = "secret";
        var middleware = new AppResolutionMiddleware(_ => throw new InvalidOperationException("不应进入下游"));

        await middleware.InvokeAsync(context, fixture.AppCache);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.False(context.Items.ContainsKey(AppResolutionMiddleware.AppIdKey));
        Assert.Equal("应用不属于当前租户", await ReadMessageAsync(context));
    }

    public void Dispose()
    {
        foreach (var dbPath in _dbPaths)
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    private AppFixture CreateAppFixture(long tenantId, long appId, string appSecret, bool isEnabled = true)
    {
        var mainDb = CreateMainScope($"app_main_{tenantId}");
        var tenantDbPath = CreateDatabasePath($"app_tenant_{tenantId}");
        mainDb.CodeFirst.InitTables(typeof(SystemTenantEntity));
        mainDb.Insertable(new SystemTenantEntity
        {
            Id = tenantId,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.Sqlite,
            Connection = $"DataSource={tenantDbPath}",
            ConfigId = tenantId.ToString(),
            IsEnabled = true,
        }).ExecuteCommand();
        var tenantResolver = new DelegateSqlSugarTenantIdResolver(() => tenantId);
        var tenantDbProvider = new ClientApiTenantDbProvider(mainDb, tenantResolver);
        var tenantDb = tenantDbProvider.GetTenantDb(tenantId);
        tenantDb.CodeFirst.InitTables(typeof(TenantGameAppEntity));
        tenantDb.Insertable(new TenantGameAppEntity
        {
            Id = appId,
            TenantId = tenantId,
            Name = "C108 App",
            // C371（GFX-838）：明文比对分支已移除，验证仅走 hash 常量时间比对（AppSecret 列恒空）
            AppSecretHash = GameFrameX.Core.Security.AppSecretHasher.Hash(appSecret),
            AppSecretHashAlgorithm = GameFrameX.Core.Security.AppSecretHasher.Algorithm,
            IsEnabled = isEnabled,
            IsDeleted = false,
            UpdateTime = 1,
        }).ExecuteCommand();

        return new AppFixture(tenantId, appId, mainDb, new AppCacheService(tenantDbProvider, tenantResolver));
    }

    private SqlSugarScope CreateMainScope(string name)
    {
        var dbPath = CreateDatabasePath(name);
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            IsAutoCloseConnection = true,
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        return new SqlSugarScope(config);
    }

    private string CreateDatabasePath(string name)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_c108_{name}_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);
        return dbPath;
    }

    private static DefaultHttpContext CreateJsonResponseContext()
    {
        return new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream(),
            },
        };
    }

    private static async Task<string> ReadMessageAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("Message").GetString()!;
    }

    private sealed record AppFixture(long TenantId, long AppId, SqlSugarScope MainDb, AppCacheService AppCache) : IDisposable
    {
        public void Dispose()
        {
            MainDb.Dispose();
        }
    }
}