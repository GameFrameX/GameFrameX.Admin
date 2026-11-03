using GameFrameX.Client.Api.Services.PublicCallback;
using System.Text.Json;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Core.Middleware;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Core.Const;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Payment.AliPay;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class PublicCallbackTenantRoutingTests : IDisposable
{
    private readonly string _mainDatabase = CreateDatabaseName("main");
    private readonly string _tenantDatabase = CreateDatabaseName("tenant");
    private readonly List<SqlSugarScope> _scopes = new();
    private bool _postgresAvailable;

    [PostgreSqlFact]
    public async Task Middleware_WithValidRouteParameters_EstablishesCallbackContextBeforeNext()
    {
        const long tenantId = 1900000000001;
        const long appId = 1900000000101;
        const long packageId = 1900000000201;
        using var fixture = CreatePostgresFixture(tenantId, appId, packageId);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/GameOrder/NotifyAliPay";
        SetPublicCallbackEndpoint(httpContext, "alipay", "payment");
        fixture.SetCurrentContext(httpContext);
        httpContext.Request.RouteValues["tenantId"] = tenantId;
        httpContext.Request.RouteValues["appId"] = appId;
        httpContext.Request.RouteValues["packageId"] = packageId;
        var nextCalled = false;
        var middleware = new PublicCallbackTenantResolutionMiddleware(context =>
        {
            nextCalled = true;
            Assert.False(context.Items.ContainsKey(TenantContextAccessor.TenantIdKey));
            Assert.IsType<PublicCallbackRouteContext>(context.Items[PublicCallbackRouteContext.ItemKey]);
            Assert.Equal(packageId, context.Items[PaymentOptionSettingProvider.PackageIdKey]);
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(httpContext, fixture.RouteResolver);

        Assert.True(nextCalled);
        Assert.Equal(200, httpContext.Response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Middleware_MissingRouteParameters_FailsClosedBeforeNext()
    {
        const long tenantId = 1900000001001;
        const long appId = 1900000001101;
        using var fixture = CreatePostgresFixture(tenantId, appId, null);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/GameOrder/NotifyAliPay";
        SetPublicCallbackEndpoint(httpContext, "alipay", "payment");
        httpContext.Request.RouteValues["tenantId"] = tenantId;
        var middleware = new PublicCallbackTenantResolutionMiddleware(_ => throw new InvalidOperationException("不应进入下游控制器"));

        await middleware.InvokeAsync(httpContext, fixture.RouteResolver);

        Assert.Equal(400, httpContext.Response.StatusCode);
        Assert.False(httpContext.Items.ContainsKey(TenantContextAccessor.TenantIdKey));
    }

    [PostgreSqlFact]
    public void PaymentProvider_IgnoresBareQueryTenantId()
    {
        const long tenantId = 1900000002001;
        const long appId = 1900000002101;
        using var fixture = CreatePostgresFixture(tenantId, appId, null);
        InsertAliPayConfig(fixture.TenantDb, tenantId, appId, false);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?tenantId={tenantId}&appId={appId}&env=prod");
        var provider = new PaymentOptionSettingProvider(fixture.TenantDbProvider, new HttpContextAccessor { HttpContext = httpContext, });

        var settings = provider.GetOptionSettings("alipay").ToList();

        Assert.Empty(settings);
    }

    [PostgreSqlFact]
    public void PaymentProvider_RejectsOutboundAliPayUntilCallbackRouteUpgrade()
    {
        const long tenantId = 1900000003001;
        const long appId = 1900000003101;
        const long packageId = 1900000003201;
        using var fixture = CreatePostgresFixture(tenantId, appId, packageId);
        InsertAliPayConfig(fixture.TenantDb, tenantId, appId, true);
        var httpContext = new DefaultHttpContext();
        httpContext.Items[TenantContextAccessor.TenantIdKey] = tenantId;
        httpContext.Items[PaymentOptionSettingProvider.PackageIdKey] = packageId;
        httpContext.Items[PaymentOptionSettingProvider.PaymentIsSandboxKey] = true;
        var provider = new PaymentOptionSettingProvider(fixture.TenantDbProvider, new HttpContextAccessor { HttpContext = httpContext, });

        var error = Assert.Throws<InvalidOperationException>(() => provider.GetOptionSetting<AliPayOptionSetting>("alipay"));
        Assert.Contains("完整支付回调路由", error.Message);
    }

    [PostgreSqlFact]
    public async Task AdvertisementOptionsProvider_PicksPangleConfigFromPublicCallbackContext()
    {
        const long tenantId = 1900000004001;
        const long appId = 1900000004101;
        using var fixture = CreatePostgresFixture(tenantId, appId, null);
        fixture.TenantDb.Insertable(new[]
        {
            new TenantAdvertisementCallbackConfigEntity
            {
                Id = 1900000004201,
                TenantId = tenantId,
                AppId = appId,
                Provider = "TopOn",
                ConfigJson = "{\"callbackUrl\":\"https://client.example.com/topon\",\"signingSecret\":\"topon-secret\"}",
                IsEnabled = true,
                IsDeleted = false,
            },
            new TenantAdvertisementCallbackConfigEntity
            {
                Id = 1900000004202,
                TenantId = tenantId,
                AppId = appId,
                Provider = "Pangle",
                ConfigJson = "{\"callbackUrl\":\"https://client.example.com/pangle\",\"signingSecret\":\"pangle-secret\"}",
                IsEnabled = true,
                IsDeleted = false,
            },
        }).ExecuteCommand();
        var httpContext = new DefaultHttpContext();
        httpContext.Items[TenantContextAccessor.TenantIdKey] = tenantId;
        httpContext.Items[PublicCallbackRouteContext.ItemKey] = new PublicCallbackRouteContext("pangle", "ad", null);
        var provider = new ClientApiTenantOptionsProvider(fixture.TenantDbProvider, new HttpContextAccessor { HttpContext = httpContext, });

        var options = await provider.GetAdvertisementCallbackOptionsAsync();

        Assert.NotNull(options);
        Assert.Equal("Pangle", options.Provider);
        Assert.Equal("pangle-secret", options.PangleSecurityKey);
    }

    public void Dispose()
    {
        foreach (var scope in _scopes)
        {
            scope.Dispose();
        }

        if (!_postgresAvailable)
        {
            return;
        }

        DropDatabase(_tenantDatabase);
        DropDatabase(_mainDatabase);
    }

    private PostgresFixture CreatePostgresFixture(long tenantId, long appId, long? packageId)
    {
        EnsurePostgresAvailable();
        CreateDatabase(_mainDatabase);
        CreateDatabase(_tenantDatabase);
        var mainDb = CreateScope(_mainDatabase);
        var tenantDb = CreateScope(_tenantDatabase);
        _scopes.Add(mainDb);
        _scopes.Add(tenantDb);
        mainDb.CodeFirst.InitTables(typeof(SystemTenantEntity));
        tenantDb.CodeFirst.InitTables(typeof(TenantGameAppPackageEntity), typeof(TenantGamePaymentConfigEntity), typeof(TenantAdvertisementCallbackConfigEntity));
        var tenantConnection = BuildConnectionString(_tenantDatabase);
        mainDb.Insertable(new SystemTenantEntity
        {
            Id = tenantId,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.PostgreSQL,
            Connection = tenantConnection,
            ConfigId = tenantId.ToString(),
            IsEnabled = true,
        }).ExecuteCommand();
        if (packageId.HasValue)
        {
            tenantDb.Insertable(new TenantGameAppPackageEntity
            {
                Id = packageId.Value,
                TenantId = tenantId,
                GameAppId = appId,
                Name = "C59 Test Package",
                PackageName = "com.gameframex.c59",
                PlatformCode = "ios",
                PlatformName = "iOS",
                ChannelCode = "default",
                ChannelName = "Default",
                IsEnabled = true,
                IsDeleted = false,
            }).ExecuteCommand();
        }

        var currentContext = new DefaultHttpContext();
        var resolver = new DelegateSqlSugarTenantIdResolver(() =>
                                                                currentContext.Items.TryGetValue(TenantContextAccessor.TenantIdKey, out var value) &&
                                                                long.TryParse(value?.ToString(), out var parsed)
                                                                    ? parsed
                                                                    : null);
        var tenantDbProvider = mainDb;
        var routeResolver = new PublicCallbackRouteResolver(tenantDb);
        return new PostgresFixture(mainDb, tenantDb, tenantDbProvider, routeResolver, context => currentContext = context);
    }

    private static void SetPublicCallbackEndpoint(DefaultHttpContext context, string provider, string callbackKind)
    {
        var metadata = new EndpointMetadataCollection(new PublicCallbackEndpointAttribute(provider, callbackKind));
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, metadata, $"{provider}-{callbackKind}"));
    }

    private static void InsertAliPayConfig(ISqlSugarClient tenantDb, long tenantId, long appId, bool isSandbox)
    {
        var configJson = JsonSerializer.Serialize(new AliPayOptionSetting
        {
            AliPayAppId = "2026000000000000",
            AliPayMerchantPrivateKey = "private-key",
            AliPayPublicKey = "public-key",
            AliPayNotifyUrl = "https://client.example.com/api/GameOrder/NotifyAliPay",
            AliPayReturnUrl = "https://client.example.com/pay/return",
        });
        tenantDb.Insertable(new TenantGamePaymentConfigEntity
        {
            Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            TenantId = tenantId,
            AppId = appId,
            GameAppId = appId,
            GameAppIdNormalized = appId,
            PackageIdNormalized = 0,
            PayChannel = "alipay",
            IsSandbox = isSandbox,
            ConfigJson = configJson,
            IsEnabled = true,
            IsDeleted = false,
        }).ExecuteCommand();
    }

    private SqlSugarScope CreateScope(string databaseName)
    {
        var config = new ConnectionConfig
        {
            ConfigId = databaseName,
            DbType = DbType.PostgreSQL,
            ConnectionString = BuildConnectionString(databaseName),
            IsAutoCloseConnection = true,
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        return new SqlSugarScope(config);
    }

    private void EnsurePostgresAvailable()
    {
        if (_postgresAvailable)
        {
            return;
        }

        try
        {
            using var scope = CreateAdminScope();
            scope.Ado.ExecuteCommand("SELECT 1");
            _postgresAvailable = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(PostgreSqlFactAttribute.SkipReason, ex);
        }
    }

    private static void CreateDatabase(string databaseName)
    {
        using var scope = CreateAdminScope();
        scope.Ado.ExecuteCommand($"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)");
        scope.Ado.ExecuteCommand($"CREATE DATABASE \"{databaseName}\"");
    }

    private static void DropDatabase(string databaseName)
    {
        using var scope = CreateAdminScope();
        scope.Ado.ExecuteCommand($"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE)");
    }

    private static SqlSugarScope CreateAdminScope()
    {
        var config = new ConnectionConfig
        {
            ConfigId = "postgres_admin",
            DbType = DbType.PostgreSQL,
            ConnectionString = BuildConnectionString("postgres"),
            IsAutoCloseConnection = true,
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        return new SqlSugarScope(config);
    }

    private static string BuildConnectionString(string databaseName)
    {
        return new Npgsql.NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("GFX_TEST_PG_CONNECTION_STRING")) { Database = databaseName }.ConnectionString;
    }

    private static string CreateDatabaseName(string suffix)
    {
        return $"gfx_c59_{suffix}_{Guid.NewGuid():N}".ToLowerInvariant();
    }

    private sealed class PostgreSqlFactAttribute : FactAttribute
    {
        internal const string SkipReason = "PostgreSQL integration tests require an explicit GFX_TEST_PG_CONNECTION_STRING.";
        private static readonly Lazy<bool> IsPostgreSqlAvailable = new(CheckPostgreSqlAvailable);

        public PostgreSqlFactAttribute()
        {
            if (!IsPostgreSqlAvailable.Value)
            {
                Skip = SkipReason;
            }
        }

        private static bool CheckPostgreSqlAvailable()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GFX_TEST_PG_CONNECTION_STRING")))
            {
                return false;
            }

            try
            {
                using var scope = CreateAdminScope();
                scope.Ado.ExecuteCommand("SELECT 1");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    private sealed class PostgresFixture : IDisposable
    {
        public PostgresFixture(
            ISqlSugarClient mainDb,
            ISqlSugarClient tenantDb,
            ISqlSugarClient tenantDbProvider,
            PublicCallbackRouteResolver routeResolver,
            Action<DefaultHttpContext> setCurrentContext)
        {
            MainDb = mainDb;
            TenantDb = tenantDb;
            TenantDbProvider = tenantDbProvider;
            RouteResolver = routeResolver;
            SetCurrentContext = setCurrentContext;
        }

        public ISqlSugarClient MainDb { get; }

        public ISqlSugarClient TenantDb { get; }

        public ISqlSugarClient TenantDbProvider { get; }

        public PublicCallbackRouteResolver RouteResolver { get; }

        public Action<DefaultHttpContext> SetCurrentContext { get; }

        public void Dispose()
        {
        }
    }
}