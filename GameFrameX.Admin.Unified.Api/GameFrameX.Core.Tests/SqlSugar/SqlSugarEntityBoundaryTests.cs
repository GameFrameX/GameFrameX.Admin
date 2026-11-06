using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Entity.Business.ClientApiConfig;
using GameFrameX.Entity.Business.Game.App;
using GameFrameX.Entity.Business.Game.RemoteConfig;
using GameFrameX.Entity.Business.Game.Release;
using GameFrameX.Entity.Business.Game.Resource;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Business.Device;

namespace GameFrameX.Core.Tests.SqlSugar;

public class SqlSugarEntityBoundaryTests
{
    [Fact]
    public void PlatformEntity_IsPlatformDatabase()
    {
    }

    [Fact]
    public void TenantGameEntity_IsTenantDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(GameGoodsEntity)));
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(GameGoodsEntity)));
        Assert.Equal("game_goods_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(GameGoodsEntity), nameof(GameGoodsEntity)));
    }

    [Fact]
    public void TenantGameAppDeviceEntity_IsTenantDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(GameAppDeviceEntity)));
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(GameAppDeviceEntity)));
    }

    [Fact]
    public void TenantUser_IsTenantDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(UserEntity)));
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(UserEntity)));
        Assert.Equal("user_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(UserEntity), nameof(UserEntity)));
    }

    [Fact]
    public void TenantGameEntity_IsInitializedInMainDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(GameGoodsEntity)));
    }

    [Fact]
    public void TenantGameGoodsEntityFamily_UsesTenantGameAppPrefixAndEntitySuffix()
    {
        var entityTypes = new[]
        {
            typeof(GameAppPackageEntity),
            typeof(GameAppOpenAccessEntity),
            typeof(GameAppAssetPackageEntity),
            typeof(GameAppAssetPackageVersionEntity),
            typeof(GameAppAssetPackageVersionHistoryEntity),
            typeof(GameAppRemoteConfigEntity),
            typeof(GameAppRemoteConfigKeyEntity),
            typeof(GameAppRemoteConfigModeEntity),
            typeof(GameAppReleaseVersionEntity),
            typeof(GameAppVersionDefinitionEntity),
        };

        var failures = entityTypes
                       .Where(type => !type.Name.StartsWith("GameApp", StringComparison.Ordinal)
                                      || !type.Name.EndsWith("Entity", StringComparison.Ordinal))
                       .Select(type => type.Name)
                       .ToList();

        Assert.Empty(failures);
    }

    [Fact]
    public void HostedSchedulerEntity_IsInitializedInMainDatabaseForCompatibility()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(GameTimeJobEntity)));
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(GameTimeJobEntity)));
    }

    [Fact]
    public void OnlineUserEntity_IsInitializedInMainDatabaseForCompatibility()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(OnlineUserEntity)));
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(OnlineUserEntity)));
    }

    [Fact]
    public void SystemLogEntity_IsPlatformMainDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemLogOperationEntity)));
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(SystemLogOperationEntity)));
        Assert.False(SqlSugarEntityBoundary.IsBusinessTable(typeof(SystemLogOperationEntity)));
        Assert.Equal("system_log_operation_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(SystemLogOperationEntity), nameof(SystemLogOperationEntity)));
    }

    [Fact]
    public void TenantLogEntity_IsTenantDatabase()
    {
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(LogOperationEntity)));
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(LogOperationEntity)));
        Assert.Equal("log_operation_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(LogOperationEntity), nameof(LogOperationEntity)));
    }

    [Fact]
    public void SystemUser_IsSystemDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(SystemUserEntity)));
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemUserEntity)));
        Assert.False(SqlSugarEntityBoundary.IsBusinessTable(typeof(SystemUserEntity)));
        Assert.Equal("system_user_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(SystemUserEntity), nameof(SystemUserEntity)));
    }

    [Fact]
    public void GlobalMenu_IsGlobalDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(GlobalMenuEntity)));
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(GlobalMenuEntity)));
        Assert.False(SqlSugarEntityBoundary.IsBusinessTable(typeof(GlobalMenuEntity)));
        Assert.Equal("global_menu_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(GlobalMenuEntity), nameof(GlobalMenuEntity)));
    }

    [Fact]
    public void DictionaryEntities_AreGlobalDatabase()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(GlobalDictionaryTypeEntity)));
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(GlobalDictionaryTypeEntity)));
        Assert.False(SqlSugarEntityBoundary.IsBusinessTable(typeof(GlobalDictionaryTypeEntity)));
        Assert.Equal("global_dictionary_type_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(GlobalDictionaryTypeEntity), nameof(GlobalDictionaryTypeEntity)));

        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(GlobalDictionaryDataEntity)));
        Assert.False(SqlSugarEntityBoundary.IsPlatformTable(typeof(GlobalDictionaryDataEntity)));
        Assert.False(SqlSugarEntityBoundary.IsBusinessTable(typeof(GlobalDictionaryDataEntity)));
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
        Assert.False(SqlSugarEntityBoundary.IsBusinessTable(typeof(SystemEmailProviderEntity)));
        Assert.Equal("system_email_provider_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(SystemEmailProviderEntity), nameof(SystemEmailProviderEntity)));

        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(EmailConfigEntity)));
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(EmailConfigEntity)));
        Assert.Equal("email_config_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(EmailConfigEntity), nameof(EmailConfigEntity)));
    }

    [Fact]
    public void ChannelDefinitionEntities_AreSplitByOwnership()
    {
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(GlobalChannelDefinitionEntity)));
        Assert.False(SqlSugarEntityBoundary.IsBusinessTable(typeof(GlobalChannelDefinitionEntity)));
        Assert.Equal("global_channel_definition_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(GlobalChannelDefinitionEntity), nameof(GlobalChannelDefinitionEntity)));

        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(ChannelDefinitionEntity)));
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(ChannelDefinitionEntity)));
        Assert.Equal("channel_definition_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(ChannelDefinitionEntity), nameof(ChannelDefinitionEntity)));
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