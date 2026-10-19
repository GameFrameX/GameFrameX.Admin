using SqlSugar;
using GameFrameX.Core.Option;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

public class SqlSugarSetupRefactorTests
{
    // --- SetDbConfig delegates to ConfigureBaseSettings ---

    [Fact]
    public void SetDbConfig_SetsRequiredProperties()
    {
        var config = new DbConnectionConfig
        {
            DbSettings = new DbSettings(),
        };
        SqlSugarSetup.SetDbConfig(config);
        Assert.NotNull(config.ConfigureExternalServices);
        Assert.Equal(InitKeyType.Attribute, config.InitKeyType);
        Assert.True(config.IsAutoCloseConnection);
        Assert.NotNull(config.MoreSettings);
    }

    [Fact]
    public void SetDbConfig_EnablesAutoRemoveFilters()
    {
        var config = new DbConnectionConfig
        {
            DbSettings = new DbSettings(),
        };
        SqlSugarSetup.SetDbConfig(config);
        Assert.True(config.MoreSettings.IsAutoRemoveDataCache);
        Assert.True(config.MoreSettings.IsAutoDeleteQueryFilter);
        Assert.True(config.MoreSettings.IsAutoUpdateQueryFilter);
    }

    [Fact]
    public void SetDbConfig_SetsCacheService()
    {
        var config = new DbConnectionConfig
        {
            DbSettings = new DbSettings(),
        };
        SqlSugarSetup.SetDbConfig(config);
        Assert.NotNull(config.ConfigureExternalServices.DataInfoCacheService);
    }

    [Fact]
    public void SetDbConfig_WithoutOptionalDbSettings_SetsRequiredProperties()
    {
        var config = new DbConnectionConfig
        {
            DbSettings = new DbSettings(),
        };
        SqlSugarSetup.SetDbConfig(config);
        Assert.NotNull(config.ConfigureExternalServices);
        Assert.Equal(InitKeyType.Attribute, config.InitKeyType);
        Assert.True(config.IsAutoCloseConnection);
    }
}