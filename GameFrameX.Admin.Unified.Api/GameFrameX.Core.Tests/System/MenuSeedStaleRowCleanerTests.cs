using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using SqlSugar;
using Xunit;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// MenuSeedStaleRowCleaner 单测（C331 / GFX-741）：global_menu 种子演化残留行启动自愈清理。
/// <para>复现形态：实体改名（D1 禁简写）后旧 Key 行不再被种子 upsert 命中而留存，与新 Key 行同 Permission 并存，
/// 导致 ValidateDbMenus 启动崩溃；清理后有效行恒等于「当前种子 ∪ permission:auto:* 运行期行」。</para>
/// <para>测试客户端沿用 <see cref="MenuSeedCatalogTestData"/> 的裸 SqlSugarClient 惯例（不带生产
/// ConfigureExternalServices）：SqlSugar 实体元数据静态缓存按类型共享，混用两种客户端配置会因表名/可空映射
/// 不一致产生测试顺序耦合（详见 notes.md）。</para>
/// </summary>
[Collection("MenuSeedCatalog")]
public class MenuSeedStaleRowCleanerTests
{
    [Fact]
    public void Clean_SoftDeletesStaleRows_KeepsSeedAndAutoRows_AndReparentsDanglingAuto()
    {
        var db = CreateClient(out var dbPath);
        try
        {
            var parentRoute = Normalize(new GlobalMenuEntity
            {
                Id = StableSeedId.Menu("route:demo"),
                Key = "route:demo",
                Pid = 0,
                Type = MenuTypeEnum.Menu,
                Title = "演示页面",
            });
            var seedButton = Normalize(new GlobalMenuEntity
            {
                Id = StableSeedId.Menu("demoEntity:page"),
                Key = "demoEntity:page",
                Pid = parentRoute.Id,
                Type = MenuTypeEnum.Button,
                Permission = "demoEntity:page",
                Title = "查询",
            });
            var staleButton = Normalize(new GlobalMenuEntity
            {
                Id = StableSeedId.Menu("demoOldEntity:page"),
                Key = "demoOldEntity:page",
                Pid = parentRoute.Id,
                Type = MenuTypeEnum.Button,
                Permission = "demoEntity:page",
                Title = "查询(旧)",
            });
            var autoButton = Normalize(new GlobalMenuEntity
            {
                Id = StableSeedId.Menu("permission:auto:demo:page"),
                Key = "permission:auto:demo:page",
                Pid = staleButton.Id,
                Type = MenuTypeEnum.Button,
                Title = "自动权限:api/demo/page",
            });
            db.Insertable(new List<GlobalMenuEntity> { parentRoute, seedButton, staleButton, autoButton, }).ExecuteCommand();

            var seedIdsByKey = new Dictionary<string, long>(StringComparer.Ordinal)
            {
                { "route:demo", StableSeedId.Menu("route:demo") },
                { "demoEntity:page", StableSeedId.Menu("demoEntity:page") },
            };

            var removedKeys = MenuSeedStaleRowCleaner.RemoveStaleRows(db, seedIdsByKey);

            Assert.Equal(new[] { "demoOldEntity:page", }, removedKeys);

            var rows = db.Queryable<GlobalMenuEntity>().ToList();
            Assert.True(rows.Single(u => u.Key == "demoOldEntity:page").IsDeleted);
            Assert.False(rows.Single(u => u.Key == "route:demo").IsDeleted);
            Assert.False(rows.Single(u => u.Key == "demoEntity:page").IsDeleted);
            var auto = rows.Single(u => u.Key == "permission:auto:demo:page");
            Assert.False(auto.IsDeleted);
            Assert.Equal(0, auto.Pid);

            // 幂等：残留已软删后再次清理不再命中任何行
            var secondRunKeys = MenuSeedStaleRowCleaner.RemoveStaleRows(db, seedIdsByKey);
            Assert.Empty(secondRunKeys);
        }
        finally
        {
            DisposeClient(db, dbPath);
        }
    }

    [Fact]
    public void Clean_PureSeedDatabase_RemovesNothing()
    {
        var db = CreateClient(out var dbPath);
        try
        {
            var seedRows = GlobalMenuSeedData.BuildData().Select(Normalize).ToList();
            db.Insertable(seedRows).ExecuteCommand();
            var seedIdsByKey = BuildSeedIdsByKey(seedRows);

            var removedKeys = MenuSeedStaleRowCleaner.RemoveStaleRows(db, seedIdsByKey);

            Assert.Empty(removedKeys);
            Assert.Equal(seedRows.Count, db.Queryable<GlobalMenuEntity>().Where(u => u.IsDeleted == false).Count());
        }
        finally
        {
            DisposeClient(db, dbPath);
        }
    }

    [Fact]
    public void Clean_ThenLoadFromDb_HealsLegacyDuplicatePermissionRows_GFX741()
    {
        var db = CreateClient(out var dbPath);
        try
        {
            var seedRows = GlobalMenuSeedData.BuildData().Select(Normalize).ToList();
            db.Insertable(seedRows).ExecuteCommand();
            var seedIdsByKey = BuildSeedIdsByKey(seedRows);

            // 复活 GFX-741 脏数据形态：旧简写 Key 子树（1 route + 1 按钮样本）与新全称 Key 行同 Permission 并存
            var legacyRoute = Normalize(new GlobalMenuEntity
            {
                Id = StableSeedId.Menu("route:tenantAdCallbackConfigEntity"),
                Key = "route:tenantAdCallbackConfigEntity",
                Pid = 0,
                Type = MenuTypeEnum.Menu,
                Title = "广告回调配置(旧)",
            });
            var legacyPage = Normalize(new GlobalMenuEntity
            {
                Id = StableSeedId.Menu("tenantAdCallbackConfigEntity:page"),
                Key = "tenantAdCallbackConfigEntity:page",
                Pid = legacyRoute.Id,
                Type = MenuTypeEnum.Button,
                Permission = "tenantAdvertisementCallbackConfigEntity:page",
                Title = "查询(旧)",
            });
            db.Insertable(new List<GlobalMenuEntity> { legacyRoute, legacyPage, }).ExecuteCommand();

            // 修复前：ValidateDbMenus 因 Permission 重复拒绝加载
            var before = Assert.Throws<InvalidOperationException>(() => MenuSeedCatalog.LoadFromDb(db));
            Assert.Contains("global_menu.Permission 重复: tenantAdvertisementCallbackConfigEntity:page", before.Message);

            var removedKeys = MenuSeedStaleRowCleaner.RemoveStaleRows(db, seedIdsByKey);
            Assert.Equal(new[] { "route:tenantAdCallbackConfigEntity", "tenantAdCallbackConfigEntity:page", }, removedKeys.OrderBy(u => u, StringComparer.Ordinal).ToArray());

            // 清理后加载通过，且该 Permission 只剩一条有效定义（种子全称 Key 行）
            MenuSeedCatalog.LoadFromDb(db);
            var permissionRows = MenuSeedCatalog.AllDefinitions
                                                .Where(u => u.Permission == "tenantAdvertisementCallbackConfigEntity:page")
                                                .ToList();
            var row = Assert.Single(permissionRows);
            Assert.Equal("tenantAdvertisementCallbackConfigEntity:page", row.Key);
        }
        finally
        {
            DisposeClient(db, dbPath);
        }
    }

    [Fact]
    public void Clean_ThenLoadFromDb_HealsDuplicateSeedKeyWrongIdRows_IdDrift()
    {
        var db = CreateClient(out var dbPath);
        try
        {
            var seedRows = GlobalMenuSeedData.BuildData().Select(Normalize).ToList();
            db.Insertable(seedRows).ExecuteCommand();
            var seedIdsByKey = BuildSeedIdsByKey(seedRows);

            // 复现 Id 漂移脏数据形态：route:dashboard 存在旧 19 位雪花 Id 行（StableSeedId 紧凑布局切换前残留），
            // 与种子权威 Id 行同 Key 并存 → ValidateDbMenus 因「global_menu.Key 重复」拒绝加载
            var legacyDashboard = Normalize(new GlobalMenuEntity
            {
                Id = 2040657799165320518,
                Key = "route:dashboard",
                Pid = 0,
                Type = MenuTypeEnum.Dir,
                Name = "dashboard",
                Title = "工作台(旧)",
            });
            db.Insertable(legacyDashboard).ExecuteCommand();

            var before = Assert.Throws<InvalidOperationException>(() => MenuSeedCatalog.LoadFromDb(db));
            Assert.Contains("global_menu.Key 重复: route:dashboard", before.Message);
            Assert.Contains("2040657799165320518/route:dashboard", before.Message);
            Assert.Contains("34586757242182/route:dashboard", before.Message);

            var removedKeys = MenuSeedStaleRowCleaner.RemoveStaleRows(db, seedIdsByKey);
            Assert.Equal(new[] { "route:dashboard", }, removedKeys);

            // 清理后加载通过，route:dashboard 只剩种子权威 Id 行
            MenuSeedCatalog.LoadFromDb(db);
            var dashboard = MenuSeedCatalog.Menus.Single(u => u.Key == "route:dashboard");
            Assert.Equal(StableSeedId.Menu("route:dashboard"), dashboard.Id);
            Assert.Equal((long)34586757242182, dashboard.Id);
        }
        finally
        {
            DisposeClient(db, dbPath);
        }
    }

    [Fact]
    public void Clean_WrongIdRowWithoutAuthoritativeRow_IsKept()
    {
        var db = CreateClient(out var dbPath);
        try
        {
            // 权威行缺席（如本次启动未执行 upsert）时，同 Key 异 Id 行是该 Key 唯一有效定义，不得误删
            var soleRow = Normalize(new GlobalMenuEntity
            {
                Id = 2040657799165320518,
                Key = "route:demo",
                Pid = 0,
                Type = MenuTypeEnum.Menu,
                Title = "演示页面",
            });
            db.Insertable(soleRow).ExecuteCommand();
            var seedIdsByKey = new Dictionary<string, long>(StringComparer.Ordinal)
            {
                { "route:demo", StableSeedId.Menu("route:demo") },
            };

            var removedKeys = MenuSeedStaleRowCleaner.RemoveStaleRows(db, seedIdsByKey);

            Assert.Empty(removedKeys);
            Assert.False(db.Queryable<GlobalMenuEntity>().Single(u => u.Key == "route:demo").IsDeleted);
        }
        finally
        {
            DisposeClient(db, dbPath);
        }
    }

    [Fact]
    public void LoadFromDb_DuplicatePermissionError_ListsConflictingRowIdsAndKeys()
    {
        var db = CreateClient(out var dbPath);
        try
        {
            var first = Normalize(new GlobalMenuEntity
            {
                Id = 111,
                Key = "demoEntity:page",
                Pid = 0,
                Type = MenuTypeEnum.Button,
                Permission = "demoEntity:page",
                Title = "查询",
            });
            var second = Normalize(new GlobalMenuEntity
            {
                Id = 222,
                Key = "demoOldEntity:page",
                Pid = 0,
                Type = MenuTypeEnum.Button,
                Permission = "demoEntity:page",
                Title = "查询(旧)",
            });
            db.Insertable(new List<GlobalMenuEntity> { first, second, }).ExecuteCommand();

            var error = Assert.Throws<InvalidOperationException>(() => MenuSeedCatalog.LoadFromDb(db));

            Assert.Contains("111/demoEntity:page", error.Message);
            Assert.Contains("222/demoOldEntity:page", error.Message);
        }
        finally
        {
            DisposeClient(db, dbPath);
        }
    }

    private static SqlSugarClient CreateClient(out string dbPath)
    {
        dbPath = Path.Combine(Path.GetTempPath(), $"gfx_menu_cleaner_{Guid.NewGuid():N}.db");
        var db = new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
        });
        db.CodeFirst.InitTables<GlobalMenuEntity>();
        return db;
    }

    /// <summary>
    /// 裸客户端（无生产 ConfigureExternalServices）建表列默认 NOT NULL，插入行须全量补齐字段
    /// （同 <see cref="MenuSeedCatalogTestData.BuildSampleMenus"/> 的映射方式）。
    /// </summary>
    private static GlobalMenuEntity Normalize(GlobalMenuEntity source)
    {
        return new GlobalMenuEntity
        {
            Id = source.Id,
            Pid = source.Pid,
            Type = source.Type,
            Key = source.Key ?? string.Empty,
            Name = source.Name ?? string.Empty,
            Path = source.Path ?? string.Empty,
            Component = source.Component ?? string.Empty,
            Redirect = source.Redirect ?? string.Empty,
            Permission = source.Permission ?? string.Empty,
            Title = source.Title,
            TitleCode = source.TitleCode ?? string.Empty,
            Icon = source.Icon ?? string.Empty,
            IsIframe = source.IsIframe,
            OutLink = source.OutLink ?? string.Empty,
            IsHide = source.IsHide,
            IsKeepAlive = source.IsKeepAlive,
            IsAffix = source.IsAffix,
            OrderNo = source.OrderNo,
            Remark = source.Remark ?? string.Empty,
            SuperAdminOnly = source.SuperAdminOnly,
            SyncRoles = source.SyncRoles,
            AllowTenantWhenSuperAdminOnly = source.AllowTenantWhenSuperAdminOnly,
            IsEnabled = source.IsEnabled ?? true,
            IsDeleted = false,
            TenantId = 0,
            CreatedId = 0,
            CreatedName = "test",
            CreatedTime = source.CreatedTime,
            UpdatedId = 0,
            UpdatedName = "test",
            UpdateTime = source.UpdateTime ?? source.CreatedTime,
            UpdateCount = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            DeleteTime = 0,
        };
    }


    /// <summary>
    /// 构建种子 Key → 权威 Id 映射（同 SqlSugarSetup 主库清理入参的构建口径：Key 非空、重复 Key 取首行）。
    /// </summary>
    private static Dictionary<string, long> BuildSeedIdsByKey(IReadOnlyList<GlobalMenuEntity> seedRows)
    {
        return seedRows
               .Where(u => !string.IsNullOrWhiteSpace(u.Key))
               .GroupBy(u => u.Key!, StringComparer.Ordinal)
               .ToDictionary(u => u.Key, u => u.First().Id, StringComparer.Ordinal);
    }

    private static void DisposeClient(SqlSugarClient db, string dbPath)
    {
        db.Dispose();
        foreach (var suffix in new[] { "", "-wal", "-shm", })
        {
            var file = dbPath + suffix;
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
