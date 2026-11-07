using GameFrameX.Core.Bootstrap;
using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace GameFrameX.Core.Tests.Bootstrap;

public class MainDatabaseStartupArgumentsTests
{
    [Fact]
    public void AddMainDatabaseStartupArguments_PairedArgumentsSpaceForm_ProducesMainSection()
    {
        var configuration = new ConfigurationBuilder()
                             .AddMainDatabaseStartupArguments(new[] { "--DbType", "PostgreSQL", "--ConnectionString", "Host=cli-main;Port=5432;Database=main;Username=u;Password=p;" })
                             .Build();

        var section = configuration.GetSection("DbConnection");
        Assert.Equal("main", section["ConnectionConfigs:0:ConfigId"]);
        Assert.Equal("PostgreSQL", section["ConnectionConfigs:0:DbType"]);
        Assert.Equal("Host=cli-main;Port=5432;Database=main;Username=u;Password=p;", section["ConnectionConfigs:0:ConnectionString"]);
        Assert.Equal(bool.TrueString, section["MainDatabaseFromStartupArguments"]);
    }

    [Fact]
    public void AddMainDatabaseStartupArguments_PairedArgumentsEqualsForm_ProducesMainSection()
    {
        var configuration = new ConfigurationBuilder()
                             .AddMainDatabaseStartupArguments(new[] { "--DbType=PostgreSQL", "--ConnectionString=Host=cli-equals;Port=5432;Database=main;Username=u;Password=p;" })
                             .Build();

        Assert.Equal("PostgreSQL", configuration["DbConnection:ConnectionConfigs:0:DbType"]);
        Assert.Equal("Host=cli-equals;Port=5432;Database=main;Username=u;Password=p;", configuration["DbConnection:ConnectionConfigs:0:ConnectionString"]);
    }

    [Fact]
    public void AddMainDatabaseStartupArguments_HalfPair_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            new ConfigurationBuilder().AddMainDatabaseStartupArguments(new[] { "--DbType", "PostgreSQL" }).Build();
        });
        Assert.Contains("成对", exception.Message);
    }

    [Fact]
    public void AddMainDatabaseStartupArguments_Absent_AddsNothing()
    {
        var configuration = new ConfigurationBuilder()
                             .AddMainDatabaseStartupArguments(Array.Empty<string>())
                             .Build();

        Assert.Null(configuration.GetSection("DbConnection")["ConnectionConfigs:0:ConfigId"]);
    }

    [Fact]
    public void StartupArguments_OverrideGfxMainEnvironmentPair_ThroughResolver()
    {
        // 命令行语义压过环境变量：裸参数标记使配置管线跳过 GFX_MAIN_DB_* 覆盖与成对校验，Database.json 仅兜底
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = "Sqlite",
            ["GFX_MAIN_DB_CONNECTION_STRING"] = "DataSource=./db/env-default.db;",
        }, () =>
        {
            var configuration = new ConfigurationBuilder()
                                 .AddMainDatabaseStartupArguments(new[] { "--DbType=PostgreSQL", "--ConnectionString=Host=cli-wins;Port=5432;Database=main;Username=u;Password=p;" })
                                 .Build();

            var main = HostMainDbConnectionResolver.Resolve(configuration, CreateEmptyBaseDirectory());

            Assert.Equal(DbType.PostgreSQL, main.DbType);
            Assert.Equal("Host=cli-wins;Port=5432;Database=main;Username=u;Password=p;", main.ConnectionString);
        });
    }

    /// <summary>
    /// 构造无 Configuration/Database.json 的临时空基目录，隔离测试输出目录的共享默认文件。
    /// </summary>
    private static string CreateEmptyBaseDirectory()
    {
        var baseDirectory = Path.Combine(Path.GetTempPath(), "main-db-startup-arguments-empty-" + Guid.NewGuid().ToString("N"));
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
