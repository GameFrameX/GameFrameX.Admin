using GameFrameX.Core.Option;
using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace GameFrameX.Core.Tests.Option;

public class DbConnectionOptionsTests
{
    [Theory]
    [InlineData(DbType.Sqlite, "DataSource=single.db")]
    [InlineData(DbType.PostgreSQL, "Host=localhost;Database=single;Username=user;Password=password")]
    [InlineData(DbType.MySql, "Server=localhost;Database=single;Uid=user;Pwd=password")]
    public void PostConfigure_UsesOnlyMainDatabase(DbType databaseType, string connectionString)
    {
        var options = CreateOptions(databaseType, connectionString);
        options.PostConfigure(options, new ConfigurationBuilder().Build());
        var actual = Assert.Single(options.ConnectionConfigs);
        Assert.Equal("main", actual.ConfigId);
        Assert.Equal(connectionString, actual.ConnectionString);
    }

    [Fact]
    public void PostConfigure_RejectsMultipleConnections()
    {
        var options = CreateOptions(DbType.Sqlite, "DataSource=main.db");
        options.ConnectionConfigs.Add(new DbConnectionConfig { ConfigId = "legacy", DbType = DbType.Sqlite, ConnectionString = "DataSource=legacy.db" });
        Assert.Throws<InvalidOperationException>(() => options.PostConfigure(options, new ConfigurationBuilder().Build()));
    }

    [Fact]
    public void PostConfigure_RejectsNonMainConnection()
    {
        var options = CreateOptions(DbType.Sqlite, "DataSource=main.db");
        options.ConnectionConfigs[0].ConfigId = "legacy";
        Assert.Throws<InvalidOperationException>(() => options.PostConfigure(options, new ConfigurationBuilder().Build()));
    }

    [Fact]
    public void PostConfigure_CreatesMainFromEnvironmentPair()
    {
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = "Sqlite",
            ["GFX_MAIN_DB_CONNECTION_STRING"] = "DataSource=main.db",
        }, () =>
        {
            var options = new DbConnectionOptions();
            options.PostConfigure(options, new ConfigurationBuilder().Build());
            var actual = Assert.Single(options.ConnectionConfigs);
            Assert.Equal("main", actual.ConfigId);
            Assert.Equal(DbType.Sqlite, actual.DbType);
        });
    }

    [Theory]
    [InlineData("GFX_MAIN_DB_TYPE", "Sqlite")]
    [InlineData("GFX_MAIN_DB_CONNECTION_STRING", "DataSource=main.db")]
    public void PostConfigure_RejectsHalfEnvironmentPair(string variable, string value)
    {
        WithEnvironmentVariables(new Dictionary<string, string?>
        {
            ["GFX_MAIN_DB_TYPE"] = variable == "GFX_MAIN_DB_TYPE" ? value : null,
            ["GFX_MAIN_DB_CONNECTION_STRING"] = variable == "GFX_MAIN_DB_CONNECTION_STRING" ? value : null,
        }, () =>
        {
            var options = CreateOptions(DbType.Sqlite, "DataSource=main.db");
            var error = Assert.Throws<InvalidOperationException>(() => options.PostConfigure(options, new ConfigurationBuilder().Build()));
            Assert.Contains("必须成对", error.Message);
        });
    }

    [Theory]
    [InlineData(DbType.Sqlite, "Host=postgres;Database=main")]
    [InlineData(DbType.PostgreSQL, "DataSource=main.db")]
    public void PostConfigure_RejectsProviderMismatch(DbType databaseType, string connectionString)
    {
        var options = CreateOptions(databaseType, connectionString);
        Assert.Throws<InvalidOperationException>(() => options.PostConfigure(options, new ConfigurationBuilder().Build()));
    }

    [Fact]
    public void PostConfigure_AppliesIndexedMainConnectionOverride()
    {
        WithEnvironmentVariables(new Dictionary<string, string?> { ["DbConnection__ConnectionConfigs__0__ConnectionString"] = "DataSource=override.db" }, () =>
        {
            var options = CreateOptions(DbType.Sqlite, "DataSource=main.db");
            options.PostConfigure(options, new ConfigurationBuilder().Build());
            Assert.Equal("DataSource=override.db", Assert.Single(options.ConnectionConfigs).ConnectionString);
        });
    }

    private static DbConnectionOptions CreateOptions(DbType databaseType, string connectionString)
    {
        return new DbConnectionOptions
        {
            ConnectionConfigs = new List<DbConnectionConfig> { new() { ConfigId = "main", DbType = databaseType, ConnectionString = connectionString } },
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