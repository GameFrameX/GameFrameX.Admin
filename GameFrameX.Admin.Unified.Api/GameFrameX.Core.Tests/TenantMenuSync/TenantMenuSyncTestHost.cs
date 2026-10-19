using GameFrameX.Core.Cache;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.TenantMenuSync;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.System;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace GameFrameX.Core.Tests.TenantMenuSync;

/// <summary>
/// 租户菜单同步端到端测试宿主（C353 T8）：真实 SqlSugar SQLite 主库（global_menu 喂 MenuSeedCatalog /
/// global_package(_menu) / system_tenant / system_tenant_menu_sync_state / system_log_operation）+
/// 租户库（tenant_role / tenant_role_menu / tenant_user）+ FakeTenantDbConnectionProvider + FakeCache。
/// <para>对齐 AuthTestHost（C217）模式；表结构走 SqlSugarSetup.InitDatabase 真实物理命名链路。</para>
/// </summary>
internal sealed class TenantMenuSyncTestHost : IDisposable
{
    public const long TenantId = 3300000000001;
    public const long PackageId = 4400000000001;
    public const long AdminRoleId = 3311000000001;
    public const long CustomRoleId = 3311000000002;
    public const string AdminRoleCode = "sys_admin";
    public const string CustomRoleCode = "my_role";

    private readonly SqlSugarScope _client;
    private readonly List<string> _dbPaths = new();
    private static long _snowflakeSeed = 7300000000000000000L;

    public TenantMenuSyncTestHost()
    {
        SqlSugarCache.Configure(new FakeCache());
        var mainConfig = CreateConfig(SqlSugarConst.MainConfigId);
        var tenantConfig = CreateConfig(TenantId.ToString());
        _client = new SqlSugarScope(new List<ConnectionConfig> { mainConfig, tenantConfig, });
        SqlSugarSetup.InitDatabase(_client, mainConfig, new[]
        {
            typeof(GlobalMenuEntity),
            typeof(GlobalPackageEntity),
            typeof(GlobalPackageMenuEntity),
            typeof(SystemTenantEntity),
            typeof(SystemTenantMenuSyncStateEntity),
            typeof(SystemLogOperationEntity),

        });
        SqlSugarSetup.InitDatabase(_client, tenantConfig, new[]
        {
            typeof(TenantRoleEntity),
            typeof(TenantRoleMenuEntity),
            typeof(TenantUserEntity),
        });

        ResetMenuSeedCatalogDbCache();
        var mainDb = _client.GetConnectionScope(SqlSugarConst.MainConfigId);
        // 最小 AOP：模拟生产 Insertable 主键雪花回填契约（进程级 CustomSnowFlakeFunc 链路，
        // 由 SqlSugarSnowflakeFallbackInitializer 安装，见 C361 / GFX-804），
        // 仅填 0 值 EntityBasic.Id，不引入 SetDbAop 的 resolver 依赖
        mainDb.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            if (entityInfo.OperationType == DataFilterType.InsertByObject
                && entityInfo.PropertyName == "Id"
                && ((dynamic)entityInfo.EntityValue).Id == 0L)
            {
                entityInfo.SetValue(Interlocked.Increment(ref _snowflakeSeed));
            }
        };
        mainDb.Insertable(MenuSeedCatalogTestData.BuildSampleMenus()).ExecuteCommand();
        MenuSeedCatalog.LoadFromDb(mainDb);
    }

    public SqlSugarScopeProvider MainDb
    {
        get { return _client.GetConnectionScope(SqlSugarConst.MainConfigId); }
    }

    public SqlSugarScopeProvider TenantDb
    {
        get { return _client.GetConnectionScope(TenantId.ToString()); }
    }

    /// <summary>
    /// 原始 SqlSugar 客户端（C354：TenantMenuSyncApplier.ComputeCeilingAsync 静态入口入参）。
    /// </summary>
    public ISqlSugarClient Client
    {
        get { return _client; }
    }

    public TenantMenuSyncApplier BuildApplier()
    {
        return new TenantMenuSyncApplier(
            _client,
            new FakeTenantDbConnectionProvider(_client, TenantId.ToString()),
            new SystemCacheService(new FakeCache(), Options.Create(new CacheOptions())),
            NullLogger<TenantMenuSyncApplier>.Instance);
    }

    /// <summary>
    /// 主库 seed 套餐（enabled 可控）+ 套餐菜单行（menuId 集合，原样落库不走编辑入口过滤）。
    /// </summary>
    public async Task SeedPackageAsync(bool isEnabled, params long[] menuIds)
    {
        await MainDb.Insertable(new GlobalPackageEntity
        {
            Id = PackageId,
            Name = "enterprise-test",
            Code = "enterprise-test",
            IsEnabled = isEnabled,
        }).ExecuteCommandAsync();

        if (menuIds.Length > 0)
        {
            // 直插不走仓储雪花 AOP，显式分配唯一 Id
            var rows = menuIds.Select((u, i) => new GlobalPackageMenuEntity { Id = 5000000000000 + i, PackageId = PackageId, MenuId = u }).ToArray();
            await MainDb.Insertable(rows).ExecuteCommandAsync();
        }
    }

    public async Task SeedTenantAsync(long? packageId)
    {
        await MainDb.Insertable(new SystemTenantEntity
        {
            Id = TenantId,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.Sqlite,
            Connection = string.Empty,
            ConfigId = TenantId.ToString(),
            IsEnabled = true,
            PackageId = packageId,
        }).ExecuteCommandAsync();
    }

    public async Task SeedTenantRolesAsync()
    {
        await TenantDb.Insertable(new[]
        {
            new TenantRoleEntity { Id = AdminRoleId, Name = "系统管理员", Code = AdminRoleCode, },
            new TenantRoleEntity { Id = CustomRoleId, Name = "自定义角色", Code = CustomRoleCode, },
        }).ExecuteCommandAsync();
    }

    public async Task SeedRoleMenuAsync(long roleId, long menuId, long rowId)
    {
        await TenantDb.Insertable(new TenantRoleMenuEntity
        {
            Id = rowId,
            RoleId = roleId,
            MenuId = menuId,
        }).ExecuteCommandAsync();
    }

    public async Task<int> CountRoleMenusAsync(long roleId)
    {
        return await TenantDb.Queryable<TenantRoleMenuEntity>().CountAsync(u => u.RoleId == roleId);
    }

    public async Task<List<long>> GetRoleMenuIdsAsync(long roleId)
    {
        return await TenantDb.Queryable<TenantRoleMenuEntity>()
                             .Where(u => u.RoleId == roleId)
                             .Select(u => u.MenuId)
                             .ToListAsync();
    }

    public async Task<SystemTenantMenuSyncStateEntity?> GetStateAsync()
    {
        return await MainDb.Queryable<SystemTenantMenuSyncStateEntity>()
                           .Filter(null, true)
                           .FirstAsync(u => u.Id == TenantId);
    }

    /// <summary>
    /// 菜单 key → 稳定种子 Id（与 global_menu seed 的 StableSeedId.Menu 同源）。
    /// </summary>
    public static long MenuIdOf(string key)
    {
        return StableSeedId.Menu(key);
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

    private static void ResetMenuSeedCatalogDbCache()
    {
        var flags = global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static;
        typeof(MenuSeedCatalog).GetField("_dbMenus", flags)!.SetValue(null, null);
        typeof(MenuSeedCatalog).GetField("_dbDefinitions", flags)!.SetValue(null, null);
        typeof(MenuSeedCatalog).GetField("_dbDefinitionsByKey", flags)!.SetValue(null, null);
    }

    private DbConnectionConfig CreateConfig(string configId)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_tms_{configId}_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);
        var config = new DbConnectionConfig
        {
            ConfigId = configId,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            IsAutoCloseConnection = true,
            DbSettings = new DbSettings { EnableInitDb = true, },
            TableSettings = new TableSettings { EnableInitTable = true, EnableIncreTable = false, },
            SeedSettings = new SeedSettings { EnableInitSeed = false, EnableIncreSeed = false, },
        };
        SqlSugarSetup.SetDbConfig(config);
        return config;
    }
}
