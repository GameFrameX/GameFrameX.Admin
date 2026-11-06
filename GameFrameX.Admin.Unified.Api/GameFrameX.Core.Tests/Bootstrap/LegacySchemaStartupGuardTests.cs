using GameFrameX.Core.Bootstrap;
using GameFrameX.Core.Option;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Tests.Bootstrap;

/// <summary>
/// 旧库拒绝启动守卫测试（C406 / D7）：预建旧 schema 标志表后初始化必须抛错且不建表；空库正常初始化；
/// 生产装配顺序（AddSqlSugar 循环内守卫先于 InitDatabase）以源码顺序断言守护。
/// <para>SQLite 临时库承载。</para>
/// </summary>
[Collection("MenuSeedCatalog")]
public sealed class LegacySchemaStartupGuardTests : IDisposable
{
    private readonly List<string> _dbPaths = new();

    [Fact]
    public void EnsureNotLegacySchema_TenantPrefixedTable_Throws()
    {
        using var scope = CreateScopeWithLegacyTable("tenant_game_user_entity", out _); // c406-keep

        var exception = Assert.Throws<InvalidOperationException>(
            () => LegacySchemaStartupGuard.EnsureNotLegacySchema(scope, CreateConfig("main")));

        Assert.Contains("2.0 不支持旧库升级，只能用新库或整体回滚", exception.Message);
        Assert.Contains("tenant_game_user_entity", exception.Message); // c406-keep
    }

    [Fact]
    public void EnsureNotLegacySchema_LegacyTenantRegistryTable_Throws()
    {
        using var scope = CreateScopeWithLegacyTable("system_tenant_entity", out _); // c406-keep

        var exception = Assert.Throws<InvalidOperationException>(
            () => LegacySchemaStartupGuard.EnsureNotLegacySchema(scope, CreateConfig("main")));

        Assert.Contains("2.0 不支持旧库升级，只能用新库或整体回滚", exception.Message);
        Assert.Contains("system_tenant_entity", exception.Message); // c406-keep
    }

    [Fact]
    public void EnsureNotLegacySchema_EmptyDatabase_PassesAndInitDatabaseCreatesTables()
    {
        using var scope = CreateScope("main");
        var config = CreateConfig("main");

        LegacySchemaStartupGuard.EnsureNotLegacySchema(scope, config);
        SqlSugarSetup.InitDatabase(scope, config, new[] { typeof(SystemUserEntity), typeof(GlobalMenuEntity), });

        var database = scope.GetConnectionScope("main");
        Assert.True(database.DbMaintenance.IsAnyTable("system_user_entity", false));
        Assert.True(database.DbMaintenance.IsAnyTable("global_menu_entity", false));
    }

    [Fact]
    public void GuardedInitSequence_LegacySchema_ThrowsBeforeCreatingAnyTable()
    {
        using var scope = CreateScopeWithLegacyTable("tenant_game_user_entity", out _); // c406-keep
        var config = CreateConfig("main");
        var types = new[] { typeof(SystemUserEntity), typeof(GlobalMenuEntity), };

        // 复刻 AddSqlSugar 初始化循环的顺序：守卫先于 InitDatabase（雪花单例使 AddSqlSugar 无法在测试进程二次装配）
        var exception = Record.Exception(() =>
        {
            LegacySchemaStartupGuard.EnsureNotLegacySchema(scope, config);
            SqlSugarSetup.InitDatabase(scope, config, types);
        });

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("2.0 不支持旧库升级，只能用新库或整体回滚", exception!.Message);

        // 守卫在建表前抛出：除预建的旧表外不得出现任何新表
        var tables = scope.GetConnectionScope("main").DbMaintenance.GetTableInfoList(false).Select(table => table.Name).ToList();
        Assert.Equal(new[] { "tenant_game_user_entity", }, tables); // c406-keep
    }

    [Fact]
    public void AddSqlSugar_Wiring_GuardRunsBeforeInitDatabase()
    {
        // 生产装配顺序守护：AddSqlSugar 初始化循环内守卫必须先于 InitDatabase 调用（建表前 fail-closed 拦截）。
        // AddSqlSugar 端到端装配在测试进程不可行（紧凑雪花单例拒绝二次 InitSnowFlake），以源码顺序断言代替。
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "GameFrameX.Core", "SqlSugar", "SqlSugarSetup.cs"));
        var guardIndex = source.IndexOf("LegacySchemaStartupGuard.EnsureNotLegacySchema", StringComparison.Ordinal);
        var initIndex = source.IndexOf("InitDatabase(sqlSugar, config, effectiveTypes)", StringComparison.Ordinal);

        Assert.True(guardIndex >= 0, "AddSqlSugar 必须调用 LegacySchemaStartupGuard.EnsureNotLegacySchema");
        Assert.True(initIndex > guardIndex, "守卫调用必须先于 InitDatabase（建表前 fail-closed）");
    }

    [Theory]
    [InlineData("GameFrameX.Client.Api/Program.cs")]
    [InlineData("GameFrameX.Hub.Api/Program.cs")]
    public void ClientAndHubPrograms_WireLegacyGuardImmediatelyAfterDatabaseCreation(string relativePath)
    {
        var source = File.ReadAllText(Path.Combine(FindRepositoryRoot(), relativePath));
        var createIndex = source.IndexOf("SqlSugarClientConfig.AddSqlSugarClientService", StringComparison.Ordinal);
        var guardIndex = source.IndexOf("LegacySchemaStartupGuard.EnsureNotLegacySchema", StringComparison.Ordinal);
        Assert.True(createIndex >= 0 && guardIndex > createIndex, $"{relativePath} 必须在建立数据库客户端后调用旧库守卫");
    }

    [Theory]
    [InlineData(false)] // Client.Api：AddSqlSugarClientService(config) 后立即守卫
    [InlineData(true)]  // Hub.Api：带 userAuditHandler 的同一组合
    public void ClientAndHubStartupComposition_LegacySchema_FailsFast(bool withAuditHandler)
    {
        // 按两宿主 Program.cs 的真实组合：AddSqlSugarClientService 建客户端后立即 EnsureNotLegacySchema；
        // 旧 schema 必须在建表/写数据之前抛错终止启动
        var dbPath = CreateDbPath();
        var config = new DbConnectionConfig
        {
            ConfigId = "main",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            IsAutoCloseConnection = true,
        };
        using (var seedScope = new SqlSugarScope(new List<ConnectionConfig> { config, }))
        {
            seedScope.GetConnectionScope("main").Ado.ExecuteCommand("CREATE TABLE tenant_game_user_entity (Id INTEGER PRIMARY KEY);"); // c406-keep
        }

        var sqlSugar = withAuditHandler
                           ? SqlSugarClientConfig.AddSqlSugarClientService(config, _ => { })
                           : SqlSugarClientConfig.AddSqlSugarClientService(config);

        Assert.Throws<InvalidOperationException>(() => LegacySchemaStartupGuard.EnsureNotLegacySchema(sqlSugar, config));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClientAndHubStartupComposition_CleanDatabase_StartsGuardPasses(bool withAuditHandler)
    {
        var config = CreateConfig("main");
        var sqlSugar = withAuditHandler
                           ? SqlSugarClientConfig.AddSqlSugarClientService(config, _ => { })
                           : SqlSugarClientConfig.AddSqlSugarClientService(config);

        LegacySchemaStartupGuard.EnsureNotLegacySchema(sqlSugar, config);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GameFrameX.Admin.Unified.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found from " + AppContext.BaseDirectory);
    }

    private string CreateDbPath()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_legacy_guard_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);
        return dbPath;
    }

    private DbConnectionConfig CreateConfig(string configId)
    {
        var dbPath = CreateDbPath();
        var config = new DbConnectionConfig
        {
            ConfigId = configId,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            IsAutoCloseConnection = true,
            DbSettings = new DbSettings
            {
                EnableInitDb = true,
                EnableDiffLog = false,
            },
            TableSettings = new TableSettings
            {
                EnableInitTable = true,
                EnableIncreTable = false,
            },
            SeedSettings = new SeedSettings
            {
                EnableInitSeed = false,
                EnableIncreSeed = false,
            },
        };
        SqlSugarSetup.SetDbConfig(config);
        return config;
    }

    private SqlSugarScope CreateScope(string configId)
    {
        var config = CreateConfig(configId);
        return new SqlSugarScope(new List<ConnectionConfig> { config, });
    }

    /// <summary>
    /// 建一个带预置旧 schema 标志表的 SQLite 库并返回指向它的 scope（配置 ConfigId=main，与库文件对应）。
    /// </summary>
    private SqlSugarScope CreateScopeWithLegacyTable(string legacyTableName, out string dbPath)
    {
        dbPath = CreateDbPath();
        var config = new DbConnectionConfig
        {
            ConfigId = "main",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            IsAutoCloseConnection = true,
        };
        var scope = new SqlSugarScope(new List<ConnectionConfig> { config, });
        scope.GetConnectionScope("main").Ado.ExecuteCommand($"CREATE TABLE {legacyTableName} (Id INTEGER PRIMARY KEY);");
        return scope;
    }

    public void Dispose()
    {
        foreach (var dbPath in _dbPaths)
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }
}
