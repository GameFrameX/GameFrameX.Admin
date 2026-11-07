using GameFrameX.Core.Option;
using SqlSugar;

namespace GameFrameX.Core.Tests.Option;

public class MainDatabaseConnectionGuardTests
{
    [Theory]
    [InlineData(DbType.Sqlite, "DataSource=single.db")]
    [InlineData(DbType.PostgreSQL, "Host=localhost;Database=single;Username=user;Password=password")]
    [InlineData(DbType.MySql, "Server=localhost;Database=single;Uid=user;Pwd=password")]
    public void EnsureSingleMainDatabase_AcceptsSingleMain(DbType databaseType, string connectionString)
    {
        var actual = MainDatabaseConnectionGuard.EnsureSingleMainDatabase(CreateConfigs(databaseType, connectionString));

        var config = Assert.Single(actual);
        Assert.Equal("main", config.ConfigId);
        Assert.Equal(connectionString, config.ConnectionString);
    }

    [Fact]
    public void EnsureSingleMainDatabase_FillsBlankConfigIdWithMain()
    {
        var connectionConfigs = new List<DbConnectionConfig> { new() { ConfigId = null!, DbType = DbType.Sqlite, ConnectionString = "DataSource=main.db" } };

        MainDatabaseConnectionGuard.EnsureSingleMainDatabase(connectionConfigs);

        Assert.Equal("main", Assert.Single(connectionConfigs).ConfigId);
    }

    [Fact]
    public void EnsureSingleMainDatabase_RejectsMultipleConnections()
    {
        var connectionConfigs = CreateConfigs(DbType.Sqlite, "DataSource=main.db");
        connectionConfigs.Add(new DbConnectionConfig { ConfigId = "extra", DbType = DbType.Sqlite, ConnectionString = "DataSource=extra.db" });

        Assert.Throws<InvalidOperationException>(() => MainDatabaseConnectionGuard.EnsureSingleMainDatabase(connectionConfigs));
    }

    [Fact]
    public void EnsureSingleMainDatabase_RejectsNonMainConnection()
    {
        var connectionConfigs = CreateConfigs(DbType.Sqlite, "DataSource=main.db");
        connectionConfigs[0].ConfigId = "legacy";

        Assert.Throws<InvalidOperationException>(() => MainDatabaseConnectionGuard.EnsureSingleMainDatabase(connectionConfigs));
    }

    [Theory]
    [InlineData(DbType.Sqlite, "Host=postgres;Database=main")]
    [InlineData(DbType.PostgreSQL, "DataSource=main.db")]
    public void EnsureSingleMainDatabase_RejectsProviderMismatch(DbType databaseType, string connectionString)
    {
        var connectionConfigs = CreateConfigs(databaseType, connectionString);

        Assert.Throws<InvalidOperationException>(() => MainDatabaseConnectionGuard.EnsureSingleMainDatabase(connectionConfigs));
    }

    private static List<DbConnectionConfig> CreateConfigs(DbType databaseType, string connectionString)
    {
        return new List<DbConnectionConfig> { new() { ConfigId = "main", DbType = databaseType, ConnectionString = connectionString } };
    }
}
