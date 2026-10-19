using GameFrameX.Core.Option;
using Microsoft.Extensions.Configuration;

namespace GameFrameX.Core.Tests.Option;

/// <summary>
/// C278：数据库初始化开关代码默认值——Database.json 省略 Settings 节点时等价全开（除 EnableIncreSeed）。
/// </summary>
public class DbInitSettingsDefaultTests
{
    [Fact]
    public void DbSettings_Defaults_EnableInitDbAndDiffLog()
    {
        var settings = new DbSettings();

        Assert.True(settings.EnableInitDb);
        Assert.True(settings.EnableDiffLog);
    }

    [Fact]
    public void TableSettings_Defaults_EnableInitTableAndIncreTable()
    {
        var settings = new TableSettings();

        Assert.True(settings.EnableInitTable);
        Assert.True(settings.EnableIncreTable);
    }

    [Fact]
    public void SeedSettings_Defaults_EnableInitSeedWithoutIncreSeed()
    {
        var settings = new SeedSettings();

        Assert.True(settings.EnableInitSeed);
        Assert.False(settings.EnableIncreSeed);
    }

    [Fact]
    public void DbConnectionConfig_SettingsProperties_DefaultNonNull()
    {
        var config = new DbConnectionConfig();

        Assert.NotNull(config.DbSettings);
        Assert.NotNull(config.TableSettings);
        Assert.NotNull(config.SeedSettings);
        Assert.True(config.DbSettings.EnableInitDb);
        Assert.True(config.TableSettings.EnableInitTable);
        Assert.True(config.SeedSettings.EnableInitSeed);
    }

    [Fact]
    public void JsonBinding_WithoutSettingsNodes_YieldsCodeDefaults()
    {
        // 模拟 C278 精简后的 Database.json：主库项仅保留 ConfigId / DbType / ConnectionString
        var configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string?>
                            {
                                ["DbConnection:EnableConsoleSql"] = "false",
                                ["DbConnection:ConnectionConfigs:0:ConfigId"] = "main",
                                ["DbConnection:ConnectionConfigs:0:DbType"] = "Sqlite",
                                ["DbConnection:ConnectionConfigs:0:ConnectionString"] = "DataSource=./db/GameFrameX.Admin.db;",
                            })
                            .Build();

        var options = configuration.GetSection("DbConnection").Get<DbConnectionOptions>();

        var config = Assert.Single(options.ConnectionConfigs);
        Assert.True(config.DbSettings.EnableInitDb);
        Assert.True(config.DbSettings.EnableDiffLog);
        Assert.True(config.TableSettings.EnableInitTable);
        Assert.True(config.TableSettings.EnableIncreTable);
        Assert.True(config.SeedSettings.EnableInitSeed);
        Assert.False(config.SeedSettings.EnableIncreSeed);
    }

    [Fact]
    public void JsonBinding_ExplicitSettingsNodes_StillOverrideDefaults()
    {
        var configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string?>
                            {
                                ["DbConnection:ConnectionConfigs:0:ConfigId"] = "main",
                                ["DbConnection:ConnectionConfigs:0:DbSettings:EnableInitDb"] = "false",
                            })
                            .Build();

        var options = configuration.GetSection("DbConnection").Get<DbConnectionOptions>();

        var config = Assert.Single(options.ConnectionConfigs);
        Assert.False(config.DbSettings.EnableInitDb);
        Assert.True(config.DbSettings.EnableDiffLog);
    }
}