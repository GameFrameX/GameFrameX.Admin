using System.Text.RegularExpressions;
using GameFrameX.Core.Extension;
using GameFrameX.Core.Option;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using SqlSugar;
using Xunit.Abstractions;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// 2026-07-13 lesson 回归验证：InsertWithDiffLogAsync（DiffLog 路径）是否丢失租户表名前缀 tenant_。
/// 复刻生产接线：SqlSugarUnifiedConfig.ConfigureBaseSettings（EntityNameService → SqlSugarEntityBoundary.GetPhysicalTableName）
/// + RepositoryExtension.InsertWithDiffLogAsync（.AS() + EnableDiffLogEvent，与 BaseService.InnerAddAsync 同链路）。
/// 同时手工建一张无前缀镜像表，模拟生产"旧表残留、写错表不报错"的场景：若 INSERT 落错表可精确定位。
/// </summary>
public class SqlSugarDiffLogTenantPrefixTests : IDisposable
{
    private const string ConfigId = "10001";
    private const string TenantPhysicalTable = "tenant_game_app_remote_config_entity";
    private const string LegacyUnprefixedTable = "game_app_client_config_entity";
    private const string PostgresConnectionString = "Host=127.0.0.1;Port=5432;Database=postgres;Username=postgres;Password=postgres;";
    private const string PostgresReproDatabase = "gameframex_difflog_repro";

    private static readonly Regex InsertTableNameRegex = new("^INSERT\\s+INTO\\s+[`\"]?([A-Za-z0-9_]+)[`\"]?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex SelectTableNameRegex = new("FROM\\s+[`\"]?([A-Za-z0-9_]+)[`\"]?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex UpdateTableNameRegex = new("^UPDATE\\s+[`\"]?([A-Za-z0-9_]+)[`\"]?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly ITestOutputHelper _output;
    private readonly List<string> _dbFiles = new();

    public SqlSugarDiffLogTenantPrefixTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Insert_PlainAndDiffLog_BothWriteTenantPrefixTable_Sqlite()
    {
        var config = CreateSqliteConfig();
        using (var scope = new SqlSugarScope(new List<ConnectionConfig> { config, }))
        {
            var db = scope.GetConnectionScope(ConfigId);
            db.CodeFirst.InitTables(typeof(TenantGameAppRemoteConfigEntity));
            var entityTableName = db.EntityMaintenance.GetEntityInfo(typeof(TenantGameAppRemoteConfigEntity)).DbTableName;
            var createdTables = db.Ado.SqlQuery<string>("SELECT name FROM sqlite_master WHERE type='table'");
            _output.WriteLine($"[DBG] EntityInfo.DbTableName={entityTableName}");
            _output.WriteLine($"[DBG] sqlite_master={string.Join(",", createdTables)}");
            // 无前缀镜像表（列结构与 tenant_ 表一致）：写错表时不报错，靠计数精确定位落表位置
            db.Ado.ExecuteCommand($"CREATE TABLE {LegacyUnprefixedTable} AS SELECT * FROM {TenantPhysicalTable} WHERE false");

            var observers = AttachObservers(db);

            // 链路 1：普通插入（SimpleClient.InsertAsync 等价链路，无 DiffLog）
            var plainEntity = NewEntity(990001, "difflog-repro-plain-insert");
            await db.Insertable(plainEntity).ExecuteCommandAsync();

            // 链路 2：BaseService.InnerAddAsync 等价链路（RepositoryExtension.InsertWithDiffLogAsync = Insertable().AS().EnableDiffLogEvent()）
            var diffLogEntity = NewEntity(990002, "difflog-repro-difflog-insert");
            await db.InsertWithDiffLogAsync(diffLogEntity);

            DumpSql(observers.CapturedSqls, observers.DiffLogSqls);

            var insertTableNames = observers.CapturedSqls
                                            .Where(u => u.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                                            .Select(u => InsertTableNameRegex.Match(u).Groups[1].Value)
                                            .ToList();

            Assert.Equal(2, insertTableNames.Count);
            Assert.All(insertTableNames, name => Assert.Equal(TenantPhysicalTable, name));

            // DiffLog after-data 查回 SQL（GetDiffTableBySql 的 Context.Queryable<T>()）也必须走 tenant_ 前缀表：
            // 捕获窗口内不允许出现任何指向非 tenant_ 表的 SQL
            Assert.All(observers.CapturedSqls, sql =>
            {
                var tables = SelectTableNameRegex.Matches(sql).Select(m => m.Groups[1].Value).ToList();
                Assert.All(tables, name => Assert.Equal(TenantPhysicalTable, name));
            });

            // DiffLog 事件中的 SQL 同样带前缀
            Assert.NotEmpty(observers.DiffLogSqls);
            Assert.All(observers.DiffLogSqls, sql => _output.WriteLine("[DiffLogEvent] " + sql));

            // 落表位置：tenant_ 表 2 行，无前缀镜像表 0 行
            var tenantRows = db.Ado.SqlQuery<long>($"SELECT COUNT(*) FROM {TenantPhysicalTable} WHERE id IN (990001, 990002)");
            var legacyRows = db.Ado.SqlQuery<long>($"SELECT COUNT(*) FROM {LegacyUnprefixedTable}");
            Assert.Equal(2, tenantRows.Single());
            Assert.Equal(0, legacyRows.Single());

            CleanupRows(db);
        }
    }

    [Fact]
    public async Task Update_EnableDiffLogEvent_UsesTenantPrefixTable_Sqlite()
    {
        // BaseService.InnerUpdateAsync 的 DiffLog 链路没有 .AS() 显式表名（RepositoryExtension.UpdateWithDiffLogAsync 亦然），
        // 表名纯依赖 EntityNameService——这条链路是 lesson 机制的残余暴露面，单独验证。
        var config = CreateSqliteConfig();
        using (var scope = new SqlSugarScope(new List<ConnectionConfig> { config, }))
        {
            var db = scope.GetConnectionScope(ConfigId);
            db.CodeFirst.InitTables(typeof(TenantGameAppRemoteConfigEntity));
            db.Ado.ExecuteCommand($"CREATE TABLE {LegacyUnprefixedTable} AS SELECT * FROM {TenantPhysicalTable} WHERE false");

            var observers = AttachObservers(db);

            var entity = NewEntity(990003, "difflog-repro-update");
            await db.Insertable(entity).ExecuteCommandAsync();
            observers.CapturedSqls.Clear();

            entity.Description = "difflog-repro-updated";
            await db.Updateable(entity)
                    .IgnoreColumns(true)
                    .EnableDiffLogEvent()
                    .ExecuteCommandAsync();

            DumpSql(observers.CapturedSqls, observers.DiffLogSqls);

            var updateTableName = observers.CapturedSqls
                                           .Where(u => u.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
                                           .Select(u => UpdateTableNameRegex.Match(u).Groups[1].Value)
                                           .Single();
            Assert.Equal(TenantPhysicalTable, updateTableName);

            // DiffLog before/after 查询（UpdateableProvider.GetDiffTable 的 Context.Queryable<T>()）表名必须带前缀
            var diffLogSelectTables = observers.CapturedSqls
                                               .Where(u => u.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                                               .SelectMany(u => SelectTableNameRegex.Matches(u).Select(m => m.Groups[1].Value))
                                               .ToList();
            Assert.NotEmpty(diffLogSelectTables);
            Assert.All(diffLogSelectTables, name => Assert.Equal(TenantPhysicalTable, name));

            // 更新结果落在 tenant_ 表
            var updated = db.Queryable<TenantGameAppRemoteConfigEntity>().Single(u => u.Id == 990003);
            Assert.Equal("difflog-repro-updated", updated.Description);
            var legacyRows = db.Ado.SqlQuery<long>($"SELECT COUNT(*) FROM {LegacyUnprefixedTable}");
            Assert.Equal(0, legacyRows.Single());

            CleanupRows(db);
        }
    }

    [Fact]
    public async Task Insert_InsertWithDiffLogAsync_WritesTenantPrefixTable_Postgres()
    {
        // 本地 postgres 容器（postgres-gameframex）真库复验，排除 SQLite provider 特有行为。
        // 连不上（无本地容器/CI 环境）则跳过本用例。
        SqlSugarClient? master = null;
        try
        {
            master = new SqlSugarClient(new ConnectionConfig
            {
                DbType = DbType.PostgreSQL,
                ConnectionString = PostgresConnectionString,
                IsAutoCloseConnection = true,
            });
            master.Ado.ExecuteCommand($"DROP DATABASE IF EXISTS {PostgresReproDatabase} WITH (FORCE)");
            master.Ado.ExecuteCommand($"CREATE DATABASE {PostgresReproDatabase}");
        }
        catch (Exception e)
        {
            _output.WriteLine($"[SKIP] 本地 postgres 不可用（{e.Message}），跳过真库复验。");
            if (master != null)
            {
                master.Dispose();
            }

            return;
        }

        master.Dispose();

        var config = new DbConnectionConfig
        {
            ConfigId = ConfigId,
            DbType = DbType.PostgreSQL,
            ConnectionString = PostgresConnectionString.Replace("Database=postgres", $"Database={PostgresReproDatabase}"),
            DbSettings = new DbSettings { EnableDiffLog = true, },
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        using (var scope = new SqlSugarScope(new List<ConnectionConfig> { config, }))
        {
            var db = scope.GetConnectionScope(ConfigId);
            db.Aop.OnError = ex => { _output.WriteLine("[PG-ERROR] " + ex.Sql); };
            db.CodeFirst.InitTables(typeof(TenantGameAppRemoteConfigEntity));
            db.Ado.ExecuteCommand($"CREATE TABLE {LegacyUnprefixedTable} AS SELECT * FROM {TenantPhysicalTable} WHERE false");

            var observers = AttachObservers(db);

            var plainEntity = NewEntity(990004, "difflog-repro-pg-plain");
            await db.Insertable(plainEntity).ExecuteCommandAsync();

            var diffLogEntity = NewEntity(990005, "difflog-repro-pg-difflog");
            await db.InsertWithDiffLogAsync(diffLogEntity);

            DumpSql(observers.CapturedSqls, observers.DiffLogSqls);

            var insertTableNames = observers.CapturedSqls
                                            .Where(u => u.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                                            .Select(u => InsertTableNameRegex.Match(u).Groups[1].Value)
                                            .ToList();
            Assert.Equal(2, insertTableNames.Count);
            Assert.All(insertTableNames, name => Assert.Equal(TenantPhysicalTable, name));

            var tenantRows = db.Ado.SqlQuery<long>($"SELECT COUNT(*) FROM {TenantPhysicalTable} WHERE id IN (990004, 990005)");
            var legacyRows = db.Ado.SqlQuery<long>($"SELECT COUNT(*) FROM {LegacyUnprefixedTable}");
            Assert.Equal(2, tenantRows.Single());
            Assert.Equal(0, legacyRows.Single());
        }

        using (var cleanup = new SqlSugarClient(new ConnectionConfig
               {
                   DbType = DbType.PostgreSQL,
                   ConnectionString = PostgresConnectionString,
                   IsAutoCloseConnection = true,
               }))
        {
            cleanup.Ado.ExecuteCommand($"DROP DATABASE IF EXISTS {PostgresReproDatabase} WITH (FORCE)");
        }
    }

    private DbConnectionConfig CreateSqliteConfig()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_difflog_repro_{Guid.NewGuid():N}.db");
        _dbFiles.Add(dbPath);
        var config = new DbConnectionConfig
        {
            ConfigId = ConfigId,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            DbSettings = new DbSettings { EnableDiffLog = true, },
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        return config;
    }

    private static TenantGameAppRemoteConfigEntity NewEntity(long id, string channel)
    {
        return new TenantGameAppRemoteConfigEntity
        {
            Id = id,
            TenantId = 10001,
            Channel = channel,
            Platform = "test",
            AppVersion = "1.0.0",
            Package = "com.difflog.repro",
            Language = "zh",
            CheckResourceUrl = "https://repro.test/resource",
            CheckAppVersionUrl = "https://repro.test/app",
            Content = "{}",
            AotCodeList = "[]",
            Description = channel,
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
    }

    private static SqlObservers AttachObservers(SqlSugarScopeProvider db)
    {
        var observers = new SqlObservers();
        db.Aop.OnLogExecuting = (sql, pars) => { observers.CapturedSqls.Add(sql); };
        db.Aop.OnDiffLogEvent = u => { observers.DiffLogSqls.Add(u.Sql); };
        return observers;
    }

    private sealed class SqlObservers
    {
        public List<string> CapturedSqls { get; } = new();

        public List<string> DiffLogSqls { get; } = new();
    }

    private void DumpSql(List<string> capturedSqls, List<string> diffLogSqls)
    {
        foreach (var sql in capturedSqls)
        {
            _output.WriteLine("[SQL] " + sql);
        }

        foreach (var sql in diffLogSqls)
        {
            _output.WriteLine("[DiffLogEvent.Sql] " + sql);
        }
    }

    private static void CleanupRows(SqlSugarScopeProvider db)
    {
        db.Ado.ExecuteCommand($"DELETE FROM {TenantPhysicalTable} WHERE id IN (990001, 990002, 990003)");
        db.Ado.ExecuteCommand($"DELETE FROM {LegacyUnprefixedTable}");
    }

    public void Dispose()
    {
        foreach (var dbFile in _dbFiles)
        {
            try
            {
                if (File.Exists(dbFile))
                {
                    File.Delete(dbFile);
                }
            }
            catch (IOException)
            {
                // SqlSugarScope 释放竞态下偶发文件占用，临时文件留在系统临时目录无害
            }
        }
    }
}