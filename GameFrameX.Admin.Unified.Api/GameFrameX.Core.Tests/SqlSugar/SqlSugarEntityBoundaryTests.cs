using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Entity.Tenant.Game.Resource;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Tenant.Device;

namespace GameFrameX.Core.Tests.SqlSugar;

public class SqlSugarEntityBoundaryTests
{
    [Fact]
    public void PlatformEntity_IsPlatformDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemTenantEntity)));
        Assert.False(SqlSugarEntityBoundary.IsTenantTable(typeof(SystemTenantEntity)));
    }

    [Fact]
    public void TenantGameEntity_IsTenantDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantGameAppEntity)));
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(TenantGameAppEntity)));
        Assert.Equal("tenant_game_app_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(TenantGameAppEntity), nameof(TenantGameAppEntity)));
    }

    [Fact]
    public void TenantGameAppDeviceEntity_IsTenantDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantGameAppDeviceEntity)));
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(TenantGameAppDeviceEntity)));
        Assert.True(SqlSugarEntityBoundary.RequiresTenantContext(typeof(TenantGameAppDeviceEntity)));
    }

    [Fact]
    public void TenantUser_IsTenantDatabase()
    {
        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantUserEntity)));
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantUserEntity)));
        Assert.True(SqlSugarEntityBoundary.RequiresTenantContext(typeof(TenantUserEntity)));
        Assert.Equal("tenant_user_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(TenantUserEntity), nameof(TenantUserEntity)));
    }

    [Fact]
    public void TenantGameEntity_IsNotInitializedInMainDatabase()
    {
        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantGameAppEntity)));
    }

    [Fact]
    public void TenantGameAppEntityFamily_UsesTenantGameAppPrefixAndEntitySuffix()
    {
        var entityTypes = new[]
        {
            typeof(TenantGameAppEntity),
            typeof(TenantGameAppPackageEntity),
            typeof(TenantGameAppOpenAccessEntity),
            typeof(TenantGameAppAssetPackageEntity),
            typeof(TenantGameAppAssetPackageVersionEntity),
            typeof(TenantGameAppAssetPackageVersionHistoryEntity),
            typeof(TenantGameAppRemoteConfigEntity),
            typeof(TenantGameAppRemoteConfigKeyEntity),
            typeof(TenantGameAppRemoteConfigModeEntity),
            typeof(TenantGameAppReleaseVersionEntity),
            typeof(TenantGameAppVersionDefinitionEntity),
        };

        var failures = entityTypes
                       .Where(type => !type.Name.StartsWith("TenantGameApp", StringComparison.Ordinal)
                                      || !type.Name.EndsWith("Entity", StringComparison.Ordinal))
                       .Select(type => type.Name)
                       .ToList();

        Assert.Empty(failures);
    }

    [Fact]
    public void HostedSchedulerEntity_IsInitializedInMainDatabaseForCompatibility()
    {
        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantGameTimeJobEntity)));
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantGameTimeJobEntity)));
        Assert.True(SqlSugarEntityBoundary.RequiresTenantContext(typeof(TenantGameTimeJobEntity)));
    }

    [Fact]
    public void OnlineUserEntity_IsInitializedInMainDatabaseForCompatibility()
    {
        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantOnlineUserEntity)));
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantOnlineUserEntity)));
    }

    [Fact]
    public void SystemLogEntity_IsPlatformMainDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemLogOperationEntity)));
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(SystemLogOperationEntity)));
        Assert.False(SqlSugarEntityBoundary.IsTenantTable(typeof(SystemLogOperationEntity)));
        Assert.False(SqlSugarEntityBoundary.RequiresTenantContext(typeof(SystemLogOperationEntity)));
        Assert.Equal("system_log_operation_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(SystemLogOperationEntity), nameof(SystemLogOperationEntity)));
    }

    [Fact]
    public void TenantLogEntity_IsTenantDatabase()
    {
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(TenantLogOperationEntity)));
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantLogOperationEntity)));
        Assert.True(SqlSugarEntityBoundary.RequiresTenantContext(typeof(TenantLogOperationEntity)));
        Assert.Equal("tenant_log_operation_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(TenantLogOperationEntity), nameof(TenantLogOperationEntity)));
    }

    [Fact]
    public void SystemUser_IsSystemDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(SystemUserEntity)));
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemUserEntity)));
        Assert.False(SqlSugarEntityBoundary.IsTenantTable(typeof(SystemUserEntity)));
        Assert.Equal("system_user_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(SystemUserEntity), nameof(SystemUserEntity)));
    }

    [Fact]
    public void GlobalMenu_IsGlobalDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(GlobalMenuEntity)));
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(GlobalMenuEntity)));
        Assert.False(SqlSugarEntityBoundary.IsTenantTable(typeof(GlobalMenuEntity)));
        Assert.Equal("global_menu_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(GlobalMenuEntity), nameof(GlobalMenuEntity)));
    }

    [Fact]
    public void DictionaryEntities_AreGlobalDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(GlobalDictionaryTypeEntity)));
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(GlobalDictionaryTypeEntity)));
        Assert.False(SqlSugarEntityBoundary.IsTenantTable(typeof(GlobalDictionaryTypeEntity)));
        Assert.False(SqlSugarEntityBoundary.RequiresTenantContext(typeof(GlobalDictionaryTypeEntity)));
        Assert.Equal("global_dictionary_type_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(GlobalDictionaryTypeEntity), nameof(GlobalDictionaryTypeEntity)));

        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(GlobalDictionaryDataEntity)));
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(GlobalDictionaryDataEntity)));
        Assert.False(SqlSugarEntityBoundary.IsTenantTable(typeof(GlobalDictionaryDataEntity)));
        Assert.False(SqlSugarEntityBoundary.RequiresTenantContext(typeof(GlobalDictionaryDataEntity)));
        Assert.Equal("global_dictionary_data_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(GlobalDictionaryDataEntity), nameof(GlobalDictionaryDataEntity)));
    }

    [Fact]
    public void ConfigEntities_AreSplitByVisibility()
    {
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemConfigEntity)));
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(SystemConfigEntity)));
        Assert.Equal("system_config_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(SystemConfigEntity), nameof(SystemConfigEntity)));

        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(GlobalConfigEntity)));
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(GlobalConfigEntity)));
        Assert.Equal("global_config_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(GlobalConfigEntity), nameof(GlobalConfigEntity)));
    }

    [Fact]
    public void EmailConfigEntities_AreSplitBetweenSystemAndTenantDatabases()
    {
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemEmailProviderEntity)));
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(SystemEmailProviderEntity)));
        Assert.False(SqlSugarEntityBoundary.IsTenantTable(typeof(SystemEmailProviderEntity)));
        Assert.Equal("system_email_provider_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(SystemEmailProviderEntity), nameof(SystemEmailProviderEntity)));

        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantEmailConfigEntity)));
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantEmailConfigEntity)));
        Assert.True(SqlSugarEntityBoundary.RequiresTenantContext(typeof(TenantEmailConfigEntity)));
        Assert.Equal("tenant_email_config_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(TenantEmailConfigEntity), nameof(TenantEmailConfigEntity)));
    }

    [Fact]
    public void ChannelDefinitionEntities_AreSplitByOwnership()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(GlobalChannelDefinitionEntity)));
        Assert.False(SqlSugarEntityBoundary.IsTenantTable(typeof(GlobalChannelDefinitionEntity)));
        Assert.Equal("global_channel_definition_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(GlobalChannelDefinitionEntity), nameof(GlobalChannelDefinitionEntity)));

        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantChannelDefinitionEntity)));
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantChannelDefinitionEntity)));
        Assert.Equal("tenant_channel_definition_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(TenantChannelDefinitionEntity), nameof(TenantChannelDefinitionEntity)));
    }

    [Fact]
    public void UnclassifiedEntity_IsNotMainDatabaseTable()
    {
        Assert.Throws<InvalidOperationException>(() => SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(UnclassifiedEntity)));
    }

    private sealed class UnclassifiedEntity
    {
    }
}