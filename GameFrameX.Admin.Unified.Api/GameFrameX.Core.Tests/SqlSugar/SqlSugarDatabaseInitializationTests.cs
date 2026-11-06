using GameFrameX.Core.Option;
using GameFrameX.Application.SeedData.Client;
using GameFrameX.Entity.Business.Device;
using GameFrameX.Entity.Business.Game.Release;
using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Tests.System;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Application.Platform.TimeJob.GameTimeJob.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Business.ObjectStorage;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

[Collection("MenuSeedCatalog")]
public class SqlSugarDatabaseInitializationTests : IDisposable
{
    private static readonly Type[] InitializationTypes =
    {
        typeof(SystemUserEntity),
        typeof(UserEntity),
        typeof(GlobalMenuEntity),
        typeof(SystemConfigEntity),
        typeof(GlobalConfigEntity),
        typeof(GlobalDictionaryTypeEntity),
        typeof(GlobalDictionaryDataEntity),
        typeof(GlobalChannelDefinitionEntity),
        typeof(ChannelDefinitionEntity),
        typeof(GameGoodsEntity),
        typeof(SystemLogVisitEntity),
        typeof(SystemLogOperationEntity),
        typeof(SystemLogVisitArchiveEntity),
        typeof(SystemLogOperationArchiveEntity),
        typeof(SystemLogDiffEntity),
        typeof(LogVisitEntity),
        typeof(LogOperationEntity),
        typeof(LogDiffEntity),
    };

    private readonly List<string> _dbPaths = new();

    [Fact]
    public void InitDatabase_SeedsBusinessDefaultsOnce_AndKeepsOperatorChanges()
    {
        SqlSugarCache.Configure(new FakeCache());
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");
            config.SeedSettings.EnableInitSeed = true;
            var types = new[] { typeof(GlobalMenuEntity), typeof(GlobalMenuSeedData), typeof(ChannelDefinitionEntity),
                typeof(BusinessChannelSeedData), typeof(GameAppDeviceGroupEntity), typeof(GameAppDeviceGroupSeedData),
                typeof(GameAppVersionDefinitionEntity), typeof(GameAppVersionDefinitionSeedData) };
            SqlSugarSetup.InitDatabase(scope, config, types);
            var database = scope.GetConnectionScope("main");
            var channel = Assert.Single(database.Queryable<ChannelDefinitionEntity>().ToList());
            Assert.Equal("business-default", channel.Code);
            Assert.True(channel.IsDefault);
            Assert.True(channel.IsEnabled);
            Assert.Single(database.Queryable<GameAppDeviceGroupEntity>().ToList());
            Assert.Equal(new[] { "1.0.0", "default" }, database.Queryable<GameAppVersionDefinitionEntity>().OrderBy(row => row.Name).Select(row => row.Name).ToArray());
            database.Updateable<ChannelDefinitionEntity>().SetColumns(row => new ChannelDefinitionEntity
                { Name = "运维修改", IsEnabled = false, IsDeleted = true }).Where(row => row.Id == channel.Id).ExecuteCommand();
            SqlSugarSetup.InitDatabase(scope, config, types);
            var actual = Assert.Single(database.Queryable<ChannelDefinitionEntity>().Filter(null, true).ToList());
            Assert.Equal("运维修改", actual.Name);
            Assert.False(actual.IsEnabled);
            Assert.True(actual.IsDeleted);
        }
    }

    [Fact]
    public void InitDatabase_OnlySeedsSuperAdmin_AndPreservesExistingOperationsUsers()
    {
        SqlSugarCache.Configure(new FakeCache());
        using (var scope = CreateScope("main"))
        {
            var config = CreateConfig("main");
            config.SeedSettings.EnableInitSeed = true;
            var userSeeds = typeof(SystemUserSeedData).Assembly.GetTypes().Where(type => !type.IsAbstract
                && type.GetInterfaces().Any(contract => contract.IsGenericType
                    && contract.GetGenericTypeDefinition() == typeof(ISqlSugarEntitySeedData<>)
                    && new[] { typeof(UserEntity), typeof(SystemUserEntity) }.Contains(contract.GetGenericArguments()[0]))).ToArray();
            var types = userSeeds.Concat(new[] { typeof(SystemUserEntity), typeof(UserEntity), typeof(GlobalMenuEntity), typeof(GlobalMenuSeedData) }).ToArray();
            SqlSugarSetup.InitDatabase(scope, config, types);
            var database = scope.GetConnectionScope("main");
            Assert.Equal("superadmin", Assert.Single(database.Queryable<SystemUserEntity>().ToList()).Account);
            Assert.Empty(database.Queryable<UserEntity>().ToList());
            var existing = new UserEntity { Id = 5001, Account = "existing-operator", Password = "existing-hash" };
            foreach (var property in typeof(UserEntity).GetProperties().Where(property => property.PropertyType == typeof(string) && property.CanWrite))
            {
                if (property.GetValue(existing) == null)
                {
                    property.SetValue(existing, string.Empty);
                }
            }

            database.Insertable(existing).ExecuteCommand();
            SqlSugarSetup.InitDatabase(scope, config, types);
            Assert.Equal("existing-hash", Assert.Single(database.Queryable<UserEntity>().ToList()).Password);
        }
    }

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
            var types = new[] { typeof(GlobalMenuEntity), typeof(GlobalMenuSeedData), typeof(ObjectStorageConfigEntity) };
            SqlSugarSetup.InitDatabase(scope, config, types);
            var database = scope.GetConnectionScope("main");
            var initial = database.Queryable<ObjectStorageConfigEntity>().Single();
            Assert.Equal("local", initial.Provider);
            Assert.True(initial.IsEnabled);
            database.Updateable<ObjectStorageConfigEntity>()
                .SetColumns(row => new ObjectStorageConfigEntity { IsEnabled = enabled, IsDeleted = deleted, Remark = "保留运维配置" })
                .Where(row => row.Id == initial.Id).ExecuteCommand();

            SqlSugarSetup.InitDatabase(scope, config, types);
            var existing = database.Queryable<ObjectStorageConfigEntity>().Filter(null, true).ToList();
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
        Assert.True(TableExists(mainDb, typeof(UserEntity)));
        Assert.True(TableExists(mainDb, typeof(ChannelDefinitionEntity)));
        Assert.True(TableExists(mainDb, typeof(GameGoodsEntity)));
        Assert.True(TableExists(mainDb, typeof(SystemLogVisitEntity)));
        Assert.True(TableExists(mainDb, typeof(SystemLogOperationEntity)));
        Assert.True(TableExists(mainDb, typeof(SystemLogVisitArchiveEntity)));
        Assert.True(TableExists(mainDb, typeof(SystemLogOperationArchiveEntity)));
        Assert.True(TableExists(mainDb, typeof(SystemLogDiffEntity)));
        Assert.True(TableExists(mainDb, typeof(LogVisitEntity)));
        Assert.True(TableExists(mainDb, typeof(LogOperationEntity)));
        Assert.True(TableExists(mainDb, typeof(LogDiffEntity)));
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
        var allowColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.AllowAdminWhenSuperAdminOnly));
        var isEnabledColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.IsEnabled));
        var isDeletedColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.IsDeleted));
        var createdIdColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.CreatedId));
        var createdTimeColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.CreatedTime));
        var createdNameColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.CreatedName));
        var updateCountColumn = ColumnName(mainDb, typeof(GlobalMenuEntity), nameof(GlobalMenuEntity.UpdateCount));
        mainDb.Ado.ExecuteCommand(
            $"INSERT INTO {tableName} ({idColumn}, {pidColumn}, {keyColumn}, {titleColumn}, {typeColumn}, {isHideColumn}, {isIframeColumn}, {isKeepAliveColumn}, {isAffixColumn}, {orderNoColumn}, {superAdminOnlyColumn}, {allowColumn}, {isEnabledColumn}, {isDeletedColumn}, {createdIdColumn}, {createdTimeColumn}, {createdNameColumn}, {updateCountColumn}) " +
            "VALUES (@legacyId, @pid, @legacyKey, @legacyTitle, @type, @falseValue, @falseValue, @trueValue, @falseValue, @orderNo, @falseValue, @falseValue, @trueValue, @falseValue, @createdId, @createdTime, @createdName, @updateCount), " +
            "(@autoId, @pid, @autoKey, @autoTitle, @type, @trueValue, @falseValue, @trueValue, @falseValue, @orderNo, @falseValue, @falseValue, @trueValue, @falseValue, @createdId, @createdTime, @createdName, @updateCount)",
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
            new SugarParameter("@falseValue", false));

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

        var sql = tenantDb.Queryable<GameTimeJobEntity>()
                          .Select<GameTimeJobResponse>()
                          .ToSql();

        Assert.Contains("game_time_job_entity", sql.Key, StringComparison.Ordinal);
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
