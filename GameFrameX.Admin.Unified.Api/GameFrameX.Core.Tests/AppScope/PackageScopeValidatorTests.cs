using GameFrameX.Application.ClientDelivery.AppScope;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game.App;
using SqlSugar;

namespace GameFrameX.Core.Tests.AppScope;

/// <summary>
/// GFX-84 / C80 回归测试：PackageScopeValidator 分包级配置包名作用域校验。
/// <para>规则集：</para>
/// <list type="number">
///   <item>package 为空 / "default" → 兜底放行，不查库。</item>
///   <item>具体包名 → 必须命中启用中的分包，否则抛错。</item>
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
        _db.CodeFirst.InitTables(typeof(GameAppPackageEntity));
    }

    /// <summary>
    /// package="default" / 空 / null：兜底放行，不查库。
    /// </summary>
    [Theory]
    [InlineData(PackageScopeValidator.DefaultPackage)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task DefaultOrEmptyPackage_PassesWithoutLookup(string? package)
    {
        await SeedAsync();

        // 兜底语义不查库、不要求任何作用域
        await PackageScopeValidator.ValidateAsync(package, Query());
    }

    /// <summary>
    /// 具体包名 + 命中启用分包 → 放行。
    /// </summary>
    [Fact]
    public async Task SpecificPackage_BelongingToEnabledPackage_Passes()
    {
        await SeedAsync();

        // com.app.a 为启用分包
        await PackageScopeValidator.ValidateAsync("com.app.a", Query());
    }

    /// <summary>
    /// 具体包名命中但分包被禁用（IsEnabled=false）→ 抛错。
    /// </summary>
    [Fact]
    public async Task SpecificPackage_DisabledInApp_Throws()
    {
        await SeedAsync();

        // com.app.b 已停用
        Assert.ThrowsAny<Exception>(() =>
                                        PackageScopeValidator.ValidateAsync("com.app.b", Query()).GetAwaiter().GetResult());
    }

    /// <summary>
    /// 主库登记的启用分包可以使用。
    /// </summary>
    [Fact]
    public async Task SpecificPackage_InMainDatabase_Passes()
    {
        await SeedAsync();

        // com.app.c 为另一启用分包（存在性校验与归属无关）
        await PackageScopeValidator.ValidateAsync("com.app.c", Query());
    }

    /// <summary>
    /// 具体包名在当前应用下根本不存在 → 抛错。
    /// </summary>
    [Fact]
    public async Task SpecificPackage_NotExisting_Throws()
    {
        await SeedAsync();

        Assert.ThrowsAny<Exception>(() =>
                                        PackageScopeValidator.ValidateAsync("com.unknown", Query()).GetAwaiter().GetResult());
    }



    private ISugarQueryable<GameAppPackageEntity> Query()
    {
        return _db.Queryable<GameAppPackageEntity>();
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.Insertable(BuildPackage(1, "com.app.a", true, now)).ExecuteCommandAsync();
        await _db.Insertable(BuildPackage(2, "com.app.b", false, now)).ExecuteCommandAsync();
        await _db.Insertable(BuildPackage(3, "com.app.c", true, now)).ExecuteCommandAsync();
    }

    private static GameAppPackageEntity BuildPackage(long id, string packageName, bool isEnabled, long now)
    {
        return new GameAppPackageEntity
        {
            Id = id,
            PackageName = packageName,
            IsEnabled = isEnabled,
            Name = packageName,
            PlatformCode = "android",
            PlatformName = "Android",
            ChannelCode = "default",
            ChannelName = "默认渠道",
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