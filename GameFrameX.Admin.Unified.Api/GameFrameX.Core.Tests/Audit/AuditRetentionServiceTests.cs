using GameFrameX.Core.Cache;
using GameFrameX.Core.Tests.System;
using GameFrameX.Application.Audit.Retention;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Job;
using GameFrameX.Core.Option;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Log;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SqlSugar;

/// <summary>
/// 审计留存服务测试（C404 / GFX-1216 R3）：超期搬移、未超期保留、批边界、幂等、归档清理、开关关闭、作业注册。
/// </summary>
public sealed class AuditRetentionServiceTests : IDisposable
{
    private const long SecondsPerDay = 24 * 60 * 60;

    private readonly List<string> _dbPaths = new();

    [Fact]
    public async Task ExecuteAsync_ArchivesExpiredRowsAndKeepsFreshRows()
    {
        using (var scope = CreateScope())
        {
            var database = scope.GetConnectionScope(SqlSugarConst.MainConfigId);
            InitTables(database);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            database.Insertable(new SystemLogVisitEntity { Id = 101, Account = "old-visit", CreatedTime = now - 40 * SecondsPerDay, }).ExecuteCommand();
            database.Insertable(new SystemLogVisitEntity { Id = 102, Account = "fresh-visit", CreatedTime = now - SecondsPerDay, }).ExecuteCommand();
            database.Insertable(new SystemLogOperationEntity { Id = 103, Account = "old-operation", CreatedTime = now - 200 * SecondsPerDay, }).ExecuteCommand();
            database.Insertable(new SystemLogOperationEntity { Id = 104, Account = "fresh-operation", CreatedTime = now - SecondsPerDay, }).ExecuteCommand();

            var result = await CreateService(scope).ExecuteAsync(CancellationToken.None);

            Assert.True(result.Enabled);
            Assert.Equal(1, result.Tables[0].ArchivedCount);
            Assert.Equal(1, result.Tables[1].ArchivedCount);
            Assert.Equal(1, database.Queryable<SystemLogVisitEntity>().Where(u => u.Account == "fresh-visit").Count());
            Assert.Equal(0, database.Queryable<SystemLogVisitEntity>().Where(u => u.Account == "old-visit").Count());
            Assert.Equal(1, database.Queryable<SystemLogOperationEntity>().Where(u => u.Account == "fresh-operation").Count());
            Assert.Equal(0, database.Queryable<SystemLogOperationEntity>().Where(u => u.Account == "old-operation").Count());
            var archivedVisit = database.Queryable<SystemLogVisitArchiveEntity>().Single();
            Assert.Equal("old-visit", archivedVisit.Account);
            Assert.NotNull(archivedVisit.ArchivedTime);
            var archivedOperation = database.Queryable<SystemLogOperationArchiveEntity>().Single();
            Assert.Equal("old-operation", archivedOperation.Account);
            Assert.NotNull(archivedOperation.ArchivedTime);
            Assert.True(result.ElapsedMilliseconds >= 0);
        }
    }

    [Fact]
    public async Task ExecuteAsync_HandlesBatchBoundary()
    {
        using (var scope = CreateScope())
        {
            var database = scope.GetConnectionScope(SqlSugarConst.MainConfigId);
            InitTables(database);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            for (var index = 1; index <= 3; index++)
            {
                database.Insertable(new SystemLogVisitEntity { Id = 100 + index, Account = $"old-{index}", CreatedTime = now - 2 * SecondsPerDay, }).ExecuteCommand();
            }

            var options = CreateOptions(visitHotDataDays: 1, operationHotDataDays: 180, archiveRetentionDays: 0, batchSize: 2);
            var result = await CreateService(scope, options).ExecuteAsync(CancellationToken.None);

            Assert.Equal(3, result.Tables[0].ArchivedCount);
            Assert.Equal(0, database.Queryable<SystemLogVisitEntity>().Count());
            Assert.Equal(3, database.Queryable<SystemLogVisitArchiveEntity>().Count());
        }
    }

    [Fact]
    public async Task ExecuteAsync_IsIdempotentOnReRun()
    {
        using (var scope = CreateScope())
        {
            var database = scope.GetConnectionScope(SqlSugarConst.MainConfigId);
            InitTables(database);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            database.Insertable(new SystemLogVisitEntity { Id = 101, Account = "old-visit", CreatedTime = now - 40 * SecondsPerDay, }).ExecuteCommand();

            var service = CreateService(scope);
            await service.ExecuteAsync(CancellationToken.None);
            var secondRun = await service.ExecuteAsync(CancellationToken.None);

            Assert.Equal(0, secondRun.Tables[0].ArchivedCount);
            Assert.Equal(0, secondRun.Tables[1].ArchivedCount);
            Assert.Equal(1, database.Queryable<SystemLogVisitArchiveEntity>().Count());
        }
    }

    [Fact]
    public async Task ExecuteAsync_PurgesArchiveByRetentionDays()
    {
        using (var scope = CreateScope())
        {
            var database = scope.GetConnectionScope(SqlSugarConst.MainConfigId);
            InitTables(database);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            database.Insertable(new SystemLogVisitArchiveEntity { Id = 201, Account = "stale-archive", CreatedTime = now - 40 * SecondsPerDay, ArchivedTime = now - 3 * SecondsPerDay, }).ExecuteCommand();
            database.Insertable(new SystemLogVisitArchiveEntity { Id = 202, Account = "kept-archive", CreatedTime = now - 40 * SecondsPerDay, ArchivedTime = now - SecondsPerDay, }).ExecuteCommand();

            var options = CreateOptions(visitHotDataDays: 30, operationHotDataDays: 180, archiveRetentionDays: 2, batchSize: 5000);
            var result = await CreateService(scope, options).ExecuteAsync(CancellationToken.None);

            Assert.Equal(1, result.Tables[0].PurgedCount);
            Assert.Equal(1, database.Queryable<SystemLogVisitArchiveEntity>().Count());
            Assert.Equal("kept-archive", database.Queryable<SystemLogVisitArchiveEntity>().Single().Account);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ArchiveRetentionZeroKeepsForever()
    {
        using (var scope = CreateScope())
        {
            var database = scope.GetConnectionScope(SqlSugarConst.MainConfigId);
            InitTables(database);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            database.Insertable(new SystemLogOperationArchiveEntity { Id = 203, Account = "very-old", CreatedTime = now - 1000 * SecondsPerDay, ArchivedTime = now - 900 * SecondsPerDay, }).ExecuteCommand();

            var result = await CreateService(scope).ExecuteAsync(CancellationToken.None);

            Assert.Equal(0, result.Tables[1].PurgedCount);
            Assert.Equal(1, database.Queryable<SystemLogOperationArchiveEntity>().Count());
        }
    }

    [Fact]
    public async Task ExecuteAsync_DisabledLeavesDataUntouched()
    {
        using (var scope = CreateScope())
        {
            var database = scope.GetConnectionScope(SqlSugarConst.MainConfigId);
            InitTables(database);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            database.Insertable(new SystemLogVisitEntity { Id = 101, Account = "old-visit", CreatedTime = now - 40 * SecondsPerDay, }).ExecuteCommand();

            var options = CreateOptions(visitHotDataDays: 30, operationHotDataDays: 180, archiveRetentionDays: 0, batchSize: 5000);
            options.Enabled = false;
            var result = await CreateService(scope, options).ExecuteAsync(CancellationToken.None);

            Assert.False(result.Enabled);
            Assert.Empty(result.Tables);
            Assert.Equal(1, database.Queryable<SystemLogVisitEntity>().Count());
            Assert.Equal(0, database.Queryable<SystemLogVisitArchiveEntity>().Count());
        }
    }

    [Fact]
    public void AuditRetentionJob_RegistersAsBuiltInJobWithDailyTrigger()
    {
        var jobDetail = typeof(AuditRetentionJob).GetCustomAttributes(typeof(JobDetailAttribute), false).OfType<JobDetailAttribute>().Single();
        Assert.Equal("job_auditRetention", jobDetail.JobId);
        Assert.False(jobDetail.Concurrent);
        var daily = typeof(AuditRetentionJob).GetCustomAttributes(typeof(DailyAttribute), false).OfType<DailyAttribute>().Single();
        Assert.Equal("trigger_auditRetention", daily.TriggerId);
        Assert.Equal(typeof(IJob), typeof(AuditRetentionJob).GetInterfaces().Single());
    }

    private static void InitTables(ISqlSugarClient database)
    {
        database.CodeFirst.InitTables<SystemLogVisitEntity, SystemLogOperationEntity, SystemLogVisitArchiveEntity, SystemLogOperationArchiveEntity>();
    }

    private static AuditRetentionService CreateService(ISqlSugarClient scope, AuditRetentionOptions? options = null)
    {
        return new AuditRetentionService(scope, Options.Create(options ?? new AuditRetentionOptions()), NullLogger<AuditRetentionService>.Instance);
    }

    private static AuditRetentionOptions CreateOptions(int visitHotDataDays, int operationHotDataDays, int archiveRetentionDays, int batchSize)
    {
        return new AuditRetentionOptions
        {
            VisitHotDataDays = visitHotDataDays,
            OperationHotDataDays = operationHotDataDays,
            ArchiveRetentionDays = archiveRetentionDays,
            BatchSize = batchSize,
        };
    }

    private SqlSugarScope CreateScope()
    {
        SqlSugarCache.Configure(new FakeCache());
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_admin_audit_retention_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);
        var config = new DbConnectionConfig
        {
            ConfigId = SqlSugarConst.MainConfigId,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
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
        return new SqlSugarScope(new List<ConnectionConfig> { config, });
    }

    public void Dispose()
    {
        foreach (var dbPath in _dbPaths)
        {
            try
            {
                File.Delete(dbPath);
            }
            catch (IOException)
            {
            }
        }
    }
}
