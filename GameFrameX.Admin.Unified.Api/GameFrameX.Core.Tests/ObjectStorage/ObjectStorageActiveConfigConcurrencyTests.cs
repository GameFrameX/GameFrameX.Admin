using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Business;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.ObjectStorage;
using SqlSugar;
using GameFrameX.Core.Extension;

namespace GameFrameX.Core.Tests.ObjectStorage;

/// <summary>
/// 唯一活动配置不变量的 DB 级测试（P1-3）。
/// 验证“停用其它 + 写当前启用”的事务在串行纪律下维持至多一条启用配置。
/// <para>注：测试复刻 <see cref="ObjectStorageConfigService"/> AddAsync/UpdateAsync 的串行纪律
/// （SemaphoreSlim 包裹 UseTran(DeactivateOthers + Insert/UpdateAsync)）；service 使用同一纪律的进程级
/// static SemaphoreSlim（见 service 字段 ActiveConfigTransitionLock 注释）。</para>
/// </summary>
public class ObjectStorageActiveConfigConcurrencyTests : IDisposable
{
    private readonly List<string> _dbPaths = new();

    public ObjectStorageActiveConfigConcurrencyTests()
    {
    }

    [Fact]
    public async Task ConcurrentEnableTransitions_WithSerialLock_LeaveExactlyOneEnabled()
    {
        var id = 1300000000001L;
        var db = OpenDb(id);

        // 预置 1 条启用配置
        var seed = DefaultDatabaseSeeder.BuildDefaultObjectStorageConfig();
        db.Insertable(seed).ExecuteCommand();

        // 复刻 service 的串行纪律：SemaphoreSlim(1,1) 包裹“停用其它 + 写当前启用”事务
        var gate = new SemaphoreSlim(1, 1);
        var tasks = Enumerable.Range(0, 8).Select(i => Task.Run(async () =>
        {
            var newId = 2000000000000L + i;
            var entity = DefaultDatabaseSeeder.BuildDefaultObjectStorageConfig();
            entity.Id = newId;
            entity.Remark = "concurrent-" + i;

            await gate.WaitAsync();
            try
            {
                db.Ado.UseTran(() =>
                {
                    db.Updateable<ObjectStorageConfigEntity>()
                      .SetColumns(e => new ObjectStorageConfigEntity { IsEnabled = false, })
                      .Where(e => e.IsEnabled == true && e.Id != newId)
                      .ExecuteCommand();
                    db.Insertable(entity).ExecuteCommand();
                });
            }
            finally
            {
                gate.Release();
            }
        })).ToArray();
        await Task.WhenAll(tasks);

        var enabledCount = db.Queryable<ObjectStorageConfigEntity>().Count(e => e.IsEnabled == true);
        Assert.Equal(1, enabledCount);
    }

    [Fact]
    public void DeactivateOthers_Mechanism_LeavesOnlyCurrentEnabled()
    {
        var id = 1300000000002L;
        var db = OpenDb(id);

        var first = DefaultDatabaseSeeder.BuildDefaultObjectStorageConfig();
        db.Insertable(first).ExecuteCommand();

        // 切换：停用其它 + 写当前启用
        var newId = 999L;
        db.Updateable<ObjectStorageConfigEntity>()
          .SetColumns(e => new ObjectStorageConfigEntity { IsEnabled = false, })
          .Where(e => e.IsEnabled == true && e.Id != newId)
          .ExecuteCommand();
        var fresh = DefaultDatabaseSeeder.BuildDefaultObjectStorageConfig();
        fresh.Id = newId;
        db.Insertable(fresh).ExecuteCommand();

        var enabledCount = db.Queryable<ObjectStorageConfigEntity>().Count(e => e.IsEnabled == true);
        var firstEnabled = db.Queryable<ObjectStorageConfigEntity>().First(e => e.Id == first.Id).IsEnabled;
        Assert.Equal(1, enabledCount);
        Assert.False(firstEnabled == true);
    }

    private ISqlSugarClient OpenDb(long id)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_active_config_{id}_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);
        var config = new DbConnectionConfig
        {
            ConfigId = id.ToString(),
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            DbSettings = new DbSettings { EnableInitDb = true, },
            TableSettings = new TableSettings { EnableInitTable = true, EnableIncreTable = false, },
            SeedSettings = new SeedSettings { EnableInitSeed = false, EnableIncreSeed = false, },
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        var scope = new SqlSugarScope(config);
        var db = scope.GetConnectionScope(id.ToString());
        db.CodeFirst.InitTables(typeof(ObjectStorageConfigEntity));
        return db;
    }

    public void Dispose()
    {
        foreach (var path in _dbPaths)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}