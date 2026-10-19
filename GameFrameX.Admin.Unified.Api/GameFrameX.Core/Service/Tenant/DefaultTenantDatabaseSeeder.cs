using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Security;
using GameFrameX.Core.Service.Tenant.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.Device;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Entity.Tenant.Game.ServerConfig;
using GameFrameX.Entity.Tenant.ObjectStorage;
using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Entity.Tenant.Game.Resource;

namespace GameFrameX.Core.Service.Tenant;

public static class DefaultTenantDatabaseSeeder
{
    private const long DefaultCreatedTime = 1745836800;
    private const string DefaultCreatedName = "租管";

    public static void Seed(SqlSugarScopeProvider tenantDb, SystemTenantEntity tenant, string encryptedPassword)
    {
        var orgs = BuildOrganizations(tenant.Id, tenant.OrganizationId);
        var roles = BuildRoles(tenant.Id);
        var positions = BuildPositions(tenant.Id);
        var adminRole = roles.Single(u => u.Code == TenantDefaultDataTemplates.AdminRoleCode);
        var adminPosition = positions.Single(u => u.Code == TenantDefaultDataTemplates.AdminPositionCode);
        var adminUser = SystemTenantService.CreateTenantAdminUser(
            new TenantResponse
            {
                Id = tenant.Id,
                AdminAccount = "admin",
                Email = TenantDefaultDataTemplates.DefaultAdminEmail,
                Phone = TenantDefaultDataTemplates.DefaultAdminPhone,
                Name = tenant.Remark ?? "默认租户",
            },
            encryptedPassword,
            tenant.OrganizationId,
            adminPosition.Id);
        adminUser.Id = tenant.UserId;

        var userRoles = new[]
        {
            new TenantUserRoleEntity
            {
                Id = StableSeedId.UserRole(tenant.Id, adminUser.Id, adminRole.Id),
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
            },
        };

        var roleMenus = MenuSeedCatalog.BuildRoleMenus(adminRole.Id, adminRole.Code ?? string.Empty);

        Upsert(tenantDb, orgs);
        Upsert(tenantDb, roles);
        Upsert(tenantDb, positions);
        Upsert(tenantDb, new[] { adminUser, });
        Upsert(tenantDb, userRoles);
        Upsert(tenantDb, roleMenus);
        // C189：Upsert 只能补入/更新本次 BuildRoleMenus 产出的行，无法删除 SyncRoles 减少后残留的 role-menu。
        // 以当前 SyncRoles 为权威删除多余授权，使默认租户管理员 role-menu 随 seed SyncRoles 变更幂等收敛。
        ReconcileAdminRoleMenus(tenantDb, adminRole.Id, adminRole.Code ?? string.Empty);
        SeedDefaultObjectStorageConfigIfEmpty(tenantDb, tenant.Id);
        SeedDefaultTenantBusinessDataIfEmpty(tenantDb, tenant.Id, tenant.UserId);
    }

    /// <summary>
    /// 为新租户构建默认启用的本地对象存储配置，保证租户开箱即用、上传不报错。
    /// </summary>
    public static TenantObjectStorageConfigEntity BuildDefaultObjectStorageConfig(long tenantId)
    {
        var createdTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds();
        return new TenantObjectStorageConfigEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            TenantId = tenantId,
            Provider = "local",
            PathTemplate = "Upload/{yyyy}/{MM}/{dd}",
            MaxFileSizeBytes = 20971520,
            AllowedContentTypesJson = string.Empty,
            EnableMd5 = true,
            ConfigJson = "{}",
            IsEnabled = true,
            Remark = "默认本地存储",
            CreatedTime = createdTime,
            UpdateCount = 0,
        };
    }

    /// <summary>
    /// 为新租户在对象存储配置为空时写入默认启用的本地配置，保证租户开箱即用、上传不报错。
    /// </summary>
    public static void SeedDefaultObjectStorageConfigIfEmpty(SqlSugarScopeProvider tenantDb, long tenantId)
    {
        var hasAny = tenantDb.Queryable<TenantObjectStorageConfigEntity>()
                             .Filter(null, true)
                             .Any(u => u.TenantId == tenantId);
        if (hasAny)
        {
            return;
        }

        tenantDb.Insertable(BuildDefaultObjectStorageConfig(tenantId)).ExecuteCommand();
    }

    /// <summary>
    /// 为首次创建的租户生成一个可由租户自行维护的默认游戏应用。
    /// </summary>
    public static TenantGameAppEntity BuildDefaultGameApp(long tenantId)
    {
        var createdTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds();
        return new TenantGameAppEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            TenantId = tenantId,
            Name = "默认应用",
            Description = "系统自动创建的默认游戏应用，可由租户自行修改",
            AppSecret = null,
            AppSecretHash = AppSecretHasher.Hash("default-game-app:" + tenantId),
            AppSecretHashAlgorithm = AppSecretHasher.Algorithm,
            SignatureRequired = false,
            DeviceProofRequired = false,
            RiskPolicy = string.Empty,
            IsEnabled = true,
            IsDeleted = false,
            LastRotatedTime = TimerHelper.UnixTimeSeconds(),
            Remark = "默认游戏应用",
            CreatedTime = createdTime,
            UpdateCount = 0,
        };
    }

    public static TenantGameAppEntity? SeedDefaultGameAppIfEmpty(SqlSugarScopeProvider tenantDb, long tenantId)
    {
        var existingApp = tenantDb.Queryable<TenantGameAppEntity>()
                                  .Filter(null, true)
                                  .First(u => u.TenantId == tenantId);
        if (existingApp != null)
        {
            return existingApp.IsDeleted == true ? null : existingApp;
        }

        var defaultApp = BuildDefaultGameApp(tenantId);
        tenantDb.Insertable(defaultApp).ExecuteCommand();
        return defaultApp;
    }

    public static async Task<TenantGameAppEntity?> SeedDefaultGameAppIfEmptyAsync(SqlSugarScopeProvider tenantDb, long tenantId)
    {
        var existingApp = await tenantDb.Queryable<TenantGameAppEntity>()
                                        .Filter(null, true)
                                        .FirstAsync(u => u.TenantId == tenantId);
        if (existingApp != null)
        {
            return existingApp.IsDeleted == true ? null : existingApp;
        }

        var defaultApp = BuildDefaultGameApp(tenantId);
        await tenantDb.Insertable(defaultApp).ExecuteCommandAsync();
        return defaultApp;
    }

    /// <summary>
    /// 为租户补齐开箱即用的业务字典数据。
    /// </summary>
    public static void SeedDefaultTenantBusinessDataIfEmpty(SqlSugarScopeProvider tenantDb, long tenantId, long createdUserId)
    {
        var defaultApp = SeedDefaultGameAppIfEmpty(tenantDb, tenantId);
        SeedDefaultTenantChannelIfMissing(tenantDb, tenantId, createdUserId);
        SeedDefaultRemoteConfigDefinitionsIfMissing(tenantDb, tenantId, createdUserId);
        SeedDefaultGameServerConfigsIfMissing(tenantDb, tenantId);

        if (defaultApp == null)
        {
            return;
        }

        SeedDefaultDeviceGroupIfMissing(tenantDb, tenantId, defaultApp.Id, createdUserId);
        SeedDefaultRedeemCodeChannelIfMissing(tenantDb, tenantId, defaultApp.Id, createdUserId);
        SeedDefaultAssetPackageIfMissing(tenantDb, tenantId, defaultApp.Id, createdUserId);
        SeedDefaultVersionDefinitionsIfMissing(tenantDb, tenantId, defaultApp.Id, createdUserId);
    }

    /// <summary>
    /// 为租户补齐开箱即用的业务字典数据。
    /// </summary>
    public static async Task SeedDefaultTenantBusinessDataIfEmptyAsync(SqlSugarScopeProvider tenantDb, long tenantId, long createdUserId)
    {
        var defaultApp = await SeedDefaultGameAppIfEmptyAsync(tenantDb, tenantId);
        await SeedDefaultTenantChannelIfMissingAsync(tenantDb, tenantId, createdUserId);
        await SeedDefaultRemoteConfigDefinitionsIfMissingAsync(tenantDb, tenantId, createdUserId);
        await SeedDefaultGameServerConfigsIfMissingAsync(tenantDb, tenantId);

        if (defaultApp == null)
        {
            return;
        }

        await SeedDefaultDeviceGroupIfMissingAsync(tenantDb, tenantId, defaultApp.Id, createdUserId);
        await SeedDefaultRedeemCodeChannelIfMissingAsync(tenantDb, tenantId, defaultApp.Id, createdUserId);
        await SeedDefaultAssetPackageIfMissingAsync(tenantDb, tenantId, defaultApp.Id, createdUserId);
        await SeedDefaultVersionDefinitionsIfMissingAsync(tenantDb, tenantId, defaultApp.Id, createdUserId);
    }

    public static List<TenantOrganizationEntity> BuildOrganizations(long tenantId, long rootOrganizationId)
    {
        var createdTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds();
        var orgsByKey = new Dictionary<string, TenantOrganizationEntity>(StringComparer.Ordinal);
        return TenantDefaultDataTemplates.OrganizationTemplates.Select(template =>
        {
            var id = template.Key == TenantDefaultDataTemplates.RootOrganizationKey ? rootOrganizationId : template.SeedId;
            var org = new TenantOrganizationEntity
            {
                Id = id,
                TenantId = tenantId,
                Pid = template.ParentKey == null ? 0 : orgsByKey[template.ParentKey].Id,
                Name = template.Name,
                Code = template.Code,
                Type = template.Type,
                Level = template.Level,
                OrderNo = template.OrderNo,
                CreatedTime = createdTime,
                Remark = template.Remark,
                UpdateCount = 0,
            };
            orgsByKey.Add(template.Key, org);
            return org;
        }).ToList();
    }

    public static List<TenantRoleEntity> BuildRoles(long tenantId)
    {
        var createdTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds();
        return TenantDefaultDataTemplates.RoleTemplates.Select(template => new TenantRoleEntity
        {
            Id = template.SeedId,
            TenantId = tenantId,
            Name = template.Name,
            DataScope = template.DataScope,
            Code = template.Code,
            CreatedTime = createdTime,
            Remark = template.Remark,
            UpdateCount = 0,
            OrderNo = template.OrderNo,
        }).ToList();
    }

    public static List<TenantPositionEntity> BuildPositions(long tenantId)
    {
        var createdTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds();
        return TenantDefaultDataTemplates.PositionTemplates.Select(template => new TenantPositionEntity
        {
            Id = template.SeedId,
            TenantId = tenantId,
            Name = template.Name,
            Code = template.Code,
            OrderNo = template.OrderNo,
            CreatedTime = createdTime,
            Remark = template.Remark,
            UpdateCount = 0,
        }).ToList();
    }

    private static TenantChannelDefinitionEntity BuildDefaultTenantChannel(long tenantId, long createdUserId)
    {
        return new TenantChannelDefinitionEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            TenantId = tenantId,
            Code = "default",
            Name = "默认渠道",
            Scope = ChannelScopeEnum.Tenant,
            PlatformCode = "all",
            ChannelType = ChannelTypeEnum.Custom,
            IsDefault = true,
            IsEnabled = true,
            IsDeleted = false,
            Description = "系统自动创建的默认渠道",
            CreatedTime = DefaultCreatedTime,
            CreatedId = createdUserId,
            CreatedName = DefaultCreatedName,
            UpdateCount = 0,
        };
    }

    private static TenantGameAppDeviceGroupEntity BuildDefaultDeviceGroup(long tenantId, long appId, long createdUserId)
    {
        return new TenantGameAppDeviceGroupEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            TenantId = tenantId,
            AppId = appId,
            Name = "default",
            Description = "默认缺省分组",
            CreatedTime = DefaultCreatedTime,
            CreatedId = createdUserId,
            CreatedName = DefaultCreatedName,
            IsDeleted = false,
            UpdateCount = 0,
        };
    }

    private static TenantGameRedeemCodeChannelEntity BuildDefaultRedeemCodeChannel(long tenantId, long appId, long createdUserId)
    {
        return new TenantGameRedeemCodeChannelEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            TenantId = tenantId,
            AppId = appId,
            Name = "default",
            Description = "默认兑换码渠道",
            CreatedTime = DefaultCreatedTime,
            CreatedId = createdUserId,
            CreatedName = DefaultCreatedName,
            RoleUsageLimit = 0,
            IsDeleted = false,
            UpdateCount = 0,
        };
    }

    private static TenantGameAppAssetPackageEntity BuildDefaultAssetPackage(long tenantId, long appId, long createdUserId)
    {
        return new TenantGameAppAssetPackageEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            TenantId = tenantId,
            GameAppId = appId,
            Name = "DefaultPackage",
            Description = "默认资源包名称",
            CreatedTime = DefaultCreatedTime,
            CreatedId = createdUserId,
            CreatedName = DefaultCreatedName,
            IsDeleted = false,
            UpdateCount = 0,
        };
    }

    private static IEnumerable<TenantGameAppVersionDefinitionEntity> BuildDefaultVersionDefinitions(long tenantId, long appId, long createdUserId)
    {
        return new[]
        {
            new TenantGameAppVersionDefinitionEntity
            {
                Id = SnowFlakeIdHelper.Instance.NextId(),
                TenantId = tenantId,
                AppId = appId,
                Name = "default",
                Description = "默认缺省版本",
                CreatedTime = DefaultCreatedTime,
                CreatedId = createdUserId,
                CreatedName = DefaultCreatedName,
                IsDeleted = false,
                UpdateCount = 0,
            },
            new TenantGameAppVersionDefinitionEntity
            {
                Id = SnowFlakeIdHelper.Instance.NextId(),
                TenantId = tenantId,
                AppId = appId,
                Name = "1.0.0",
                Description = "初始版本",
                CreatedTime = DefaultCreatedTime,
                CreatedId = createdUserId,
                CreatedName = DefaultCreatedName,
                IsDeleted = false,
                UpdateCount = 0,
            },
        };
    }

    private static IEnumerable<TenantGameAppRemoteConfigKeyEntity> BuildDefaultRemoteConfigKeys(long tenantId, long createdUserId)
    {
        return new[]
        {
            BuildDefaultRemoteConfigKey(tenantId, createdUserId, "maintenance", "维护模式", "bool", "是否处于维护状态", 0),
            BuildDefaultRemoteConfigKey(tenantId, createdUserId, "noticeUrl", "公告地址", "string", "游戏内公告页面URL", 1),
            BuildDefaultRemoteConfigKey(tenantId, createdUserId, "customerServiceUrl", "客服地址", "string", "客服系统页面URL", 2),
            BuildDefaultRemoteConfigKey(tenantId, createdUserId, "enableRegister", "开放注册", "bool", "是否允许新用户注册", 3),
        };
    }

    private static TenantGameAppRemoteConfigKeyEntity BuildDefaultRemoteConfigKey(long tenantId, long createdUserId, string keyName, string displayName, string valueType, string description, int sortOrder)
    {
        return new TenantGameAppRemoteConfigKeyEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            TenantId = tenantId,
            KeyName = keyName,
            DisplayName = displayName,
            ValueType = valueType,
            Description = description,
            SortOrder = sortOrder,
            IsEnabled = true,
            CreatedTime = DefaultCreatedTime,
            CreatedId = createdUserId,
            CreatedName = DefaultCreatedName,
            IsDeleted = false,
            UpdateCount = 0,
        };
    }

    private static IEnumerable<TenantGameAppRemoteConfigModeEntity> BuildDefaultRemoteConfigModes(long tenantId, long createdUserId)
    {
        return new[]
        {
            BuildDefaultRemoteConfigMode(tenantId, createdUserId, "review", "审核模式", "应用商店审核用", 0),
            BuildDefaultRemoteConfigMode(tenantId, createdUserId, "formal", "正式模式", "线上运营用", 1),
        };
    }

    private static TenantGameAppRemoteConfigModeEntity BuildDefaultRemoteConfigMode(long tenantId, long createdUserId, string modeCode, string displayName, string description, int sortOrder)
    {
        return new TenantGameAppRemoteConfigModeEntity
        {
            Id = SnowFlakeIdHelper.Instance.NextId(),
            TenantId = tenantId,
            ModeCode = modeCode,
            DisplayName = displayName,
            Description = description,
            SortOrder = sortOrder,
            IsEnabled = true,
            CreatedTime = DefaultCreatedTime,
            CreatedId = createdUserId,
            CreatedName = DefaultCreatedName,
            IsDeleted = false,
            UpdateCount = 0,
        };
    }

    private static void SeedDefaultTenantChannelIfMissing(SqlSugarScopeProvider tenantDb, long tenantId, long createdUserId)
    {
        var exists = tenantDb.Queryable<TenantChannelDefinitionEntity>()
                             .Filter(null, true)
                             .Any(u => u.TenantId == tenantId && u.Code == "default");
        if (!exists)
        {
            tenantDb.Insertable(BuildDefaultTenantChannel(tenantId, createdUserId)).ExecuteCommand();
        }
    }

    private static async Task SeedDefaultTenantChannelIfMissingAsync(SqlSugarScopeProvider tenantDb, long tenantId, long createdUserId)
    {
        var exists = await tenantDb.Queryable<TenantChannelDefinitionEntity>()
                                   .Filter(null, true)
                                   .AnyAsync(u => u.TenantId == tenantId && u.Code == "default");
        if (!exists)
        {
            await tenantDb.Insertable(BuildDefaultTenantChannel(tenantId, createdUserId)).ExecuteCommandAsync();
        }
    }

    private static void SeedDefaultDeviceGroupIfMissing(SqlSugarScopeProvider tenantDb, long tenantId, long appId, long createdUserId)
    {
        var exists = tenantDb.Queryable<TenantGameAppDeviceGroupEntity>()
                             .Filter(null, true)
                             .Any(u => u.TenantId == tenantId && u.AppId == appId && u.Name == "default");
        if (!exists)
        {
            tenantDb.Insertable(BuildDefaultDeviceGroup(tenantId, appId, createdUserId)).ExecuteCommand();
        }
    }

    private static async Task SeedDefaultDeviceGroupIfMissingAsync(SqlSugarScopeProvider tenantDb, long tenantId, long appId, long createdUserId)
    {
        var exists = await tenantDb.Queryable<TenantGameAppDeviceGroupEntity>()
                                   .Filter(null, true)
                                   .AnyAsync(u => u.TenantId == tenantId && u.AppId == appId && u.Name == "default");
        if (!exists)
        {
            await tenantDb.Insertable(BuildDefaultDeviceGroup(tenantId, appId, createdUserId)).ExecuteCommandAsync();
        }
    }

    private static void SeedDefaultRedeemCodeChannelIfMissing(SqlSugarScopeProvider tenantDb, long tenantId, long appId, long createdUserId)
    {
        var exists = tenantDb.Queryable<TenantGameRedeemCodeChannelEntity>()
                             .Filter(null, true)
                             .Any(u => u.TenantId == tenantId && u.AppId == appId && u.Name == "default");
        if (!exists)
        {
            tenantDb.Insertable(BuildDefaultRedeemCodeChannel(tenantId, appId, createdUserId)).ExecuteCommand();
        }
    }

    private static async Task SeedDefaultRedeemCodeChannelIfMissingAsync(SqlSugarScopeProvider tenantDb, long tenantId, long appId, long createdUserId)
    {
        var exists = await tenantDb.Queryable<TenantGameRedeemCodeChannelEntity>()
                                   .Filter(null, true)
                                   .AnyAsync(u => u.TenantId == tenantId && u.AppId == appId && u.Name == "default");
        if (!exists)
        {
            await tenantDb.Insertable(BuildDefaultRedeemCodeChannel(tenantId, appId, createdUserId)).ExecuteCommandAsync();
        }
    }

    private static void SeedDefaultAssetPackageIfMissing(SqlSugarScopeProvider tenantDb, long tenantId, long appId, long createdUserId)
    {
        var exists = tenantDb.Queryable<TenantGameAppAssetPackageEntity>()
                             .Filter(null, true)
                             .Any(u => u.TenantId == tenantId && u.GameAppId == appId && u.Name == "DefaultPackage");
        if (!exists)
        {
            tenantDb.Insertable(BuildDefaultAssetPackage(tenantId, appId, createdUserId)).ExecuteCommand();
        }
    }

    private static async Task SeedDefaultAssetPackageIfMissingAsync(SqlSugarScopeProvider tenantDb, long tenantId, long appId, long createdUserId)
    {
        var exists = await tenantDb.Queryable<TenantGameAppAssetPackageEntity>()
                                   .Filter(null, true)
                                   .AnyAsync(u => u.TenantId == tenantId && u.GameAppId == appId && u.Name == "DefaultPackage");
        if (!exists)
        {
            await tenantDb.Insertable(BuildDefaultAssetPackage(tenantId, appId, createdUserId)).ExecuteCommandAsync();
        }
    }

    private static void SeedDefaultVersionDefinitionsIfMissing(SqlSugarScopeProvider tenantDb, long tenantId, long appId, long createdUserId)
    {
        var existingNames = tenantDb.Queryable<TenantGameAppVersionDefinitionEntity>()
                                    .Filter(null, true)
                                    .Where(u => u.TenantId == tenantId && u.AppId == appId)
                                    .Select(u => u.Name)
                                    .ToList();
        var existing = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = BuildDefaultVersionDefinitions(tenantId, appId, createdUserId)
                      .Where(u => !existing.Contains(u.Name))
                      .ToList();
        if (missing.Count > 0)
        {
            tenantDb.Insertable(missing).ExecuteCommand();
        }
    }

    private static async Task SeedDefaultVersionDefinitionsIfMissingAsync(SqlSugarScopeProvider tenantDb, long tenantId, long appId, long createdUserId)
    {
        var existingNames = await tenantDb.Queryable<TenantGameAppVersionDefinitionEntity>()
                                          .Filter(null, true)
                                          .Where(u => u.TenantId == tenantId && u.AppId == appId)
                                          .Select(u => u.Name)
                                          .ToListAsync();
        var existing = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = BuildDefaultVersionDefinitions(tenantId, appId, createdUserId)
                      .Where(u => !existing.Contains(u.Name))
                      .ToList();
        if (missing.Count > 0)
        {
            await tenantDb.Insertable(missing).ExecuteCommandAsync();
        }
    }

    private static void SeedDefaultRemoteConfigDefinitionsIfMissing(SqlSugarScopeProvider tenantDb, long tenantId, long createdUserId)
    {
        var existingKeys = tenantDb.Queryable<TenantGameAppRemoteConfigKeyEntity>()
                                   .Filter(null, true)
                                   .Where(u => u.TenantId == tenantId)
                                   .Select(u => u.KeyName)
                                   .ToList()
                                   .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingKeys = BuildDefaultRemoteConfigKeys(tenantId, createdUserId)
                          .Where(u => !existingKeys.Contains(u.KeyName))
                          .ToList();
        if (missingKeys.Count > 0)
        {
            tenantDb.Insertable(missingKeys).ExecuteCommand();
        }

        var existingModes = tenantDb.Queryable<TenantGameAppRemoteConfigModeEntity>()
                                    .Filter(null, true)
                                    .Where(u => u.TenantId == tenantId)
                                    .Select(u => u.ModeCode)
                                    .ToList()
                                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingModes = BuildDefaultRemoteConfigModes(tenantId, createdUserId)
                           .Where(u => !existingModes.Contains(u.ModeCode))
                           .ToList();
        if (missingModes.Count > 0)
        {
            tenantDb.Insertable(missingModes).ExecuteCommand();
        }
    }

    private static async Task SeedDefaultRemoteConfigDefinitionsIfMissingAsync(SqlSugarScopeProvider tenantDb, long tenantId, long createdUserId)
    {
        var keyNames = await tenantDb.Queryable<TenantGameAppRemoteConfigKeyEntity>()
                                     .Filter(null, true)
                                     .Where(u => u.TenantId == tenantId)
                                     .Select(u => u.KeyName)
                                     .ToListAsync();
        var existingKeys = keyNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingKeys = BuildDefaultRemoteConfigKeys(tenantId, createdUserId)
                          .Where(u => !existingKeys.Contains(u.KeyName))
                          .ToList();
        if (missingKeys.Count > 0)
        {
            await tenantDb.Insertable(missingKeys).ExecuteCommandAsync();
        }

        var modeCodes = await tenantDb.Queryable<TenantGameAppRemoteConfigModeEntity>()
                                      .Filter(null, true)
                                      .Where(u => u.TenantId == tenantId)
                                      .Select(u => u.ModeCode)
                                      .ToListAsync();
        var existingModes = modeCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingModes = BuildDefaultRemoteConfigModes(tenantId, createdUserId)
                           .Where(u => !existingModes.Contains(u.ModeCode))
                           .ToList();
        if (missingModes.Count > 0)
        {
            await tenantDb.Insertable(missingModes).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// C314 游戏运营配置出厂基线行补种：按 Key 查缺插入（幂等，不覆盖已有行——运营改过的应用行与停用过的基线行均不受影响）。
    /// <para>自动推导的租户库连接 EnableInitSeed=false，ISqlSugarEntitySeedData 框架种子渠道不生效，新租户建库由此路径灌基线；
    /// 基线行 AppId=null（租户默认），登录下发时被应用行按 Key 覆盖。</para>
    /// </summary>
    private static void SeedDefaultGameServerConfigsIfMissing(SqlSugarScopeProvider tenantDb, long tenantId)
    {
        var existingKeys = tenantDb.Queryable<TenantGameAppServerConfigEntity>()
                                   .Filter(null, true)
                                   .Where(u => u.AppId == null)
                                   .Select(u => u.Key)
                                   .ToList()
                                   .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = GameServerConfigBaselineSeed.BuildRows()
                      .Where(u => !existingKeys.Contains(u.Key))
                      .ToList();
        if (missing.Count == 0)
        {
            return;
        }

        foreach (var row in missing)
        {
            row.TenantId = tenantId;
        }

        tenantDb.Insertable(missing).ExecuteCommand();
    }

    /// <summary>
    /// C314 游戏运营配置出厂基线行补种（异步版，语义同同步版）
    /// </summary>
    private static async Task SeedDefaultGameServerConfigsIfMissingAsync(SqlSugarScopeProvider tenantDb, long tenantId)
    {
        var keyList = await tenantDb.Queryable<TenantGameAppServerConfigEntity>()
                                    .Filter(null, true)
                                    .Where(u => u.AppId == null)
                                    .Select(u => u.Key)
                                    .ToListAsync();
        var existingKeys = keyList.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = GameServerConfigBaselineSeed.BuildRows()
                      .Where(u => !existingKeys.Contains(u.Key))
                      .ToList();
        if (missing.Count == 0)
        {
            return;
        }

        foreach (var row in missing)
        {
            row.TenantId = tenantId;
        }

        await tenantDb.Insertable(missing).ExecuteCommandAsync();
    }

    /// <summary>
    /// 按主键补缺种子行（C278 起 insert-only）：只插入 DB 中缺失的行，不覆盖用户已修改的已有行
    /// （默认租户管理员账号/组织/角色/岗位等）。role-menu 的减少方向收敛由 <see cref="ReconcileAdminRoleMenus"/> 负责。
    /// </summary>
    internal static void Upsert<T>(SqlSugarScopeProvider tenantDb, IEnumerable<T> data) where T : class, new()
    {
        var items = data.ToList();
        if (items.Count == 0)
        {
            return;
        }

        var entityInfo = tenantDb.EntityMaintenance.GetEntityInfo<T>();
        var primaryKeyColumns = entityInfo.Columns.Where(u => u.IsPrimarykey).Select(u => u.PropertyName).ToArray();
        if (primaryKeyColumns.Length == 0)
        {
            tenantDb.Insertable(items).ExecuteCommand();
            return;
        }

        tenantDb.QueryFilter.ClearAndBackup();
        try
        {
            var storageable = tenantDb.Storageable(items);
            storageable.WhereColumns(primaryKeyColumns);
            var storage = storageable.ToStorage();
            storage.AsInsertable.ExecuteCommand();
        }
        finally
        {
            tenantDb.QueryFilter.Restore();
        }
    }

    /// <summary>
    /// 以当前 global_menu SyncRoles 为权威，删除默认租户管理员角色下"种子菜单范围内、但不再授权"的 role-menu（C189）。
    /// <para>补缺失由 <see cref="Seed"/> 的 <c>Upsert(BuildRoleMenus)</c> 负责；本方法只收敛 SyncRoles 减少方向的多余授权，
    /// 保证重跑幂等、不残留孤儿 role-menu。仅删除 <see cref="MenuSeedCatalog.GetSurplusSeedMenuIds"/> 范围内的种子菜单，
    /// 不影响租户手动授权的非种子菜单。</para>
    /// </summary>
    private static void ReconcileAdminRoleMenus(SqlSugarScopeProvider tenantDb, long roleId, string roleCode)
    {
        var surplusMenuIds = MenuSeedCatalog.GetSurplusSeedMenuIds(roleCode).ToList();
        if (surplusMenuIds.Count == 0)
        {
            return;
        }

        tenantDb.QueryFilter.ClearAndBackup();
        try
        {
            tenantDb.Deleteable<TenantRoleMenuEntity>()
                    .Where(u => u.RoleId == roleId && surplusMenuIds.Contains(u.MenuId))
                    .ExecuteCommand();
        }
        finally
        {
            tenantDb.QueryFilter.Restore();
        }
    }
}