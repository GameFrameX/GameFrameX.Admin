using GameFrameX.Core.Option;
using SqlSugar;

namespace GameFrameX.Core.Tests.Option;

public class MainDatabaseConnectionGuardTests
{
    [Theory]
    [InlineData(DbType.Sqlite, "DataSource=single.db")]
    [InlineData(DbType.PostgreSQL, "Host=localhost;Database=single;Username=user;Password=password")]
    [InlineData(DbType.MySql, "Server=localhost;Database=single;Uid=user;Pwd=password")]
    public void EnsureMainDatabase_AcceptsMainConnection(DbType databaseType, string connectionString)
    {
        var actual = MainDatabaseConnectionGuard.EnsureMainDatabase(CreateConfig(databaseType, connectionString));

        Assert.Equal("main", actual.ConfigId);
        Assert.Equal(connectionString, actual.ConnectionString);
    }

    [Fact]
    public void EnsureMainDatabase_FillsBlankConfigIdWithMain()
    {
        var connectionConfig = new DbConnectionConfig { ConfigId = null!, DbType = DbType.Sqlite, ConnectionString = "DataSource=main.db" };

        MainDatabaseConnectionGuard.EnsureMainDatabase(connectionConfig);

        Assert.Equal("main", connectionConfig.ConfigId);
    }

    [Fact]
    public void EnsureMainDatabase_RejectsNonMainConnection()
    {
        var connectionConfig = CreateConfig(DbType.Sqlite, "DataSource=main.db");
        connectionConfig.ConfigId = "legacy";

        Assert.Throws<InvalidOperationException>(() => MainDatabaseConnectionGuard.EnsureMainDatabase(connectionConfig));
    }

    [Fact]
    public void EnsureMainDatabase_RejectsMissingConnectionString()
    {
        var connectionConfig = new DbConnectionConfig { ConfigId = "main", DbType = DbType.Sqlite, ConnectionString = null! };

        var exception = Assert.Throws<InvalidOperationException>(() => MainDatabaseConnectionGuard.EnsureMainDatabase(connectionConfig));
        Assert.Contains("配置缺失", exception.Message);
    }

    [Theory]
    [InlineData(DbType.Sqlite, "Host=postgres;Database=main")]
    [InlineData(DbType.PostgreSQL, "DataSource=main.db")]
    public void EnsureMainDatabase_RejectsProviderMismatch(DbType databaseType, string connectionString)
    {
        var connectionConfig = CreateConfig(databaseType, connectionString);

        Assert.Throws<InvalidOperationException>(() => MainDatabaseConnectionGuard.EnsureMainDatabase(connectionConfig));
    }

    private static DbConnectionConfig CreateConfig(DbType databaseType, string connectionString)
    {
        return new DbConnectionConfig { ConfigId = "main", DbType = databaseType, ConnectionString = connectionString };
    }
}
