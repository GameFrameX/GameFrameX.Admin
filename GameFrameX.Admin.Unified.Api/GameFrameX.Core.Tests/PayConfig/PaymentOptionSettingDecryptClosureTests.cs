using System.Reflection;
using GameFrameX.Client.Api.Integrations.Payment;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Payment.AliPay;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.PayConfig;

// C372（GFX-838）T2/T5：支付配置读取链路解密收口——密文 ConfigJson（C371 起库内形态）
// 经 PaymentOptionSettingProvider 反序列化前字段级解密，Option 拿到明文商户私钥。
// change：gfx-doc/dev/client-api/changes/C372-runtime-credential-decrypt-closure/change.md
// PG 真库回归：作用域 Where 的 Nullable.HasValue 闭包曾使 SqlSugar 生成非法 SQL
// （SQLite 报 near "IS"、PG 报 near "NOT"，真机冒烟发现 PG 同样失败），已提为局部布尔修复；
// 与 PublicCallbackTenantRoutingTests 同款 PG fixture。SQLite 侧等价回归见 RuntimeCredentialDecryptClosureTests。
public sealed class PaymentOptionSettingDecryptClosureTests
{
    [PostgreSqlFact]
    public void GetOptionSetting_SensitiveJsonFieldDecrypts()
    {
        const long tenantId = 1900000004001;
        const long appId = 1900000004101;
        var originalKey = Environment.GetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
        _ = Assembly.Load("GameFrameX.Payment.AliPay");
        try
        {
            using var fixture = new PostgresPaymentFixture(tenantId, appId);
            var configJson = PaymentConfigCrypto.EncryptConfigJson("alipay",
                "{\"AliPayAppId\":\"2021001\",\"AliPayMerchantPrivateKey\":\"plain-merchant-key\",\"AliPayGatewayUrl\":\"https://openapi.alipay.com/gateway.do\"}");
            fixture.TenantDb.Insertable(new TenantGamePaymentConfigEntity
            {
                Id = 1900000004201, TenantId = tenantId, AppId = appId, GameAppIdNormalized = appId, PayChannel = "alipay", ConfigJson = configJson,
            }).ExecuteCommand();

            var httpContext = new DefaultHttpContext();
            httpContext.Items[TenantContextAccessor.TenantIdKey] = tenantId;
            httpContext.Items[AppResolutionMiddleware.AppIdKey] = appId;
            var provider = new PaymentOptionSettingProvider(fixture.TenantDbProvider, new HttpContextAccessor { HttpContext = httpContext, });

            var setting = provider.GetOptionSetting<AliPayOptionSetting>("alipay");

            Assert.NotNull(setting);
            Assert.Equal("2021001", setting.AliPayAppId);
            Assert.Equal("plain-merchant-key", setting.AliPayMerchantPrivateKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable(TenantSecretCrypto.KeyEnvironmentVariable, originalKey);
        }
    }

    /// <summary>
    /// 独立 PG 测试库 fixture（镜像 PublicCallbackTenantRoutingTests 私有实现，不跨测试类共享状态）：
    /// 建库 → 主库注册租户行 → 租户库建租户表，Dispose 整库删除。
    /// </summary>
    private sealed class PostgresPaymentFixture : IDisposable
    {
        private readonly string _databaseName;

        public PostgresPaymentFixture(long tenantId, long appId)
        {
            _databaseName = ("gfx_c372_pay_" + Guid.NewGuid().ToString("N")).ToLowerInvariant();
            ExecuteAdminCommand($"CREATE DATABASE \"{_databaseName}\"");

            var mainConfig = CreateConfig("postgres", _databaseName);
            MainDb = new SqlSugarScope(mainConfig);
            MainDb.CodeFirst.InitTables(typeof(SystemTenantEntity));
            MainDb.Insertable(new SystemTenantEntity
            {
                Id = tenantId,
                TenantType = TenantTypeEnum.Db,
                DbType = DbType.PostgreSQL,
                Connection = BuildConnectionString(_databaseName),
                ConfigId = tenantId.ToString(),
                IsEnabled = true,
            }).ExecuteCommand();

            TenantIdResolver = new DelegateSqlSugarTenantIdResolver(() => tenantId);
            TenantDbProvider = new ClientApiTenantDbProvider(MainDb, TenantIdResolver);
            TenantDb = TenantDbProvider.GetTenantDb(tenantId);
            TenantDb.CodeFirst.InitTables(typeof(TenantGamePaymentConfigEntity));
            _ = appId;
        }

        public SqlSugarScope MainDb { get; }

        public ISqlSugarClient TenantDb { get; }

        public ClientApiTenantDbProvider TenantDbProvider { get; }

        public ISqlSugarTenantIdResolver TenantIdResolver { get; }

        public void Dispose()
        {
            MainDb.Dispose();
            ExecuteAdminCommand($"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)");
        }

        private static void ExecuteAdminCommand(string sql)
        {
            using var scope = new SqlSugarScope(CreateConfig("postgres", "postgres"));
            scope.Ado.ExecuteCommand(sql);
        }

        private static ConnectionConfig CreateConfig(string configId, string databaseName)
        {
            var config = new ConnectionConfig
            {
                ConfigId = configId,
                DbType = DbType.PostgreSQL,
                ConnectionString = BuildConnectionString(databaseName),
                IsAutoCloseConnection = true,
            };
            SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
            return config;
        }

        private static string BuildConnectionString(string databaseName)
        {
            return $"Host=127.0.0.1;Port=5432;Database={databaseName};Username=postgres;Password=postgres;Maximum Pool Size=20";
        }
    }

    /// <summary>
    /// 本地 PG（127.0.0.1:5432 postgres/postgres）不可用时跳过，语义同 PublicCallbackTenantRoutingTests.PostgreSqlFactAttribute。
    /// </summary>
    private sealed class PostgreSqlFactAttribute : FactAttribute
    {
        internal const string SkipReason = "PostgreSQL integration tests require local PostgreSQL at 127.0.0.1:5432 with postgres/postgres.";

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
            try
            {
                using var scope = new SqlSugarScope(new ConnectionConfig
                {
                    ConfigId = "postgres_admin",
                    DbType = DbType.PostgreSQL,
                    ConnectionString = "Host=127.0.0.1;Port=5432;Database=postgres;Username=postgres;Password=postgres",
                    IsAutoCloseConnection = true,
                });
                scope.Ado.ExecuteCommand("SELECT 1");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
