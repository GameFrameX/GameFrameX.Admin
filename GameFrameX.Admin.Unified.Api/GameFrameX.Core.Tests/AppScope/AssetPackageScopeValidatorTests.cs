using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.Resource;
using SqlSugar;

namespace GameFrameX.Core.Tests.AppScope;

/// <summary>
/// C1 回归测试：资源包版本引用资源包定义时，必须限定在当前应用内。
/// </summary>
public sealed class AssetPackageScopeValidatorTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), string.Format("asset_package_scope_{0:N}.db", Guid.NewGuid()));
    private readonly SqlSugarScope _db;

    public AssetPackageScopeValidatorTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = string.Format("DataSource={0}", _dbPath),
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(TenantGameAppAssetPackageEntity));
    }

    /// <summary>
    /// 同应用下存在资源包定义时放行。
    /// </summary>
    [Fact]
    public async Task AssetPackageBelongsToCurrentApp_Passes()
    {
        await SeedAsync();

        await AssetPackageScopeValidator.ValidateAsync(100, "base", Query());
    }

    /// <summary>
    /// 同名资源包属于其它应用时拒绝，避免跨应用引用。
    /// </summary>
    [Fact]
    public async Task AssetPackageBelongsToOtherApp_Throws()
    {
        await SeedAsync();

        Assert.ThrowsAny<Exception>(() =>
                                        AssetPackageScopeValidator.ValidateAsync(200, "base", Query()).GetAwaiter().GetResult());
    }

    /// <summary>
    /// 缺少应用时拒绝，资源包版本不能落到租户默认层。
    /// </summary>
    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(null)]
    public async Task MissingAppId_Throws(long? appId)
    {
        await SeedAsync();

        Assert.ThrowsAny<Exception>(() =>
                                        AssetPackageScopeValidator.ValidateAsync(appId, "base", Query()).GetAwaiter().GetResult());
    }

    /// <summary>
    /// 空资源包名称拒绝。
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task MissingAssetPackageName_Throws(string? assetPackageName)
    {
        await SeedAsync();

        Assert.ThrowsAny<Exception>(() =>
                                        AssetPackageScopeValidator.ValidateAsync(100, assetPackageName, Query()).GetAwaiter().GetResult());
    }

    private ISugarQueryable<TenantGameAppAssetPackageEntity> Query()
    {
        return _db.Queryable<TenantGameAppAssetPackageEntity>();
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.Insertable(BuildAssetPackage(1, 100, "base", now)).ExecuteCommandAsync();
        await _db.Insertable(BuildAssetPackage(2, 200, "patch", now)).ExecuteCommandAsync();
    }

    private static TenantGameAppAssetPackageEntity BuildAssetPackage(long id, long gameAppId, string name, long now)
    {
        return new TenantGameAppAssetPackageEntity
        {
            Id = id,
            GameAppId = gameAppId,
            Name = name,
            Description = "资源包定义",
            TenantId = null,
            CreatedTime = now,
            IsDeleted = false,
        };
    }

    public void Dispose()
    {
        _db.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}