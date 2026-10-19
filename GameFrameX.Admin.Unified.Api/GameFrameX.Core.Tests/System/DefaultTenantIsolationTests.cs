using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Option;
using GameFrameX.Core.Security;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.Tenant.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.Device;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Entity.Tenant.Game.ServerConfig;
using GameFrameX.Entity.Tenant.ObjectStorage;
using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Entity.Tenant.Game.Resource;
using Npgsql;
using SqlSugar;
using GameFrameX.Foundation.Utility.DistributedSystem.Snowflake;
using GameFrameX.Core.Extension;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.System;

[Collection("MenuSeedCatalog")]
public class DefaultTenantIsolationTests : IDisposable
{
    private static readonly Type[] DefaultTenantBootstrapTypes =
    {
        typeof(TenantOrganizationEntity),
        typeof(TenantRoleEntity),
        typeof(TenantPositionEntity),
        typeof(TenantUserEntity),
        typeof(TenantUserRoleEntity),
        typeof(TenantRoleMenuEntity),
        typeof(TenantObjectStorageConfigEntity),
        typeof(TenantGameAppEntity),
        typeof(TenantChannelDefinitionEntity),
        typeof(TenantGameAppDeviceGroupEntity),
        typeof(TenantGameRedeemCodeChannelEntity),
        typeof(TenantGameAppAssetPackageEntity),
        typeof(TenantGameAppVersionDefinitionEntity),
        typeof(TenantGameAppRemoteConfigKeyEntity),
        typeof(TenantGameAppRemoteConfigModeEntity),
        typeof(TenantGameAppServerConfigEntity),
    };

    private readonly List<string> _dbPaths = new();

    public DefaultTenantIsolationTests()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
    }

    [Fact]
    public void MainConfigId_IsNotDefaultTenantId()
    {
        Assert.NotEqual(SqlSugarConst.MainConfigId, TenantDefaultDataTemplates.MainTenantId.ToString());
    }

    [Fact]
    public void MainUserSeed_ContainsOnlySuperAdmin()
    {
        var users = SystemUserSeedData.BuildData("test-password").ToList();

        Assert.Single(users);
        Assert.Equal("superadmin", users[0].Account);
        Assert.Equal(AccountTypeEnum.SuperAdmin, users[0].AccountType);
        Assert.DoesNotContain(users, u => u.Account == "admin");
    }

    [Fact]
    public void MainDatabaseBootstrap_DoesNotIncludeTenantIdentityTablesExceptPlatformUser()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(SystemUserEntity)));
        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantUserEntity)));
        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantOrganizationEntity)));
        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantRoleEntity)));
        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantPositionEntity)));
        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantUserRoleEntity)));
        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantRoleMenuEntity)));
    }

    [Fact]
    public void DefaultTenantDatabaseSeeder_CreatesTenantAdminInTenantDatabase()
    {
        var tenantConfig = CreateConfig(TenantDefaultDataTemplates.MainTenantId.ToString());
        SqlSugarUnifiedConfig.ConfigureBaseSettings(tenantConfig);
        using var tenantScope = new SqlSugarScope(tenantConfig);
        var tenantDb = tenantScope.GetConnectionScope(TenantDefaultDataTemplates.MainTenantId.ToString());
        foreach (var entityType in DefaultTenantBootstrapTypes)
        {
            tenantDb.CodeFirst.InitTables(entityType);
        }

        DefaultTenantDatabaseSeeder.Seed(tenantDb, new SystemTenantEntity
        {
            Id = TenantDefaultDataTemplates.MainTenantId,
            OrganizationId = 1300000000101,
            UserId = 1300000000111,
            TenantType = TenantTypeEnum.Db,
            DbType = tenantConfig.DbType,
            Connection = tenantConfig.ConnectionString,
            ConfigId = tenantConfig.ConfigId.ToString(),
            Remark = "系统默认租户",
            Host = "tenant.test",
        }, "encrypted-password");

        Assert.True(tenantDb.DbMaintenance.IsAnyTable(tenantDb.EntityMaintenance.GetEntityInfo<TenantUserEntity>().DbTableName, false));
        Assert.True(tenantDb.DbMaintenance.IsAnyTable(tenantDb.EntityMaintenance.GetEntityInfo<TenantOrganizationEntity>().DbTableName, false));
        Assert.True(tenantDb.DbMaintenance.IsAnyTable(tenantDb.EntityMaintenance.GetEntityInfo<TenantRoleEntity>().DbTableName, false));
        var admin = tenantDb.Queryable<TenantUserEntity>().First(u => u.Account == "admin");
        Assert.NotNull(admin);
        Assert.Equal(TenantDefaultDataTemplates.MainTenantId, admin.TenantId);
        Assert.Equal(AccountTypeEnum.SystemAdmin, admin.AccountType);
        Assert.Equal("encrypted-password", admin.Password);
        Assert.Equal(TenantDefaultDataTemplates.DefaultAdminPhone, admin.Phone);
        Assert.Equal(TenantDefaultDataTemplates.DefaultAdminEmail, admin.Email);

        var defaultApp = tenantDb.Queryable<TenantGameAppEntity>().Single();
        Assert.Equal(TenantDefaultDataTemplates.MainTenantId, defaultApp.TenantId);
        Assert.Equal("默认应用", defaultApp.Name);
        Assert.True(defaultApp.IsEnabled);
        Assert.False(defaultApp.IsDeleted);
        Assert.Null(defaultApp.AppSecret);
        Assert.False(string.IsNullOrWhiteSpace(defaultApp.AppSecretHash));
        Assert.Equal(AppSecretHasher.Algorithm, defaultApp.AppSecretHashAlgorithm);

        var tenantChannel = tenantDb.Queryable<TenantChannelDefinitionEntity>().Single(u => u.Code == "default");
        Assert.Equal(TenantDefaultDataTemplates.MainTenantId, tenantChannel.TenantId);
        Assert.Equal("默认渠道", tenantChannel.Name);
        Assert.Equal(ChannelScopeEnum.Tenant, tenantChannel.Scope);
        Assert.True(tenantChannel.IsDefault);

        var redeemChannel = tenantDb.Queryable<TenantGameRedeemCodeChannelEntity>().Single(u => u.Name == "default");
        Assert.Equal(TenantDefaultDataTemplates.MainTenantId, redeemChannel.TenantId);
        Assert.Equal(defaultApp.Id, redeemChannel.AppId);
        Assert.Equal("默认兑换码渠道", redeemChannel.Description);

        var deviceGroup = tenantDb.Queryable<TenantGameAppDeviceGroupEntity>().Single(u => u.Name == "default");
        Assert.Equal(TenantDefaultDataTemplates.MainTenantId, deviceGroup.TenantId);
        Assert.Equal("默认缺省分组", deviceGroup.Description);

        var assetPackage = tenantDb.Queryable<TenantGameAppAssetPackageEntity>().Single(u => u.Name == "DefaultPackage");
        Assert.Equal(TenantDefaultDataTemplates.MainTenantId, assetPackage.TenantId);
        Assert.Equal(defaultApp.Id, assetPackage.GameAppId);

        var versionNames = tenantDb.Queryable<TenantGameAppVersionDefinitionEntity>()
                                   .Where(u => u.AppId == defaultApp.Id)
                                   .Select(u => u.Name)
                                   .ToList();
        Assert.Contains("default", versionNames);
        Assert.Contains("1.0.0", versionNames);

        var configKeys = tenantDb.Queryable<TenantGameAppRemoteConfigKeyEntity>().Select(u => u.KeyName).ToList();
        Assert.Contains("maintenance", configKeys);
        Assert.Contains("enableRegister", configKeys);

        var configModes = tenantDb.Queryable<TenantGameAppRemoteConfigModeEntity>().Select(u => u.ModeCode).ToList();
        Assert.Contains("review", configModes);
        Assert.Contains("formal", configModes);
    }

    [Fact]
    public void DefaultTenantDatabaseSeeder_DoesNotOverwriteExistingTenantGameApp()
    {
        var tenantConfig = CreateConfig(TenantDefaultDataTemplates.MainTenantId.ToString());
        SqlSugarUnifiedConfig.ConfigureBaseSettings(tenantConfig);
        using var tenantScope = new SqlSugarScope(tenantConfig);
        var tenantDb = tenantScope.GetConnectionScope(TenantDefaultDataTemplates.MainTenantId.ToString());
        foreach (var entityType in DefaultTenantBootstrapTypes)
        {
            tenantDb.CodeFirst.InitTables(entityType);
        }

        tenantDb.Insertable(new TenantGameAppEntity
        {
            Id = 991001,
            TenantId = TenantDefaultDataTemplates.MainTenantId,
            Name = "租户自定义应用",
#pragma warning disable CS8625 // 测试意图：明文 AppSecret 留空，只设 Hash，模拟生产"明文不入库"路径
            AppSecret = null,
#pragma warning restore CS8625
            AppSecretHash = AppSecretHasher.Hash("tenant-secret"),
            AppSecretHashAlgorithm = AppSecretHasher.Algorithm,
            IsEnabled = true,
            IsDeleted = false,
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        }).ExecuteCommand();

        DefaultTenantDatabaseSeeder.Seed(tenantDb, new SystemTenantEntity
        {
            Id = TenantDefaultDataTemplates.MainTenantId,
            OrganizationId = 1300000000101,
            UserId = 1300000000111,
            TenantType = TenantTypeEnum.Db,
            DbType = tenantConfig.DbType,
            Connection = tenantConfig.ConnectionString,
            ConfigId = tenantConfig.ConfigId.ToString(),
            Remark = "系统默认租户",
            Host = "tenant.test",
        }, "encrypted-password");

        var apps = tenantDb.Queryable<TenantGameAppEntity>().Filter(null, true).ToList();
        Assert.Single(apps);
        Assert.Equal("租户自定义应用", apps[0].Name);
    }

    [Fact]
    public void DefaultTenantDatabaseSeeder_UpsertsDefaultObjectStorageThroughTenantFilters()
    {
        var tenantConfig = CreateConfig(TenantDefaultDataTemplates.MainTenantId.ToString());
        SqlSugarUnifiedConfig.ConfigureBaseSettings(tenantConfig);
        using var tenantScope = new SqlSugarScope(tenantConfig);
        var tenantDb = tenantScope.GetConnectionScope(TenantDefaultDataTemplates.MainTenantId.ToString());
        foreach (var entityType in DefaultTenantBootstrapTypes)
        {
            tenantDb.CodeFirst.InitTables(entityType);
        }

        var existing = DefaultTenantDatabaseSeeder.BuildDefaultObjectStorageConfig(TenantDefaultDataTemplates.MainTenantId);
        existing.Remark = "已有默认本地存储";
        tenantDb.Insertable(existing).ExecuteCommand();
        SqlSugarUnifiedConfig.ConfigureAdminFilters(tenantDb, TenantDefaultDataTemplates.MainTenantId + 1);

        var exception = Record.Exception(() => DefaultTenantDatabaseSeeder.Seed(tenantDb, new SystemTenantEntity
        {
            Id = TenantDefaultDataTemplates.MainTenantId,
            OrganizationId = 1300000000101,
            UserId = 1300000000111,
            TenantType = TenantTypeEnum.Db,
            DbType = tenantConfig.DbType,
            Connection = tenantConfig.ConnectionString,
            ConfigId = tenantConfig.ConfigId.ToString(),
            Remark = "系统默认租户",
            Host = "tenant.test",
        }, "encrypted-password"));

        Assert.Null(exception);
        tenantDb.QueryFilter.ClearAndBackup();
        try
        {
            var configs = tenantDb.Queryable<TenantObjectStorageConfigEntity>().ToList();
            Assert.Single(configs);
            Assert.Equal(existing.Id, configs[0].Id);
        }
        finally
        {
            tenantDb.QueryFilter.Restore();
        }
    }

    [Fact]
    public void DefaultTenantDatabaseSeeder_DoesNotRecreateDeletedTenantGameApp()
    {
        var tenantConfig = CreateConfig(TenantDefaultDataTemplates.MainTenantId.ToString());
        SqlSugarUnifiedConfig.ConfigureBaseSettings(tenantConfig);
        using var tenantScope = new SqlSugarScope(tenantConfig);
        var tenantDb = tenantScope.GetConnectionScope(TenantDefaultDataTemplates.MainTenantId.ToString());
        foreach (var entityType in DefaultTenantBootstrapTypes)
        {
            tenantDb.CodeFirst.InitTables(entityType);
        }

        tenantDb.Insertable(new TenantGameAppEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            TenantId = TenantDefaultDataTemplates.MainTenantId,
            Name = "默认应用",
#pragma warning disable CS8625 // 测试意图：明文 AppSecret 留空，只设 Hash，模拟生产"明文不入库"路径
            AppSecret = null,
#pragma warning restore CS8625
            AppSecretHash = AppSecretHasher.Hash("deleted-secret"),
            AppSecretHashAlgorithm = AppSecretHasher.Algorithm,
            IsEnabled = true,
            IsDeleted = true,
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        }).ExecuteCommand();

        DefaultTenantDatabaseSeeder.Seed(tenantDb, new SystemTenantEntity
        {
            Id = TenantDefaultDataTemplates.MainTenantId,
            OrganizationId = 1300000000101,
            UserId = 1300000000111,
            TenantType = TenantTypeEnum.Db,
            DbType = tenantConfig.DbType,
            Connection = tenantConfig.ConnectionString,
            ConfigId = tenantConfig.ConfigId.ToString(),
            Remark = "系统默认租户",
            Host = "tenant.test",
        }, "encrypted-password");

        var apps = tenantDb.Queryable<TenantGameAppEntity>().Filter(null, true).ToList();
        Assert.Single(apps);
        Assert.True(apps[0].IsDeleted);
    }

    [Fact]
    public void DefaultTenant_IsProtectedFromStatusOrDeleteOperations()
    {
        Assert.True(SystemTenantService.IsDefaultTenantId(TenantDefaultDataTemplates.MainTenantId));
        Assert.True(SystemTenantService.IsProtectedDefaultTenant(new SystemTenantEntity
        {
            Id = TenantDefaultDataTemplates.MainTenantId,
            ConfigId = TenantDefaultDataTemplates.MainTenantId.ToString(),
        }));
        Assert.True(SystemTenantService.IsProtectedDefaultTenant(new SystemTenantEntity
        {
            Id = 999,
            ConfigId = SqlSugarConst.MainConfigId,
        }));
        Assert.False(SystemTenantService.IsProtectedDefaultTenant(new SystemTenantEntity
        {
            Id = 999,
            ConfigId = "999",
        }));
    }

    [Fact]
    public void OrdinaryTenantValidation_RejectsLegacyTenantTypeZeroAndUnsupportedDatabase()
    {
        Assert.ThrowsAny<Exception>(() => SystemTenantService.ValidateOrdinaryTenantInput(new AddTenantRequest
        {
            TenantType = (TenantTypeEnum)0,
            DbType = DbType.PostgreSQL,
        }));

        Assert.ThrowsAny<Exception>(() => SystemTenantService.ValidateOrdinaryTenantInput(new AddTenantRequest
        {
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.Sqlite,
        }));

        var validInput = new AddTenantRequest
        {
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.PostgreSQL,
        };
        var validException = Record.Exception(() => SystemTenantService.ValidateOrdinaryTenantInput(validInput));
        Assert.Null(validException);
        Assert.Equal(TenantTypeEnum.Db, validInput.TenantType);
        Assert.Equal(DbType.PostgreSQL, validInput.DbType);
    }

    [Fact]
    public void CreateTenantDatabase_GuardsProtectDefaultAndNonPostgreSqlTenants()
    {
        Assert.True(SystemTenantService.CanCreateTenantDatabase(new SystemTenantEntity
        {
            Id = 10001,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.PostgreSQL,
            Connection = "Host=127.0.0.1;Port=5432;Database=gameframex_admin_tenant_10001;Username=gameframex_admin_tenant_user_10001;Password=secret;",
            ConfigId = "10001",
        }));

        Assert.False(SystemTenantService.CanCreateTenantDatabase(new SystemTenantEntity
        {
            Id = TenantDefaultDataTemplates.MainTenantId,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.PostgreSQL,
            Connection = "Host=127.0.0.1;Port=5432;Database=gameframex_admin_tenant_default;Username=postgres;Password=secret;",
            ConfigId = TenantDefaultDataTemplates.MainTenantId.ToString(),
        }));

        Assert.False(SystemTenantService.CanCreateTenantDatabase(new SystemTenantEntity
        {
            Id = 10002,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.Sqlite,
            Connection = "DataSource=test.db",
            ConfigId = "10002",
        }));

        Assert.False(SystemTenantService.CanCreateTenantDatabase(new SystemTenantEntity
        {
            Id = 10003,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.PostgreSQL,
            Connection = "",
            ConfigId = "10003",
        }));
    }

    [Fact]
    public void CreateTenantAdminRoleMenus_AssignsUniquePrimaryKeys()
    {
        var roleMenus = SystemTenantService.CreateTenantAdminRoleMenus(1300000000201);

        Assert.NotEmpty(roleMenus);
        Assert.All(roleMenus, roleMenu =>
        {
            Assert.NotEqual(0, roleMenu.Id);
            Assert.Equal(1300000000201, roleMenu.RoleId);
        });
        Assert.Equal(roleMenus.Count, roleMenus.Select(u => u.Id).Distinct().Count());
    }

    [Fact]
    public void CreateOrdinaryTenantForPersistence_MapsNameToRemark()
    {
        var tenant = SystemTenantService.CreateOrdinaryTenantForPersistence(new AddTenantRequest
        {
            Id = 51255252080389,
            Name = "AlianBlank",
            AdminAccount = "AlianBlank",
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.Sqlite,
            Connection = "DataSource=test.db",
        });

        Assert.Equal(TenantTypeEnum.Db, tenant.TenantType);
        Assert.Equal(DbType.PostgreSQL, tenant.DbType);
        Assert.Equal("51255252080389", tenant.ConfigId);
        Assert.Equal("AlianBlank", tenant.Remark);
        Assert.True(tenant.CreatedTime > 0);
    }

    [Fact]
    public void PostgreSqlTenantDatabaseProvisioner_BuildPlan_DerivesTenantDatabaseUserAndConnection()
    {
        const long tenantId = 51255252080389;
        var plan = PostgreSqlTenantDatabaseProvisioner.BuildPlan(tenantId, new DbConnectionConfig
        {
            ConfigId = SqlSugarConst.MainConfigId,
            DbType = DbType.PostgreSQL,
            ConnectionString = "Host=127.0.0.1;Port=5432;Database=gameframex_admin;Username=postgres;Password=postgres;Maximum Pool Size=100",
        }, "generated-password");

        Assert.Equal("gameframex_admin_tenant_51255252080389", plan.DatabaseName);
        Assert.Equal("gameframex_admin_tenant_user_51255252080389", plan.Username);
        Assert.Equal("generated-password", plan.Password);

        var tenantBuilder = new NpgsqlConnectionStringBuilder(plan.TenantConnectionString);
        Assert.Equal(plan.DatabaseName, tenantBuilder.Database);
        Assert.Equal(plan.Username, tenantBuilder.Username);
        Assert.Equal("generated-password", tenantBuilder.Password);
        Assert.Equal("127.0.0.1", tenantBuilder.Host);
        Assert.Equal(5432, tenantBuilder.Port);
    }

    [Fact]
    public async Task TenantAdminUpdate_UpdatesTenantIdentityDataInTenantDatabase()
    {
        var tenantConfig = CreateConfig(TenantDefaultDataTemplates.MainTenantId.ToString());
        SqlSugarUnifiedConfig.ConfigureBaseSettings(tenantConfig);
        using var tenantScope = new SqlSugarScope(tenantConfig);
        var tenantDb = tenantScope.GetConnectionScope(TenantDefaultDataTemplates.MainTenantId.ToString());
        tenantDb.CodeFirst.InitTables(typeof(TenantOrganizationEntity), typeof(TenantUserEntity));
        tenantDb.Insertable(new TenantOrganizationEntity
        {
            Id = 1300000000101,
            TenantId = TenantDefaultDataTemplates.MainTenantId,
            Name = "发行运营中心",
            Code = "publisher",
            Type = "publisher",
            Level = 1,
            Remark = "发行运营中心",
        }).ExecuteCommand();
        var adminUser = SystemTenantService.CreateTenantAdminUser(new TenantResponse
        {
            Phone = "10086",
            Email = "old@example.com",
            Id = TenantDefaultDataTemplates.MainTenantId,
            AdminAccount = "admin",
            Name = "发行运营中心",
        }, "encrypted-password", 1300000000101, 1300000000103);
        adminUser.Id = 1300000000111;
        tenantDb.Insertable(adminUser).ExecuteCommand();

        await SystemTenantService.UpdateTenantIdentityDataAsync(tenantDb, new UpdateTenantRequest
        {
            Id = TenantDefaultDataTemplates.MainTenantId,
            OrganizationId = 1300000000101,
            UserId = 1300000000111,
            Name = "新的发行中心",
            AdminAccount = "tenant_admin",
            Phone = "13800000000",
            Email = "admin@example.com",
        });

        var org = tenantDb.Queryable<TenantOrganizationEntity>().First(u => u.Id == 1300000000101);
        var user = tenantDb.Queryable<TenantUserEntity>().First(u => u.Id == 1300000000111);
        Assert.Equal("新的发行中心", org.Name);
        Assert.Equal("tenant_admin", user.Account);
        Assert.Equal("13800000000", user.Phone);
        Assert.Equal("admin@example.com", user.Email);
    }

    [Fact]
    public void InitDefaultTenantDatabase_RequiresConfiguredDefaultTenantConnection()
    {
        var mainConfig = CreateConfig(SqlSugarConst.MainConfigId);
        using var scope = CreateScope(mainConfig);
        var mainDb = scope.GetConnectionScope(SqlSugarConst.MainConfigId);
        mainDb.CodeFirst.InitTables(typeof(SystemTenantEntity));
        mainDb.Insertable(CreateStaleDefaultTenant(mainConfig)).ExecuteCommand();

        var options = new DbConnectionOptions
        {
            ConnectionConfigs = new List<DbConnectionConfig> { mainConfig, },
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
                                                                     SqlSugarSetup.InitDefaultTenantDatabase(scope, options, DefaultTenantBootstrapTypes));
        Assert.Contains(TenantDefaultDataTemplates.MainTenantId.ToString(), exception.Message);
    }

    [Fact]
    public void InitDefaultTenantDatabase_UsesConfiguredConnectionAndRepairsStaleTenantRecord()
    {
        var mainConfig = CreateConfig(SqlSugarConst.MainConfigId);
        var tenantConfig = CreateConfig(TenantDefaultDataTemplates.MainTenantId.ToString());
        using var scope = CreateScope(mainConfig, tenantConfig);
        var mainDb = scope.GetConnectionScope(SqlSugarConst.MainConfigId);
        mainDb.CodeFirst.InitTables(typeof(SystemTenantEntity), typeof(SystemConfigEntity), typeof(Entity.Global.GlobalPackageMenuEntity));
        mainDb.Insertable(CreateStaleDefaultTenant(mainConfig)).ExecuteCommand();

        var options = new DbConnectionOptions
        {
            ConnectionConfigs = new List<DbConnectionConfig> { mainConfig, tenantConfig, },
        };

        // C354 T2：InitDefaultTenantDatabase 内 EnsureCompleted 依赖 MenuSeedCatalog（生产由 InitDatabase 种子块加载）
        MenuSeedCatalogTestData.LoadSampleMenus();
        SqlSugarSetup.InitDefaultTenantDatabase(scope, options, DefaultTenantBootstrapTypes, "encrypted-password");

        var repairedTenant = mainDb.Queryable<SystemTenantEntity>().First(u => u.Id == TenantDefaultDataTemplates.MainTenantId);
        Assert.Equal(tenantConfig.ConnectionString, repairedTenant.Connection);
        Assert.Equal(tenantConfig.DbType, repairedTenant.DbType);
        Assert.Equal(TenantDefaultDataTemplates.MainTenantId.ToString(), repairedTenant.ConfigId);
        Assert.Equal(TenantDefaultDataTemplates.EnterprisePackageId, repairedTenant.PackageId);

        // GFX-798（C359）：启动上下文没有 AOP 数据审计，EnsureCompleted 必须自带主键与创建时间。
        // 一旦退回「Id=0 等回填」，多行插入会从第二行起主键冲突（生产启动即崩，本用例改前必红）。
        var packageMenuRows = mainDb.Queryable<Entity.Global.GlobalPackageMenuEntity>()
                                    .Where(u => u.PackageId == TenantDefaultDataTemplates.EnterprisePackageId)
                                    .ToList();
        Assert.NotEmpty(packageMenuRows);
        Assert.All(packageMenuRows, u => Assert.True(u.Id > 0));
        Assert.All(packageMenuRows, u => Assert.True(u.CreatedTime > 0));
        Assert.Equal(packageMenuRows.Count, packageMenuRows.Select(u => u.Id).Distinct().Count());

        var tenantDb = scope.GetConnectionScope(TenantDefaultDataTemplates.MainTenantId.ToString());
        var admin = tenantDb.Queryable<TenantUserEntity>().First(u => u.Account == "admin");
        Assert.NotNull(admin);
        Assert.Equal(TenantDefaultDataTemplates.MainTenantId, admin.TenantId);
        Assert.Equal(TenantDefaultDataTemplates.DefaultAdminPhone, admin.Phone);
        Assert.Equal(TenantDefaultDataTemplates.DefaultAdminEmail, admin.Email);

        var defaultApp = tenantDb.Queryable<TenantGameAppEntity>().Single();
        Assert.Equal(TenantDefaultDataTemplates.MainTenantId, defaultApp.TenantId);
        Assert.Equal("默认应用", defaultApp.Name);
        Assert.Null(defaultApp.AppSecret);
        Assert.False(string.IsNullOrWhiteSpace(defaultApp.AppSecretHash));
    }

    [Fact]
    public void InitDefaultTenantDatabase_EncryptsRepairedConnectionWhenEncryptionKeyConfigured()
    {
        WithEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key", () =>
        {
            var mainConfig = CreateConfig(SqlSugarConst.MainConfigId);
            var tenantConfig = CreateConfig(TenantDefaultDataTemplates.MainTenantId.ToString());
            using (var scope = CreateScope(mainConfig, tenantConfig))
            {
                var mainDb = scope.GetConnectionScope(SqlSugarConst.MainConfigId);
                mainDb.CodeFirst.InitTables(typeof(SystemTenantEntity), typeof(SystemConfigEntity), typeof(Entity.Global.GlobalPackageMenuEntity));
                mainDb.Insertable(CreateStaleDefaultTenant(mainConfig)).ExecuteCommand();

                var options = new DbConnectionOptions
                {
                    ConnectionConfigs = new List<DbConnectionConfig> { mainConfig, tenantConfig, },
                };

                // C354 T2：InitDefaultTenantDatabase 内 EnsureCompleted 依赖 MenuSeedCatalog（生产由 InitDatabase 种子块加载）
                MenuSeedCatalogTestData.LoadSampleMenus();
                SqlSugarSetup.InitDefaultTenantDatabase(scope, options, DefaultTenantBootstrapTypes, "encrypted-password");

                // GFX-727：stale 明文行修复后 connection 必须以 enc:v1: 密文落库（TenantConnectionCrypto 收口），解密回读与配置一致
                var repairedTenant = mainDb.Queryable<SystemTenantEntity>().First(u => u.Id == TenantDefaultDataTemplates.MainTenantId);
                Assert.True(TenantConnectionCrypto.IsEncrypted(repairedTenant.Connection));
                Assert.Equal(tenantConfig.ConnectionString, TenantConnectionCrypto.Decrypt(repairedTenant.Connection));
            }
        });
    }

    [Fact]
    public void InitDefaultTenantDatabase_KeepsEncryptedConnectionWhenAlreadyCurrent()
    {
        WithEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key", () =>
        {
            var mainConfig = CreateConfig(SqlSugarConst.MainConfigId);
            var tenantConfig = CreateConfig(TenantDefaultDataTemplates.MainTenantId.ToString());
            using (var scope = CreateScope(mainConfig, tenantConfig))
            {
                var mainDb = scope.GetConnectionScope(SqlSugarConst.MainConfigId);
                mainDb.CodeFirst.InitTables(typeof(SystemTenantEntity), typeof(SystemConfigEntity), typeof(Entity.Global.GlobalPackageMenuEntity));
                // 已是目标形态且 connection 已加密：对账必须按「解密后比较」，不得把密文当漂移盖回明文
                var encryptedTenant = CreateStaleDefaultTenant(mainConfig);
                encryptedTenant.DbType = tenantConfig.DbType;
                encryptedTenant.Connection = TenantConnectionCrypto.Encrypt(tenantConfig.ConnectionString);
                encryptedTenant.PackageId = TenantDefaultDataTemplates.EnterprisePackageId;
                mainDb.Insertable(encryptedTenant).ExecuteCommand();

                var options = new DbConnectionOptions
                {
                    ConnectionConfigs = new List<DbConnectionConfig> { mainConfig, tenantConfig, },
                };

                // C354 T2：InitDefaultTenantDatabase 内 EnsureCompleted 依赖 MenuSeedCatalog（生产由 InitDatabase 种子块加载）
                MenuSeedCatalogTestData.LoadSampleMenus();
                SqlSugarSetup.InitDefaultTenantDatabase(scope, options, DefaultTenantBootstrapTypes, "encrypted-password");

                // 无漂移则不触发回写；若对账退化为密文 vs 明文比较，重加密的随机密文必与原值不等，此断言拦截
                var persistedTenant = mainDb.Queryable<SystemTenantEntity>().First(u => u.Id == TenantDefaultDataTemplates.MainTenantId);
                Assert.True(TenantConnectionCrypto.IsEncrypted(persistedTenant.Connection));
                Assert.Equal(encryptedTenant.Connection, persistedTenant.Connection);
            }
        });
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

    private DbConnectionConfig CreateConfig(string configId)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_default_tenant_{configId}_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);

        return new DbConnectionConfig
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
    }

    private static SqlSugarScope CreateScope(params DbConnectionConfig[] configs)
    {
        foreach (var config in configs)
        {
            SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        }

        // GFX-798（C359）：本 scope 刻意不补挂 DataExecuting 审计——生产启动路径执行 InitDatabase 时，
        // SqlSugarSetup.cs:428 的 CurrentConnectionConfig 覆盖会抹除 SetDbAop 挂载的 AOP，启动上下文
        // 实际没有数据审计。此处曾手工补挂该 AOP，导致测试镜像了一个生产不存在的接线，
        // C354 T2 EnsureCompleted 的 Id=0 被伪装成可用（启动必崩却在 CI 全绿）。
        return new SqlSugarScope(configs.Cast<ConnectionConfig>().ToList());
    }

    private static SystemTenantEntity CreateStaleDefaultTenant(DbConnectionConfig mainConfig)
    {
        return new SystemTenantEntity
        {
            Id = TenantDefaultDataTemplates.MainTenantId,
            OrganizationId = 1300000000101,
            UserId = 1300000000111,
            TenantType = TenantTypeEnum.Db,
            DbType = mainConfig.DbType,
            Connection = mainConfig.ConnectionString,
            ConfigId = TenantDefaultDataTemplates.MainTenantId.ToString(),
            Remark = "系统默认租户",
            Host = "tenant.test",
        };
    }

    [Fact]
    public void InitDefaultTenantDatabase_ReEncryptsPlaintextConnectionWhenKeyConfigured()
    {
        WithEnvironmentVariable(TenantConnectionCrypto.KeyEnvironmentVariable, "unit-test-encryption-key", () =>
        {
            var mainConfig = CreateConfig(SqlSugarConst.MainConfigId);
            var tenantConfig = CreateConfig(TenantDefaultDataTemplates.MainTenantId.ToString());
            using (var scope = CreateScope(mainConfig, tenantConfig))
            {
                var mainDb = scope.GetConnectionScope(SqlSugarConst.MainConfigId);
                mainDb.CodeFirst.InitTables(typeof(SystemTenantEntity), typeof(SystemConfigEntity), typeof(Entity.Global.GlobalPackageMenuEntity));
                // 内容与配置一致但为明文形态（历史版本未走加密收口的写入残留）：配钥环境下须收敛为密文
                var plaintextTenant = CreateStaleDefaultTenant(mainConfig);
                plaintextTenant.DbType = tenantConfig.DbType;
                plaintextTenant.Connection = tenantConfig.ConnectionString;
                plaintextTenant.PackageId = TenantDefaultDataTemplates.EnterprisePackageId;
                mainDb.Insertable(plaintextTenant).ExecuteCommand();

                var options = new DbConnectionOptions
                {
                    ConnectionConfigs = new List<DbConnectionConfig> { mainConfig, tenantConfig, },
                };

                // C354 T2：InitDefaultTenantDatabase 内 EnsureCompleted 依赖 MenuSeedCatalog（生产由 InitDatabase 种子块加载）
                MenuSeedCatalogTestData.LoadSampleMenus();
                SqlSugarSetup.InitDefaultTenantDatabase(scope, options, DefaultTenantBootstrapTypes, "encrypted-password");

                var persistedTenant = mainDb.Queryable<SystemTenantEntity>().First(u => u.Id == TenantDefaultDataTemplates.MainTenantId);
                Assert.True(TenantConnectionCrypto.IsEncrypted(persistedTenant.Connection));
                Assert.Equal(tenantConfig.ConnectionString, TenantConnectionCrypto.Decrypt(persistedTenant.Connection));
            }
        });
    }

    private static void WithEnvironmentVariable(string name, string? value, Action action)
    {
        var original = Environment.GetEnvironmentVariable(name);
        try
        {
            Environment.SetEnvironmentVariable(name, value);
            action();
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, original);
        }
    }
}