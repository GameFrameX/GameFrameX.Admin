using GameFrameX.Core.Bootstrap;
using GameFrameX.Core.SqlSugar;
using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace GameFrameX.Core.Tests.Bootstrap;

public class HostMainDbConnectionResolverTests
{
    [Fact]
    public void Resolve_BindsDbConnectionSectionAndDerivesDefaultTenant()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DbConnection:ConnectionConfigs:0:ConfigId"] = "main",
            ["DbConnection:ConnectionConfigs:0:DbType"] = "PostgreSQL",
            ["DbConnection:ConnectionConfigs:0:ConnectionString"] = "Host=pg-main;Port=5432;Database=gameframex_admin;Username=u;Password=p;",
        }).Build();

        var main = HostMainDbConnectionResolver.Resolve(configuration, CreateLauncherOptions());

        Assert.Equal("main", main.ConfigId?.ToString());
        Assert.Equal(DbType.PostgreSQL, main.DbType);
        Assert.Equal("Host=pg-main;Port=5432;Database=gameframex_admin;Username=u;Password=p;", main.ConnectionString);
    }

    [Fact]
    public void Resolve_FallsBackToLauncherOptionsWhenNoConfiguredSource()
    {
        // 无 DbConnection 节、无 GFX_MAIN_DB_* 环境变量、输出目录无共享 Database.json：LauncherOptions 裸启动参数兜底（兼容层）
        // 注入空基目录隔离——测试输出目录经传递依赖必然存在 Configuration/Database.json（C279 起为合法默认来源，不应命中本用例）
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = null,
            ["GFX_MAIN_DB_CONNECTION_STRING"] = null,
        }, () =>
        {
            var configuration = new ConfigurationBuilder().Build();
            var launcherOptions = new LauncherOptions
            {
                DbType = "PostgreSQL",
                ConnectionString = "Host=legacy-main;Port=5432;Database=main;Username=u;Password=p;",
            };

            var main = HostMainDbConnectionResolver.Resolve(configuration, launcherOptions, CreateEmptyBaseDirectory());

            Assert.Equal("main", main.ConfigId?.ToString());
            Assert.Equal(DbType.PostgreSQL, main.DbType);
            Assert.Equal("Host=legacy-main;Port=5432;Database=main;Username=u;Password=p;", main.ConnectionString);
        });
    }

    [Fact]
    public void Resolve_ThrowsWhenGfxMainVariablesOnlyHalfSet()
    {
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_CONNECTION_STRING"] = "Host=half-set;Port=5432;Database=main;Username=u;Password=p;",
        }, () =>
        {
            var configuration = new ConfigurationBuilder().Build();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                                                                         HostMainDbConnectionResolver.Resolve(configuration, CreateLauncherOptions(), CreateEmptyBaseDirectory()));
            Assert.Contains("必须成对", exception.Message);
        });
    }

    [Fact]
    public void Resolve_ThrowsWhenNoSourceAndLauncherOptionsEmpty()
    {
        // 无节、无 GFX_MAIN_DB_*、无共享文件默认、兼容层裸参数也缺失：启动期友好报错而非 NRE（LauncherOptions 已降级非必填）
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = null,
            ["GFX_MAIN_DB_CONNECTION_STRING"] = null,
        }, () =>
        {
            var configuration = new ConfigurationBuilder().Build();
            var launcherOptions = new LauncherOptions();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                                                                         HostMainDbConnectionResolver.Resolve(configuration, launcherOptions, CreateEmptyBaseDirectory()));
            Assert.Contains("配置缺失", exception.Message);
        });
    }

    [Fact]
    public void Resolve_LoadsSharedDatabaseJsonWhenSectionMissing()
    {
        // C279：DbConnection 节为空 + 输出目录存在共享 Database.json → 采用文件默认（三 host 唯一默认来源）
        var baseDir = CreateSharedDatabaseJsonDirectory();
        try
        {
            WithEnvironmentVariables(new Dictionary<string, string?>
            {
                ["GFX_MAIN_DB_TYPE"] = null,
                ["GFX_MAIN_DB_CONNECTION_STRING"] = null,
            }, () =>
            {
                var configuration = new ConfigurationBuilder().Build();

                var main = HostMainDbConnectionResolver.Resolve(configuration, CreateLauncherOptions(), baseDir);

                Assert.Equal("main", main.ConfigId?.ToString());
                Assert.Equal(DbType.Sqlite, main.DbType);
                Assert.Equal("DataSource=./db/shared-default.db;", main.ConnectionString);
            });
        }
        finally
        {
            Directory.Delete(baseDir, true);
        }
    }

    [Fact]
    public void Resolve_PrefersConfigurationSectionOverSharedFile()
    {
        // 优先级锁定：显式 DbConnection 节存在时不读共享文件（显式节 > 环境变量 > 文件默认）
        var baseDir = CreateSharedDatabaseJsonDirectory();
        try
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DbConnection:ConnectionConfigs:0:ConfigId"] = "main",
                ["DbConnection:ConnectionConfigs:0:DbType"] = "PostgreSQL",
                ["DbConnection:ConnectionConfigs:0:ConnectionString"] = "Host=explicit-section;Port=5432;Database=main;Username=u;Password=p;",
            }).Build();

            var main = HostMainDbConnectionResolver.Resolve(configuration, CreateLauncherOptions(), baseDir);

            Assert.Equal(DbType.PostgreSQL, main.DbType);
            Assert.Equal("Host=explicit-section;Port=5432;Database=main;Username=u;Password=p;", main.ConnectionString);
        }
        finally
        {
            Directory.Delete(baseDir, true);
        }
    }

    private static LauncherOptions CreateLauncherOptions()
    {
        return new LauncherOptions
        {
            DbType = "Sqlite",
            ConnectionString = "DataSource=./db/GameFrameX.Admin.db;",
        };
    }

    /// <summary>
    /// 构造含 Configuration/Database.json 的临时基目录，模拟 Client/Hub 输出目录的共享默认文件。
    /// </summary>
    private static string CreateSharedDatabaseJsonDirectory()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "gfx-c279-shared-db-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(baseDir, "Configuration"));
        File.WriteAllText(
            Path.Combine(baseDir, "Configuration", "Database.json"),
            "{\"DbConnection\":{\"ConnectionConfigs\":[{\"ConfigId\":\"main\",\"DbType\":\"Sqlite\",\"ConnectionString\":\"DataSource=./db/shared-default.db;\"}]}}");
        return baseDir;
    }

    /// <summary>
    /// 构造无 Configuration/Database.json 的临时空基目录：隔离「测试输出目录经传递依赖存在共享默认文件」的环境巧合。
    /// </summary>
    private static string CreateEmptyBaseDirectory()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "gfx-c279-empty-base-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(baseDir);
        return baseDir;
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