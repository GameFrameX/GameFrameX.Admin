using SqlSugar;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Routing;
using GameFrameX.Foundation.Orm.Attribute;

namespace GameFrameX.Core.Tests.SqlSugar;

public class SqlSugarUnifiedConfigTests
{
    // --- ConfigureBaseSettings ---

    [Fact]
    public void ConfigureBaseSettings_SetsRequiredProperties()
    {
        var config = new ConnectionConfig();
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        Assert.NotNull(config.ConfigureExternalServices);
        Assert.Equal(InitKeyType.Attribute, config.InitKeyType);
        Assert.True(config.IsAutoCloseConnection);
        Assert.NotNull(config.MoreSettings);
    }

    [Fact]
    public void ConfigureBaseSettings_EnablesAutoRemoveFilters()
    {
        var config = new ConnectionConfig();
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        Assert.True(config.MoreSettings.IsAutoRemoveDataCache);
        Assert.True(config.MoreSettings.IsAutoDeleteQueryFilter);
        Assert.True(config.MoreSettings.IsAutoUpdateQueryFilter);
    }

    [Fact]
    public void ConfigureBaseSettings_AlwaysAppliesUnderLineConversion()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = "DataSource=:memory:",
            IsAutoCloseConnection = true,
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);

        using var scope = new SqlSugarScope(config);
        var db = scope.GetConnectionScope("");
        db.CodeFirst.InitTables(typeof(UnderlineProbeEntity));

        var entityInfo = db.EntityMaintenance.GetEntityInfo(typeof(UnderlineProbeEntity));
        Assert.Equal("system_underline_probe_entity", entityInfo.DbTableName);
        Assert.Equal("sample_value", entityInfo.Columns.Single(u => u.PropertyName == nameof(UnderlineProbeEntity.SampleValue)).DbColumnName);
    }

    [Fact]
    public void ConfigureBaseSettings_WithoutCacheService_NoCacheSet()
    {
        var config = new ConnectionConfig();
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        Assert.Null(config.ConfigureExternalServices.DataInfoCacheService);
    }

    [Fact]
    public void ConfigureBaseSettings_WithCacheService_SetsCache()
    {
        var config = new ConnectionConfig();
        var mockCache = new TestCacheService();
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config, mockCache);
        Assert.Same(mockCache, config.ConfigureExternalServices.DataInfoCacheService);
    }

    // --- ConfigureBaseFilters ---

    [Fact]
    public void ConfigureBaseFilters_DoesNotThrow()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = "DataSource=:memory:",
            IsAutoCloseConnection = true,
        };
        using var scope = new SqlSugarScope(config);
        var db = scope.GetConnectionScope("");

        var exception = Record.Exception(() => SqlSugarUnifiedConfig.ConfigureBaseFilters(db));
        Assert.Null(exception);
    }

    // --- ConfigureClientFilters ---

    [Fact]
    public void ConfigureClientFilters_DoesNotThrow()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = "DataSource=:memory:",
            IsAutoCloseConnection = true,
        };
        using var scope = new SqlSugarScope(config);
        var db = scope.GetConnectionScope("");

        var exception = Record.Exception(() => SqlSugarUnifiedConfig.ConfigureClientFilters(db));
        Assert.Null(exception);
    }

    /// <summary>
    /// 最小化 ICacheService 实现，用于测试
    /// </summary>
    private class TestCacheService : ICacheService
    {
        public void Add<V>(string key, V value)
        {
        }

        public void Add<V>(string key, V value, int cacheDurationInSeconds)
        {
        }

        public bool ContainsKey<V>(string key)
        {
            return false;
        }

        public V Get<V>(string key)
        {
#pragma warning disable CS8603 // 测试桩 ICacheService：未命中缓存返回 default(V)，引用类型即 null，与 SqlSugar 缓存契约一致
            return default;
#pragma warning restore CS8603
        }

        public IEnumerable<string> GetAllKey<V>()
        {
            return Array.Empty<string>();
        }

        public V GetOrCreate<V>(string key, Func<V> create, int cacheDurationInSeconds = int.MaxValue)
        {
            return create();
        }

        public void Remove<V>(string key)
        {
        }
    }

    [SugarTable("UnderlineProbeEntity")]
    [PlatformTable]
    private sealed class UnderlineProbeEntity
    {
        [SugarColumn(IsPrimaryKey = true)] public long Id { get; set; }

        public string SampleValue { get; set; } = string.Empty;
    }
}