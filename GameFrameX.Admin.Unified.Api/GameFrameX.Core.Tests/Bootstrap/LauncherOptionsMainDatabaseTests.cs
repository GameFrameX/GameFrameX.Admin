using GameFrameX.Core.Bootstrap;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Foundation.Options;
using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace GameFrameX.Core.Tests.Bootstrap;

public class LauncherOptionsMainDatabaseTests
{
    [Fact]
    public void AddMainDatabaseFromLauncherOptions_PairedOptions_ProducesMainSection()
    {
        var configuration = new ConfigurationBuilder()
                             .AddMainDatabaseFromLauncherOptions(new LauncherOptions { DbType = "PostgreSQL", ConnectionString = "Host=cli-main;Port=5432;Database=main;Username=u;Password=p;" })
                             .Build();

        Assert.Equal("main", configuration["DbConnection:ConnectionConfigs:0:ConfigId"]);
        Assert.Equal("PostgreSQL", configuration["DbConnection:ConnectionConfigs:0:DbType"]);
        Assert.Equal("Host=cli-main;Port=5432;Database=main;Username=u;Password=p;", configuration["DbConnection:ConnectionConfigs:0:ConnectionString"]);
    }

    [Fact]
    public void AddMainDatabaseFromLauncherOptions_HalfPair_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            new ConfigurationBuilder().AddMainDatabaseFromLauncherOptions(new LauncherOptions { DbType = "PostgreSQL" }).Build();
        });
        Assert.Contains("成对", exception.Message);
    }

    [Fact]
    public void AddMainDatabaseFromLauncherOptions_Absent_AddsNothing()
    {
        var configuration = new ConfigurationBuilder()
                             .AddMainDatabaseFromLauncherOptions(new LauncherOptions())
                             .Build();

        Assert.Null(configuration.GetSection("DbConnection")["ConnectionConfigs:0:ConfigId"]);
    }

    [Fact]
    public void EnvironmentPair_FlowsThroughLauncherOptions_AndBeatsSharedFile()
    {
        // 环境变量 GFX_MAIN_DB_* 与命令行参数是同一入口：无命令行时环境变量成对生效，压过共享 Database.json 兜底
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = "PostgreSQL",
            ["GFX_MAIN_DB_CONNECTION_STRING"] = "Host=env-main;Port=5432;Database=main;Username=u;Password=p;",
        }, () =>
        {
            var launcherOptions = OptionsBuilder.Create<LauncherOptions>(Array.Empty<string>());
            var configuration = new ConfigurationBuilder()
                                .AddMainDatabaseFromLauncherOptions(launcherOptions)
                                .Build();

            var main = HostMainDbConnectionResolver.Resolve(configuration, CreateEmptyBaseDirectory());

            Assert.Equal(DbType.PostgreSQL, main.DbType);
            Assert.Equal("Host=env-main;Port=5432;Database=main;Username=u;Password=p;", main.ConnectionString);
        });
    }

    [Fact]
    public void CommandLineArguments_BeatEnvironmentPair_ThroughUnifiedEntry()
    {
        // 命令行优先是统一入口的内置语义：OptionsBuilder 合并后 --DbType/--ConnectionString 压过 GFX_MAIN_DB_*
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = "Sqlite",
            ["GFX_MAIN_DB_CONNECTION_STRING"] = "DataSource=./db/env-default.db;",
        }, () =>
        {
            var launcherOptions = OptionsBuilder.Create<LauncherOptions>(new[] { "--DbType", "PostgreSQL", "--ConnectionString", "Host=cli-wins;Port=5432;Database=main;Username=u;Password=p;" });
            var configuration = new ConfigurationBuilder()
                                .AddMainDatabaseFromLauncherOptions(launcherOptions)
                                .Build();

            var main = HostMainDbConnectionResolver.Resolve(configuration, CreateEmptyBaseDirectory());

            Assert.Equal(DbType.PostgreSQL, main.DbType);
            Assert.Equal("Host=cli-wins;Port=5432;Database=main;Username=u;Password=p;", main.ConnectionString);
        });
    }

    [Fact]
    public void HalfEnvironmentPair_ThrowsAtUnifiedEntry()
    {
        // 半套环境变量在统一入口处即拒绝（provider 与连接串错配防线从启动参数入口前移）
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = null,
            ["GFX_MAIN_DB_CONNECTION_STRING"] = "Host=half-set;Port=5432;Database=main;Username=u;Password=p;",
        }, () =>
        {
            var launcherOptions = OptionsBuilder.Create<LauncherOptions>(Array.Empty<string>());

            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                new ConfigurationBuilder().AddMainDatabaseFromLauncherOptions(launcherOptions).Build();
            });
            Assert.Contains("成对", exception.Message);
        });
    }

    /// <summary>
    /// 构造无 Configuration/Database.json 的临时空基目录，隔离测试输出目录的共享默认文件。
    /// </summary>
    private static string CreateEmptyBaseDirectory()
    {
        var baseDirectory = Path.Combine(Path.GetTempPath(), "launcher-options-main-database-empty-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(baseDirectory);
        return baseDirectory;
    }

    private static void WithEnvironmentVariables(IReadOnlyDictionary<string, string?> values, Action action)
    {
        var original = values.Keys.Select(Environment.GetEnvironmentVariable).ToList();
        try
        {
            foreach (var pair in values)
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }

            action();
        }
        finally
        {
            foreach (var (key, value) in values.Keys.Zip(original))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
