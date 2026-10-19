using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.System;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// C278：种子执行默认 insert-only（只补缺，不覆盖用户已修改行）；带 [IncrementSeed] 特性的权威收敛种子仍按 seed 覆盖；
/// EnableIncreSeed=true 时收窄为只执行权威种子。
/// </summary>
[Collection("MenuSeedCatalog")]
public class SeedInsertOnlyTests : IDisposable
{
    private static readonly Type[] MainSeedTypes =
    {
        typeof(SystemConfigEntity),
        typeof(SystemConfigSeedData),
        typeof(GlobalMenuEntity),
        typeof(GlobalMenuSeedData),
    };

    private readonly List<string> _dbPaths = new();

    public SeedInsertOnlyTests()
    {
        // 种子 insert 触发二级缓存清理（IsAutoRemoveDataCache），静态运行期缓存须先就位
        SqlSugarCache.Configure(new FakeCache());
    }

    [Fact]
    public void InitDatabase_SecondRun_DoesNotOverwriteModifiedOrdinarySeedRow()
    {
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");

            SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes);

            var mainDb = scope.GetConnectionScope("main");
            // 模拟用户在管理端修改 sys_password 配置
            mainDb.Updateable<SystemConfigEntity>()
                  .SetColumns(u => u.Value == "999999999999")
                  .Where(u => u.Id == 1300000000102)
                  .ExecuteCommand();

            SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes);

            var value = mainDb.Queryable<SystemConfigEntity>().First(u => u.Id == 1300000000102).Value;
            Assert.Equal("999999999999", value);
        }
    }

    [Fact]
    public void InitDatabase_SecondRun_AuthoritativeSeedStillConverges()
    {
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");
            var seedRow = GlobalMenuSeedData.BuildData().First();

            SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes);

            var mainDb = scope.GetConnectionScope("main");
            // 模拟 DB 中 path 漂移（如历史迁移残留）
            mainDb.Updateable<GlobalMenuEntity>()
                  .SetColumns(u => u.Path == "/legacy/wrong/path")
                  .Where(u => u.Id == seedRow.Id)
                  .ExecuteCommand();

            SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes);

            var path = mainDb.Queryable<GlobalMenuEntity>().First(u => u.Id == seedRow.Id).Path;
            Assert.Equal(seedRow.Path, path);
        }
    }

    [Fact]
    public void InitDatabase_FirstRun_InsertsMissingSeedRows()
    {
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");

            SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes);

            var mainDb = scope.GetConnectionScope("main");
            Assert.True(mainDb.Queryable<SystemConfigEntity>().Any(u => u.Id == 1300000000102));
            Assert.True(mainDb.Queryable<GlobalMenuEntity>().Any());
        }
    }

    [Fact]
    public void InitDatabase_EnableIncreSeedTrue_RunsAuthoritativeSeedsOnly()
    {
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");
            config.SeedSettings.EnableIncreSeed = true;

            SqlSugarSetup.InitDatabase(scope, config, MainSeedTypes);

            var mainDb = scope.GetConnectionScope("main");
            // 权威种子（GlobalMenuSeedData 带 [IncrementSeed]）执行
            Assert.True(mainDb.Queryable<GlobalMenuEntity>().Any());
            // 普通种子（SystemConfigSeedData 未标记）被收窄跳过
            Assert.False(mainDb.Queryable<SystemConfigEntity>().Any());
        }
    }

    [Fact]
    public void DefaultTenantSeeder_Upsert_InsertsMissingRowsOnly()
    {
        using (var scope = CreateScope("tenant_2001"))
        {
            var tenantConfig = CreateConfig("tenant_2001");
            SqlSugarSetup.InitTenantDatabase(scope.AsTenant(), tenantConfig, new[] { typeof(TenantRoleEntity), });

            var tenantDb = scope.GetConnectionScope("tenant_2001");
            var role = new TenantRoleEntity { Id = 99001, TenantId = 2001, Name = "初始角色", Code = "init_role", };
            DefaultTenantDatabaseSeeder.Upsert(tenantDb, new[] { role, });

            // 模拟用户在管理端改名
            tenantDb.Updateable<TenantRoleEntity>()
                    .SetColumns(u => u.Name == "用户改过")
                    .Where(u => u.Id == 99001)
                    .ExecuteCommand();

            // 二次 seed（如默认租户库重复初始化）：同名主键行不再覆盖
            var seedAgain = new TenantRoleEntity { Id = 99001, TenantId = 2001, Name = "seed 新值", Code = "init_role", };
            DefaultTenantDatabaseSeeder.Upsert(tenantDb, new[] { seedAgain, });

            var name = tenantDb.Queryable<TenantRoleEntity>().First(u => u.Id == 99001).Name;
            Assert.Equal("用户改过", name);
        }
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

    private SqlSugarScope CreateScope(params string[] configIds)
    {
        return new SqlSugarScope(configIds.Select(configId =>
        {
            var config = CreateConfig(configId);
            SqlSugarSetup.SetDbConfig(config);
            return (ConnectionConfig)config;
        }).ToList());
    }

    private DbConnectionConfig CreateConfig(string configId)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_admin_seed_{configId}_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);

        var config = new DbConnectionConfig
        {
            ConfigId = configId,
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
                EnableInitSeed = true,
                EnableIncreSeed = false,
            },
        };
        SqlSugarSetup.SetDbConfig(config);
        return config;
    }
}