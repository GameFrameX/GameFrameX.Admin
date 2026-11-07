using GameFrameX.Core.Bootstrap;
using GameFrameX.Core.SqlSugar;
using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace GameFrameX.Core.Tests.Bootstrap;

public class HostMainDbConnectionResolverTests
{
    [Fact]
    public void Resolve_BindsDbConnectionSectionAndDerivesMainDatabase()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DbConnection:ConnectionConfigs:0:ConfigId"] = "main",
            ["DbConnection:ConnectionConfigs:0:DbType"] = "PostgreSQL",
            ["DbConnection:ConnectionConfigs:0:ConnectionString"] = "Host=pg-main;Port=5432;Database=gameframex_admin;Username=u;Password=p;",
        }).Build();

        var main = HostMainDbConnectionResolver.Resolve(configuration);

        Assert.Equal("main", main.ConfigId?.ToString());
        Assert.Equal(DbType.PostgreSQL, main.DbType);
        Assert.Equal("Host=pg-main;Port=5432;Database=gameframex_admin;Username=u;Password=p;", main.ConnectionString);
    }

    [Fact]
    public void Resolve_BindsCommandLineArgumentsSection()
    {
        // 命令行 args 与环境变量同走标准配置管线：--DbConnection:... 形式的参数构成显式节，优先级高于 GFX_MAIN_DB_* 与共享文件
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = null,
            ["GFX_MAIN_DB_CONNECTION_STRING"] = null,
        }, () =>
        {
            var configuration = new ConfigurationBuilder().AddCommandLine(new[]
            {
                "--DbConnection:ConnectionConfigs:0:ConfigId", "main",
                "--DbConnection:ConnectionConfigs:0:DbType", "PostgreSQL",
                "--DbConnection:ConnectionConfigs:0:ConnectionString", "Host=cli-args;Port=5432;Database=main;Username=u;Password=p;",
            }).Build();

            var main = HostMainDbConnectionResolver.Resolve(configuration, CreateEmptyBaseDirectory());

            Assert.Equal(DbType.PostgreSQL, main.DbType);
            Assert.Equal("Host=cli-args;Port=5432;Database=main;Username=u;Password=p;", main.ConnectionString);
        });
    }

    [Fact]
    public void Resolve_HalfEnvironmentPairAloneDoesNotProvideConnection()
    {
        // 环境变量不是解析器的直接来源：GFX_MAIN_DB_* 由 LauncherOptions 统一入口合并并做成对校验（见 LauncherOptionsMainDatabaseTests）；
        // 仅半套环境变量且无配置节 / 无共享文件时按配置缺失报错
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_CONNECTION_STRING"] = "Host=half-set;Port=5432;Database=main;Username=u;Password=p;",
        }, () =>
        {
            var configuration = new ConfigurationBuilder().Build();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                                                                         HostMainDbConnectionResolver.Resolve(configuration, CreateEmptyBaseDirectory()));
            Assert.Contains("配置缺失", exception.Message);
        });
    }

    [Fact]
    public void Resolve_ThrowsWhenNoSourceConfigured()
    {
        // 无 DbConnection 节、无 GFX_MAIN_DB_* 环境变量、输出目录无共享 Database.json：启动期明确报配置缺失
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = null,
            ["GFX_MAIN_DB_CONNECTION_STRING"] = null,
        }, () =>
        {
            var configuration = new ConfigurationBuilder().Build();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                                                                         HostMainDbConnectionResolver.Resolve(configuration, CreateEmptyBaseDirectory()));
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

                var main = HostMainDbConnectionResolver.Resolve(configuration, baseDir);

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

            var main = HostMainDbConnectionResolver.Resolve(configuration, baseDir);

            Assert.Equal(DbType.PostgreSQL, main.DbType);
            Assert.Equal("Host=explicit-section;Port=5432;Database=main;Username=u;Password=p;", main.ConnectionString);
        }
        finally
        {
            Directory.Delete(baseDir, true);
        }
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