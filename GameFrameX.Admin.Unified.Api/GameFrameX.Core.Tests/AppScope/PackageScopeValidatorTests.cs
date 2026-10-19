using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.App;
using SqlSugar;

namespace GameFrameX.Core.Tests.AppScope;

/// <summary>
/// GFX-84 / C80 回归测试：PackageScopeValidator 分包级配置包名作用域校验。
/// <para>规则集：</para>
/// <list type="number">
///   <item>package 为空 / "default" → 兜底放行（AppId 可为 0 / null）。</item>
///   <item>具体包名 + AppId&gt;0 → 必须命中当前应用下启用分包，否则抛错。</item>
///   <item>具体包名 + AppId&lt;=0（租户默认层）→ 抛错。</item>
/// </list>
/// <para>用 Sqlite 内存文件库 + ConfigureBaseFilters（软删除过滤）端到端验证；
/// 校验器内部用 历史框架 <c>BusinessError.Oh</c> 抛友好异常，无 历史框架 host 的纯单测下友好异常构造可能触发
/// Friendly 静态初始化异常，故失败用例只断言「抛异常」，消息文本正确性由 code review 把关
/// （同 <c>ObjectStorageActiveConfigGuardTests</c> 既有约定）。</para>
/// </summary>
public sealed class PackageScopeValidatorTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"package_scope_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public PackageScopeValidatorTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(TenantGameAppPackageEntity));
    }

    /// <summary>
    /// package="default" / 空 / null：兜底放行，AppId 可为 0 / null，不查库。
    /// </summary>
    [Theory]
    [InlineData(PackageScopeValidator.DefaultPackage)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task DefaultOrEmptyPackage_PassesWithoutAppScope(string? package)
    {
        await SeedAsync();

        // AppId=null / 0 均应放行（兜底语义不要求应用作用域）
        await PackageScopeValidator.ValidateAsync(null, package, Query());
        await PackageScopeValidator.ValidateAsync(0, package, Query());
    }

    /// <summary>
    /// 具体包名 + AppId>0 + 命中当前应用下启用分包 → 放行。
    /// </summary>
    [Fact]
    public async Task SpecificPackage_BelongingToEnabledAppPackage_Passes()
    {
        await SeedAsync();

        // com.app.a 属于 GameAppId=100 且启用
        await PackageScopeValidator.ValidateAsync(100, "com.app.a", Query());
    }

    /// <summary>
    /// 具体包名命中但分包被禁用（IsEnabled=false）→ 抛错。
    /// </summary>
    [Fact]
    public async Task SpecificPackage_DisabledInApp_Throws()
    {
        await SeedAsync();

        // com.app.b 属于 GameAppId=100 但 IsEnabled=false
        Assert.ThrowsAny<Exception>(() =>
                                        PackageScopeValidator.ValidateAsync(100, "com.app.b", Query()).GetAwaiter().GetResult());
    }

    /// <summary>
    /// 具体包名属于其它应用（跨应用）→ 抛错。
    /// </summary>
    [Fact]
    public async Task SpecificPackage_BelongingToOtherApp_Throws()
    {
        await SeedAsync();

        // com.app.c 属于 GameAppId=200，当前作用域 100 下不应命中
        Assert.ThrowsAny<Exception>(() =>
                                        PackageScopeValidator.ValidateAsync(100, "com.app.c", Query()).GetAwaiter().GetResult());
    }

    /// <summary>
    /// 具体包名在当前应用下根本不存在 → 抛错。
    /// </summary>
    [Fact]
    public async Task SpecificPackage_NotExisting_Throws()
    {
        await SeedAsync();

        Assert.ThrowsAny<Exception>(() =>
                                        PackageScopeValidator.ValidateAsync(100, "com.unknown", Query()).GetAwaiter().GetResult());
    }

    /// <summary>
    /// 具体包名 + AppId<=0（租户默认层）→ 抛错，禁止「租户默认 + 具体包名」组合。
    /// </summary>
    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(null)]
    public async Task SpecificPackage_AtTenantDefaultLayer_Throws(long? appId)
    {
        await SeedAsync();

        Assert.ThrowsAny<Exception>(() =>
                                        PackageScopeValidator.ValidateAsync(appId, "com.app.a", Query()).GetAwaiter().GetResult());
    }

    private ISugarQueryable<TenantGameAppPackageEntity> Query()
    {
        return _db.Queryable<TenantGameAppPackageEntity>();
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.Insertable(BuildPackage(1, 100, "com.app.a", true, now)).ExecuteCommandAsync();
        await _db.Insertable(BuildPackage(2, 100, "com.app.b", false, now)).ExecuteCommandAsync();
        await _db.Insertable(BuildPackage(3, 200, "com.app.c", true, now)).ExecuteCommandAsync();
    }

    private static TenantGameAppPackageEntity BuildPackage(long id, long gameAppId, string packageName, bool isEnabled, long now)
    {
        return new TenantGameAppPackageEntity
        {
            Id = id,
            GameAppId = gameAppId,
            PackageName = packageName,
            IsEnabled = isEnabled,
            Name = packageName,
            PlatformCode = "android",
            PlatformName = "Android",
            ChannelCode = "default",
            ChannelName = "默认渠道",
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