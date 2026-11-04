using GameFrameX.Core.Option;
using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Tests.System;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Application.Game.TimeJob.GameTimeJob.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Tenant.ObjectStorage;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

[Collection("MenuSeedCatalog")]
public class SqlSugarDatabaseInitializationTests : IDisposable
{
    private static readonly Type[] InitializationTypes =
    {
        typeof(SystemUserEntity),
        typeof(TenantUserEntity),
        typeof(GlobalMenuEntity),
        typeof(SystemConfigEntity),
        typeof(GlobalConfigEntity),
        typeof(GlobalDictionaryTypeEntity),
        typeof(GlobalDictionaryDataEntity),
        typeof(GlobalChannelDefinitionEntity),
        typeof(TenantChannelDefinitionEntity),
        typeof(TenantGameGoodsEntity),
        typeof(SystemLogVisitEntity),
        typeof(SystemLogOperationEntity),
        typeof(SystemLogDiffEntity),
        typeof(TenantLogVisitEntity),
        typeof(TenantLogOperationEntity),
        typeof(TenantLogDiffEntity),
    };

    private readonly List<string> _dbPaths = new();

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void InitDatabase_SeedsLocalStorageOnceAndPreservesExistingConfiguration(bool enabled, bool deleted)
    {
        SqlSugarCache.Configure(new FakeCache());
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");
            config.SeedSettings.EnableInitSeed = true;
            var types = new[] { typeof(GlobalMenuEntity), typeof(GlobalMenuSeedData), typeof(TenantObjectStorageConfigEntity) };
            SqlSugarSetup.InitDatabase(scope, config, types);
            var database = scope.GetConnectionScope("main");
            var initial = database.Queryable<TenantObjectStorageConfigEntity>().Single();
            Assert.Equal("local", initial.Provider);
            Assert.True(initial.IsEnabled);
            database.Updateable<TenantObjectStorageConfigEntity>()
                .SetColumns(row => new TenantObjectStorageConfigEntity { IsEnabled = enabled, IsDeleted = deleted, Remark = "保留运维配置" })
                .Where(row => row.Id == initial.Id).ExecuteCommand();

            SqlSugarSetup.InitDatabase(scope, config, types);
            var existing = database.Queryable<TenantObjectStorageConfigEntity>().Filter(null, true).ToList();
            var actual = Assert.Single(existing);
            Assert.Equal(initial.Id, actual.Id);
            Assert.Equal(enabled, actual.IsEnabled);
            Assert.Equal(deleted, actual.IsDeleted);
            Assert.Equal("保留运维配置", actual.Remark);
        }
    }

    [Fact]
    public void InitDatabase_WithDefaultIncrementalSettings_CreatesEveryManagedEntity()
    {
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");
            config.TableSettings.EnableIncreTable = true;
            var entities = typeof(SystemUserEntity).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && type.IsDefined(typeof(SugarTable), false)).ToArray();
            SqlSugarSetup.InitDatabase(scope, config, entities);
            var database = scope.GetConnectionScope("main");
            var tables = database.DbMaintenance.GetTableInfoList(false).Select(table => table.Name).ToArray();
            foreach (var entity in entities)
            {
                var name = TableName(database, entity);
                Assert.Contains(tables, table => table == name || table.StartsWith(name + "_", StringComparison.Ordinal));
            }
        }
    }

    [Fact]
    public void InitDatabase_ForMainConfig_CreatesAllManagedTables()
    {
        using var scope = CreateScope("main");
        var config = CreateConfig("main");

        SqlSugarSetup.InitDatabase(scope, config, InitializationTypes);

        var mainDb = scope.GetConnectionScope("main");
        Assert.True(TableExists(mainDb, typeof(SystemUserEntity)));
        Assert.True(TableExists(mainDb, typeof(GlobalMenuEntity)));
        Assert.True(TableExists(mainDb, typeof(SystemConfigEntity)));
        Assert.True(TableExists(mainDb, typeof(GlobalConfigEntity)));
        Assert.True(TableExists(mainDb, typeof(GlobalDictionaryTypeEntity)));
        Assert.True(TableExists(mainDb, typeof(GlobalDictionaryDataEntity)));
        Assert.True(TableExists(mainDb, typeof(GlobalChannelDefinitionEntity)));
        Assert.True(TableExists(mainDb, typeof(TenantUserEntity)));
        Assert.True(TableExists(mainDb, typeof(TenantChannelDefinitionEntity)));
        Assert.True(TableExists(mainDb, typeof(TenantGameGoodsEntity)));
        Assert.True(TableExists(mainDb, typeof(SystemLogVisitEntity)));
        Assert.True(TableExists(mainDb, typeof(SystemLogOperationEntity)));
        Assert.True(TableExists(mainDb, typeof(SystemLogDiffEntity)));
        Assert.True(TableExists(mainDb, typeof(TenantLogVisitEntity)));
        Assert.True(TableExists(mainDb, typeof(TenantLogOperationEntity)));
        Assert.True(TableExists(mainDb, typeof(TenantLogDiffEntity)));
        Assert.Equal("system_log_operation_entity", TableName(mainDb, typeof(SystemLogOperationEntity)));
        Assert.Equal("system_user_entity", TableName(mainDb, typeof(SystemUserEntity)));
        Assert.Equal("global_menu_entity", TableName(mainDb, typeof(GlobalMenuEntity)));
        Assert.Equal("system_config_entity", TableName(mainDb, typeof(SystemConfigEntity)));
        Assert.Equal("global_config_entity", TableName(mainDb, typeof(GlobalConfigEntity)));
        Assert.Equal("global_dictionary_type_entity", TableName(mainDb, typeof(GlobalDictionaryTypeEntity)));
        Assert.Equal("global_dictionary_data_entity", TableName(mainDb, typeof(GlobalDictionaryDataEntity)));
        Assert.Equal("global_channel_definition_entity", TableName(mainDb, typeof(GlobalChannelDefinitionEntity)));
    }

    [Fact]
    public void MenuSeedCatalog_LoadFromDb_KeepsRuntimeAutoPermissionMenusAsDbSource()
    {
        using var scope = CreateScope("main");
        var config = CreateConfig("main");

        SqlSugarSetup.InitDatabase(scope, config, new[] { typeof(GlobalMenuEntity), });

        var mainDb = scope.GetConnectionScope("main");
        var legacyId = -1001L;
        var autoId = -1002L;
        var tableName = TableName(mainDb, typeof(GlobalMenuEntity));
        var idColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.Id));
        var pidColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.Pid));
        var keyColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.Key));
        var titleColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.Title));
        var typeColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.Type));
        var isHideColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.IsHide));
        var isIframeColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.IsIframe));
        var isKeepAliveColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.IsKeepAlive));
        var isAffixColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.IsAffix));
        var orderNoColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.OrderNo));
        var superAdminOnlyColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.SuperAdminOnly));
        var allowTenantColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.AllowTenantWhenSuperAdminOnly));
        var isEnabledColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.IsEnabled));
        var isDeletedColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.IsDeleted));
        var createdIdColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.CreatedId));
        var createdTimeColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.CreatedTime));
        var createdNameColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.CreatedName));
        var updateCountColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.UpdateCount));
        var appScopeColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.AppScope));
        mainDb.Ado.ExecuteCommand(
            $"INSERT INTO {tableName} ({idColumn}, {pidColumn}, {keyColumn}, {titleColumn}, {typeColumn}, {isHideColumn}, {isIframeColumn}, {isKeepAliveColumn}, {isAffixColumn}, {orderNoColumn}, {superAdminOnlyColumn}, {allowTenantColumn}, {isEnabledColumn}, {isDeletedColumn}, {createdIdColumn}, {createdTimeColumn}, {createdNameColumn}, {updateCountColumn}, {appScopeColumn}) " +
            "VALUES (@legacyId, @pid, @legacyKey, @legacyTitle, @type, @falseValue, @falseValue, @trueValue, @falseValue, @orderNo, @falseValue, @falseValue, @trueValue, @falseValue, @createdId, @createdTime, @createdName, @updateCount, @appScope), " +
            "(@autoId, @pid, @autoKey, @autoTitle, @type, @trueValue, @falseValue, @trueValue, @falseValue, @orderNo, @falseValue, @falseValue, @trueValue, @falseValue, @createdId, @createdTime, @createdName, @updateCount, @appScope)",
            new SugarParameter("@legacyId", legacyId),
            new SugarParameter("@pid", 0),
            new SugarParameter("@legacyKey", "legacy:menu"),
            new SugarParameter("@legacyTitle", "legacy"),
            new SugarParameter("@autoId", autoId),
            new SugarParameter("@autoKey", "permission:auto:test:page"),
            new SugarParameter("@autoTitle", "auto"),
            new SugarParameter("@type", (int)MenuTypeEnum.Button),
            new SugarParameter("@orderNo", 9999),
            new SugarParameter("@createdId", 0),
            new SugarParameter("@createdTime", DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            new SugarParameter("@createdName", "test"),
            new SugarParameter("@updateCount", 0),
            new SugarParameter("@trueValue", true),
            new SugarParameter("@falseValue", false),
            new SugarParameter("@appScope", (int)MenuAppScopeEnum.Console));

        ResetMenuSeedCatalogDbCache();
        MenuSeedCatalog.LoadFromDb(mainDb);

        mainDb.QueryFilter.ClearAndBackup();
        try
        {
            var legacy = mainDb.Queryable<GlobalMenuEntity>().Single(u => u.Id == legacyId);
            var auto = mainDb.Queryable<GlobalMenuEntity>().Single(u => u.Id == autoId);

            Assert.False(legacy.IsDeleted);
            Assert.True(legacy.IsEnabled);
            Assert.False(auto.IsDeleted);
            Assert.True(auto.IsEnabled);
            Assert.Contains(MenuSeedCatalog.Menus, u => u.Id == legacyId);
            Assert.Contains(MenuSeedCatalog.Menus, u => u.Id == autoId);
        }
        finally
        {
            mainDb.QueryFilter.Restore();
        }
    }

    [Fact]
    public void ProjectionDto_InheritingEntity_DoesNotRequirePhysicalOwnership()
    {
        using var scope = CreateScope("tenant_1003");
        var tenantDb = scope.GetConnectionScope("tenant_1003");

        var sql = tenantDb.Queryable<TenantGameTimeJobEntity>()
                          .Select<TenantGameTimeJobEntityResponse>()
                          .ToSql();

        Assert.Contains("tenant_game_time_job_entity", sql.Key, StringComparison.Ordinal);
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
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_admin_{configId}_{Guid.NewGuid():N}.db");
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
                EnableInitSeed = false,
                EnableIncreSeed = false,
            },
        };
        SqlSugarSetup.SetDbConfig(config);
        return config;
    }

    private static bool TableExists(SqlSugarScopeProvider db, Type entityType)
    {
        return db.DbMaintenance.IsAnyTable(TableName(db, entityType), false);
    }

    private static string TableName(SqlSugarScopeProvider db, Type entityType)
    {
        var currentTableName = db.EntityMaintenance.GetEntityInfo(entityType).DbTableName;
        return SqlSugarEntityBoundary.GetPhysicalTableName(entityType, currentTableName);
    }

    private static string ColumnName(SqlSugarScopeProvider db, Type entityType, string propertyName)
    {
        return db.EntityMaintenance.GetEntityInfo(entityType).Columns.Single(u => u.PropertyName == propertyName).DbColumnName;
    }

    private static void ResetMenuSeedCatalogDbCache()
    {
        var flags = global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static;
        typeof(MenuSeedCatalog).GetField("_dbMenus", flags)!.SetValue(null, null);
        typeof(MenuSeedCatalog).GetField("_dbDefinitions", flags)!.SetValue(null, null);
        typeof(MenuSeedCatalog).GetField("_dbDefinitionsByKey", flags)!.SetValue(null, null);
    }
}
