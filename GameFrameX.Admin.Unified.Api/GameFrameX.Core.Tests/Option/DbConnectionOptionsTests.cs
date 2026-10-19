using GameFrameX.Core.Option;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Npgsql;
using SqlSugar;

namespace GameFrameX.Core.Tests.Option;

public class DbConnectionOptionsTests
{
    [Fact]
    public void PostConfigure_AppliesDatabaseConnectionEnvironmentOverrides()
    {
        const string mainConnection = "Host=main-env;Port=5432;Database=main;Username=u;Password=p;";
        const string tenantConnection = "Host=tenant-env;Port=5432;Database=tenant;Username=u;Password=p;";

        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_CONNECTION_STRING"] = mainConnection,
            ["GFX_DEFAULT_TENANT_DB_CONNECTION_STRING"] = tenantConnection,
        }, () =>
        {
            var options = CreateOptions();

            options.PostConfigure(options, new ConfigurationBuilder().Build());

            Assert.Equal(mainConnection, options.ConnectionConfigs[0].ConnectionString);
            Assert.Equal(tenantConnection, options.ConnectionConfigs[1].ConnectionString);
            // C156：标准部署不再要求独立日志库，GFX_LOG_DB_CONNECTION_STRING 已移除；索引 2 保留 JSON 原值。
            Assert.Equal("Host=json-log;Port=5432;Database=log;Username=u;Password=p;", options.ConnectionConfigs[2].ConnectionString);
        });
    }

    [Fact]
    public void PostConfigure_AppliesIndexedDatabaseConnectionEnvironmentOverrides()
    {
        const string mainConnection = "Host=index-main-env;Port=5432;Database=main;Username=u;Password=p;";
        const string tenantConnection = "Host=index-tenant-env;Port=5432;Database=tenant;Username=u;Password=p;";
        const string logConnection = "Host=index-log-env;Port=5432;Database=log;Username=u;Password=p;";

        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["DbConnection__ConnectionConfigs__0__ConnectionString"] = mainConnection,
            ["DbConnection__ConnectionConfigs__1__ConnectionString"] = tenantConnection,
            ["DbConnection__ConnectionConfigs__2__ConnectionString"] = logConnection,
        }, () =>
        {
            var options = CreateOptions();

            options.PostConfigure(options, new ConfigurationBuilder().Build());

            Assert.Equal(mainConnection, options.ConnectionConfigs[0].ConnectionString);
            Assert.Equal(tenantConnection, options.ConnectionConfigs[1].ConnectionString);
            Assert.Equal(logConnection, options.ConnectionConfigs[2].ConnectionString);
        });
    }

    [Fact]
    public void PostConfigure_AppliesDatabaseTypeAndConnectionStringAsPair()
    {
        const string mainConnection = "Host=postgres;Port=5432;Database=main;Username=u;Password=p;";

        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = "PostgreSQL",
            ["GFX_MAIN_DB_CONNECTION_STRING"] = mainConnection,
        }, () =>
        {
            var options = CreateTypedOptions(DbType.Sqlite, DbType.Sqlite);

            options.PostConfigure(options, new ConfigurationBuilder().Build());

            Assert.Equal(DbType.PostgreSQL, options.ConnectionConfigs[0].DbType);
            Assert.Equal(mainConnection, options.ConnectionConfigs[0].ConnectionString);
        });
    }

    [Fact]
    public void PostConfigure_ThrowsWhenSqliteProviderGetsHostStyleConnectionString()
    {
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            // 只覆盖连接串不覆盖类型：Sqlite provider + pgsql 串，必须启动期暴露
            ["GFX_MAIN_DB_CONNECTION_STRING"] = "Host=postgres;Port=5432;Database=main;Username=u;Password=p;",
        }, () =>
        {
            var options = CreateTypedOptions(DbType.Sqlite, DbType.Sqlite);

            Assert.Throws<InvalidOperationException>(() => options.PostConfigure(options, new ConfigurationBuilder().Build()));
        });
    }

    [Fact]
    public void PostConfigure_ThrowsWhenPostgreSqlProviderGetsDataSourceStyleConnectionString()
    {
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = "PostgreSQL",
            // 半套覆盖：类型切了 PostgreSQL，连接串仍是 SQLite 的 DataSource= 风格
            ["GFX_MAIN_DB_CONNECTION_STRING"] = "DataSource=./db/GameFrameX.Admin.db",
        }, () =>
        {
            var options = CreateTypedOptions(DbType.PostgreSQL, DbType.PostgreSQL);

            Assert.Throws<InvalidOperationException>(() => options.PostConfigure(options, new ConfigurationBuilder().Build()));
        });
    }

    [Fact]
    public void PostConfigure_DerivesDefaultTenantDatabaseFromPostgreSqlMain()
    {
        var options = new DbConnectionOptions
        {
            ConnectionConfigs = new List<DbConnectionConfig>
            {
                new()
                {
                    ConfigId = "main",
                    DbType = DbType.PostgreSQL,
                    ConnectionString = "Host=pg-main;Port=5432;Database=gameframex_admin;Username=admin;Password=secret;",
                },
            },
        };

        options.PostConfigure(options, new ConfigurationBuilder().Build());

        // 推导追加为第二项：库名 = 普通租户同款「前缀+租户ID」规律，Host/Port/账号/密码全继承主库
        Assert.Equal(2, options.ConnectionConfigs.Count);
        var derived = options.ConnectionConfigs[1];
        Assert.Equal("1300000000001", derived.ConfigId?.ToString());
        Assert.Equal(DbType.PostgreSQL, derived.DbType);
        var builder = new NpgsqlConnectionStringBuilder(derived.ConnectionString);
        Assert.Equal("gameframex_admin_tenant_1300000000001", builder.Database);
        Assert.Equal("pg-main", builder.Host);
        Assert.Equal("admin", builder.Username);
        // 推导项开关与历史文件第二项等价：建库幂等交给专用管线，建表/种子全关，DiffLog 强制关闭
        Assert.True(derived.DbSettings.EnableInitDb);
        Assert.False(derived.DbSettings.EnableDiffLog);
        Assert.False(derived.TableSettings.EnableInitTable);
        Assert.False(derived.TableSettings.EnableIncreTable);
        Assert.False(derived.SeedSettings.EnableInitSeed);
        Assert.False(derived.SeedSettings.EnableIncreSeed);
    }

    [Fact]
    public void PostConfigure_DerivesDefaultTenantDatabaseFromSqliteMain()
    {
        var options = new DbConnectionOptions
        {
            ConnectionConfigs = new List<DbConnectionConfig>
            {
                new()
                {
                    ConfigId = "main",
                    DbType = DbType.Sqlite,
                    ConnectionString = "DataSource=./db/GameFrameX.Admin.db;",
                },
            },
        };

        options.PostConfigure(options, new ConfigurationBuilder().Build());

        Assert.Equal(2, options.ConnectionConfigs.Count);
        Assert.Equal("1300000000001", options.ConnectionConfigs[1].ConfigId?.ToString());
        Assert.Equal(DbType.Sqlite, options.ConnectionConfigs[1].DbType);
        Assert.Equal("DataSource=./db/gameframex_admin_tenant_1300000000001.db;", options.ConnectionConfigs[1].ConnectionString);
    }

    [Fact]
    public void PostConfigure_SkipsDerivationWhenTenantConfiguredInFile()
    {
        const string explicitTenantConnection = "Host=explicit-tenant;Port=5432;Database=tenant;Username=u;Password=p;";
        var options = new DbConnectionOptions
        {
            ConnectionConfigs = new List<DbConnectionConfig>
            {
                new()
                {
                    ConfigId = "main",
                    DbType = DbType.PostgreSQL,
                    ConnectionString = "Host=pg-main;Port=5432;Database=gameframex_admin;Username=u;Password=p;",
                },
                new()
                {
                    ConfigId = "1300000000001",
                    DbType = DbType.PostgreSQL,
                    ConnectionString = explicitTenantConnection,
                },
            },
        };

        options.PostConfigure(options, new ConfigurationBuilder().Build());

        Assert.Equal(2, options.ConnectionConfigs.Count);
        Assert.Equal(explicitTenantConnection, options.ConnectionConfigs[1].ConnectionString);
    }

    [Fact]
    public void PostConfigure_CreatesDefaultTenantConfigFromEnvironmentPairWhenFileOmitted()
    {
        const string envTenantConnection = "Host=env-tenant;Port=5432;Database=tenant;Username=u;Password=p;";
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_DEFAULT_TENANT_DB_TYPE"] = "PostgreSQL",
            ["GFX_DEFAULT_TENANT_DB_CONNECTION_STRING"] = envTenantConnection,
        }, () =>
        {
            var options = new DbConnectionOptions
            {
                ConnectionConfigs = new List<DbConnectionConfig>
                {
                    new()
                    {
                        ConfigId = "main",
                        DbType = DbType.PostgreSQL,
                        ConnectionString = "Host=pg-main;Port=5432;Database=gameframex_admin;Username=u;Password=p;",
                    },
                },
            };

            options.PostConfigure(options, new ConfigurationBuilder().Build());

            // 成对显式环境变量 + 文件省略该项：跳过推导但由环境变量对补建，不再落成种子期「连接配置缺失」启动崩溃
            Assert.Equal(2, options.ConnectionConfigs.Count);
            var created = options.ConnectionConfigs[1];
            Assert.Equal("1300000000001", created.ConfigId?.ToString());
            Assert.Equal(DbType.PostgreSQL, created.DbType);
            Assert.Equal(envTenantConnection, created.ConnectionString);
            // 补建项开关与推导项等价：建库幂等交给专用管线，建表/种子全关，DiffLog 强制关闭
            Assert.True(created.DbSettings.EnableInitDb);
            Assert.False(created.DbSettings.EnableDiffLog);
            Assert.False(created.TableSettings.EnableInitTable);
            Assert.False(created.TableSettings.EnableIncreTable);
            Assert.False(created.SeedSettings.EnableInitSeed);
            Assert.False(created.SeedSettings.EnableIncreSeed);
        });
    }

    [Theory]
    [InlineData("GFX_DEFAULT_TENANT_DB_TYPE")]
    [InlineData("GFX_DEFAULT_TENANT_DB_CONNECTION_STRING")]
    public void PostConfigure_ThrowsWhenDefaultTenantVariablesOnlyHalfSet(string halfSetName)
    {
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            [halfSetName] = "PostgreSQL",
        }, () =>
        {
            var options = new DbConnectionOptions
            {
                ConnectionConfigs = new List<DbConnectionConfig>
                {
                    new()
                    {
                        ConfigId = "main",
                        DbType = DbType.PostgreSQL,
                        ConnectionString = "Host=pg-main;Port=5432;Database=gameframex_admin;Username=u;Password=p;",
                    },
                },
            };

            var exception = Assert.Throws<InvalidOperationException>(() => options.PostConfigure(options, new ConfigurationBuilder().Build()));
            Assert.Contains("必须成对", exception.Message);
        });
    }

    [Fact]
    public void PostConfigure_DerivesDefaultTenantDatabaseFromMySqlConnectorMain()
    {
        var options = new DbConnectionOptions
        {
            ConnectionConfigs = new List<DbConnectionConfig>
            {
                new()
                {
                    ConfigId = "main",
                    DbType = DbType.MySqlConnector,
                    ConnectionString = "Server=mysql-main;Port=3307;Database=gameframex_admin;Uid=admin;Pwd=secret;Maximum Pool Size=50;SslMode=none",
                },
            },
        };

        options.PostConfigure(options, new ConfigurationBuilder().Build());

        // 推导追加为第二项：库名 = 普通租户同款「前缀+租户ID」规律，Server/Port/账号/密码/连接池等参数全继承主库
        Assert.Equal(2, options.ConnectionConfigs.Count);
        var derived = options.ConnectionConfigs[1];
        Assert.Equal("1300000000001", derived.ConfigId?.ToString());
        Assert.Equal(DbType.MySqlConnector, derived.DbType);
        var builder = new MySqlConnectionStringBuilder(derived.ConnectionString);
        Assert.Equal("gameframex_admin_tenant_1300000000001", builder.Database);
        Assert.Equal("mysql-main", builder.Server);
        Assert.Equal((uint)3307, builder.Port);
        Assert.Equal("admin", builder.UserID);
        Assert.Equal("secret", builder.Password);
        Assert.Equal((uint)50, builder.MaximumPoolSize);
        // 推导项开关与 PostgreSQL 测试同构：建库幂等交给专用管线，建表/种子全关，DiffLog 强制关闭
        Assert.True(derived.DbSettings.EnableInitDb);
        Assert.False(derived.DbSettings.EnableDiffLog);
        Assert.False(derived.TableSettings.EnableInitTable);
        Assert.False(derived.TableSettings.EnableIncreTable);
        Assert.False(derived.SeedSettings.EnableInitSeed);
        Assert.False(derived.SeedSettings.EnableIncreSeed);
    }

    [Fact]
    public void PostConfigure_DerivesDefaultTenantDatabaseFromMySqlMain()
    {
        // DbType.MySql 在 .NET 上驱动即 MySqlConnector 包（MySqlConnector 类型位于 MySql.Data.MySqlClient 命名空间），全链路可跑；
        // 推导是纯连接串改写（与 DbType.MySqlConnector 共用同一 builder 解析路径），本用例以小写键别名风格覆盖共享改写路径
        var options = new DbConnectionOptions
        {
            ConnectionConfigs = new List<DbConnectionConfig>
            {
                new()
                {
                    ConfigId = "main",
                    DbType = DbType.MySql,
                    ConnectionString = "server=mysql-main;port=3307;database=gameframex_admin;uid=admin;pwd=secret;",
                },
            },
        };

        options.PostConfigure(options, new ConfigurationBuilder().Build());

        Assert.Equal(2, options.ConnectionConfigs.Count);
        var derived = options.ConnectionConfigs[1];
        Assert.Equal("1300000000001", derived.ConfigId?.ToString());
        Assert.Equal(DbType.MySql, derived.DbType);
        var builder = new MySqlConnectionStringBuilder(derived.ConnectionString);
        Assert.Equal("gameframex_admin_tenant_1300000000001", builder.Database);
        Assert.Equal("mysql-main", builder.Server);
        Assert.Equal((uint)3307, builder.Port);
        Assert.Equal("admin", builder.UserID);
        Assert.Equal("secret", builder.Password);
    }

    [Fact]
    public void PostConfigure_ThrowsWhenMainDbTypeDoesNotSupportDerivation()
    {
        var options = new DbConnectionOptions
        {
            ConnectionConfigs = new List<DbConnectionConfig>
            {
                new()
                {
                    ConfigId = "main",
                    DbType = DbType.Odbc,
                    ConnectionString = "Driver={PostgreSQL};Server=odbc-main;Database=gameframex_admin;Uid=admin;Pwd=secret;",
                },
            },
        };

        var exception = Assert.Throws<InvalidOperationException>(() => options.PostConfigure(options, new ConfigurationBuilder().Build()));
        Assert.Contains("不支持自动推导", exception.Message);
        // 报错文案必须准确列出当前支持的 DbType 清单（GFX-686 验收标准 4）
        Assert.Contains("PostgreSQL / Sqlite / MySql / MySqlConnector", exception.Message);
    }

    [Fact]
    public void PostConfigure_CreatesMainConfigFromEnvironmentPairWhenListEmpty()
    {
        const string mainConnection = "Host=pair-main;Port=5432;Database=main;Username=u;Password=p;";
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = "PostgreSQL",
            ["GFX_MAIN_DB_CONNECTION_STRING"] = mainConnection,
        }, () =>
        {
            var options = new DbConnectionOptions
            {
                ConnectionConfigs = new List<DbConnectionConfig>(),
            };

            options.PostConfigure(options, new ConfigurationBuilder().Build());

            // 无配置文件的 host（Client/Hub）用成对 GFX_MAIN_DB_* 即可自建 main，并继续走推导
            Assert.Equal(2, options.ConnectionConfigs.Count);
            Assert.Equal("main", options.ConnectionConfigs[0].ConfigId?.ToString());
            Assert.Equal(DbType.PostgreSQL, options.ConnectionConfigs[0].DbType);
            Assert.Equal(mainConnection, options.ConnectionConfigs[0].ConnectionString);
            Assert.Equal("1300000000001", options.ConnectionConfigs[1].ConfigId?.ToString());
        });
    }

    [Fact]
    public void PostConfigure_DoesNotCreateMainConfigWhenOnlyConnectionStringOverridden()
    {
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            // 半套覆盖：只给连接串不给类型，不建 main 项，把错配留给装配点按「不成对」报错
            ["GFX_MAIN_DB_CONNECTION_STRING"] = "Host=half-set;Port=5432;Database=main;Username=u;Password=p;",
        }, () =>
        {
            var options = new DbConnectionOptions
            {
                ConnectionConfigs = new List<DbConnectionConfig>(),
            };

            options.PostConfigure(options, new ConfigurationBuilder().Build());

            Assert.Empty(options.ConnectionConfigs);
        });
    }

    private static DbConnectionOptions CreateOptions()
    {
        return new DbConnectionOptions
        {
            ConnectionConfigs = new List<DbConnectionConfig>
            {
                new()
                {
                    ConfigId = "main",
                    ConnectionString = "Host=json-main;Port=5432;Database=main;Username=u;Password=p;",
                },
                new()
                {
                    ConfigId = "1300000000001",
                    ConnectionString = "Host=json-tenant;Port=5432;Database=tenant;Username=u;Password=p;",
                },
                new()
                {
                    ConfigId = "1300000000002",
                    ConnectionString = "Host=json-log;Port=5432;Database=log;Username=u;Password=p;",
                },
            },
        };
    }

    private static DbConnectionOptions CreateTypedOptions(DbType mainType, DbType tenantType)
    {
        return new DbConnectionOptions
        {
            ConnectionConfigs = new List<DbConnectionConfig>
            {
                new()
                {
                    ConfigId = "main",
                    DbType = mainType,
                    ConnectionString = "DataSource=./db/GameFrameX.Admin.db",
                },
                new()
                {
                    ConfigId = "1300000000001",
                    DbType = tenantType,
                    ConnectionString = "DataSource=./db/gameframex_admin_tenant_1300000000001.db",
                },
            },
        };
    }

    private static void WithEnvironmentVariables(IReadOnlyDictionary<string, string?> values, Action action)
    {
        var originalValues = values.ToDictionary(item => item.Key, item => Environment.GetEnvironmentVariable(item.Key));
        try
        {
            foreach (var (key, value) in values)
            {
                Environment.SetEnvironmentVariable(key, value);
            }

            action();
        }
        finally
        {
            foreach (var (key, value) in originalValues)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}