using GameFrameX.Entity.Tenant.Game.App;
using System.Collections.Concurrent;
using GameFrameX.Entity.Enum;
using SqlSugar;

namespace GameFrameX.Client.Api.Services.PackageVersion;

/// <summary>
/// 包名解析服务：将子包包名解析为母包包名，供客户端 Fallback 匹配器与远程配置投放做第 2 轮母包回退。
/// </summary>
/// <remarks>
/// 运行时继承语义（与 <see cref="TenantGameAppPackageEntity.PlatformInheritanceMode"/> 对齐）：
/// <list type="bullet">
/// <item><description><see cref="InheritanceModeEnum.Inherit"/>：子包允许母包回退，参与子包名→母包名映射。</description></item>
/// <item><description><see cref="InheritanceModeEnum.Override"/>：子包自洽配置，不回退母包（Resolve 返回子包原名）。</description></item>
/// <item><description><see cref="InheritanceModeEnum.Disable"/>：显式禁用，不回退母包（Resolve 返回子包原名）。</description></item>
/// </list>
/// 仅纳入 <c>IsEnabled == true</c> 且未软删除的启用分包；禁用子包或禁用母包都不参与回退。
/// 缓存刷新信号为复合值（参与继承的启用分包数量 + 最大更新时间），任一分包的写入、启停、父子关系重指、继承模式切换或母包改名都会触发刷新。
/// </remarks>
public class GameAppPackageResolver
{
    private readonly ConcurrentDictionary<long, TenantPackageCache> _tenantCaches = new();
    private readonly ClientApiTenantDbProvider _tenantDbProvider;
    private readonly ISqlSugarTenantIdResolver _tenantIdResolver;

    public GameAppPackageResolver(ClientApiTenantDbProvider tenantDbProvider, ISqlSugarTenantIdResolver tenantIdResolver)
    {
        _tenantDbProvider = tenantDbProvider;
        _tenantIdResolver = tenantIdResolver;
    }

    /// <summary>
    /// 启动时调用：加载全量映射 + 记录当前版本号
    /// </summary>
    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// 定时调用（每 30 秒）：检查版本号，变化时刷新缓存
    /// </summary>
    public async Task CheckAndRefreshAsync()
    {
        var tenantId = _tenantIdResolver.GetTenantId();
        if (tenantId.HasValue)
        {
            RefreshCacheIfChanged(tenantId.Value);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 解析包名（纯内存查找，0 次 DB）。仅当子包启用且继承模式为 <see cref="InheritanceModeEnum.Inherit"/> 时返回母包名；否则原样返回子包名。
    /// </summary>
    public string Resolve(string sourcePackageName)
    {
        var tenantId = _tenantIdResolver.GetTenantId();
        if (!tenantId.HasValue)
        {
            return sourcePackageName;
        }

        var cache = RefreshCacheIfChanged(tenantId.Value).Packages;
        return cache.TryGetValue(sourcePackageName, out var masterName)
                   ? masterName
                   : sourcePackageName;
    }

    /// <summary>
    /// 从数据库加载映射
    /// </summary>
    private TenantPackageCache RefreshCacheIfChanged(long tenantId)
    {
        var db = _tenantDbProvider.GetTenantDb(tenantId);
        var signal = GetSignal(db);
        if (_tenantCaches.TryGetValue(tenantId, out var existing) && existing.Signal == signal)
        {
            return existing;
        }

        var refreshed = RefreshCache(db, signal);
        _tenantCaches[tenantId] = refreshed;
        return refreshed;
    }

    private static TenantPackageCache RefreshCache(ISqlSugarClient db, PackageCacheSignal signal)
    {
        // 仅"继承"模式的启用子包才允许母包回退；Override/DisableAsync 与禁用子包不进入映射。
        var children = db.Queryable<TenantGameAppPackageEntity>()
                         .Where(p => p.IsEnabled == true && p.IsDeleted == false)
                         .Where(p => p.ParentPackageId != null)
                         .Where(p => p.PlatformInheritanceMode == InheritanceModeEnum.Inherit)
                         .ToList();

        var masterIds = children.Select(p => p.ParentPackageId!.Value).Distinct().ToList();
        var masters = masterIds.Count > 0
                          ? db.Queryable<TenantGameAppPackageEntity>()
                              .Where(p => p.IsEnabled == true && p.IsDeleted == false)
                              .In(masterIds)
                              .ToList()
                          : new List<TenantGameAppPackageEntity>();

        var cache = new ConcurrentDictionary<string, string>();
        foreach (var pkg in children)
        {
            var masterName = masters.FirstOrDefault(m => m.Id == pkg.ParentPackageId)?.PackageName;
            if (masterName != null)
            {
                cache[pkg.PackageName] = masterName;
            }
        }

        return new TenantPackageCache(signal, cache);
    }

    /// <summary>
    /// 读取数据库缓存刷新信号：参与继承的启用分包数量 + 最大更新时间。
    /// 参与集 = 启用且未删除、且（有母包 或 自身是母包）的分包，覆盖子包与母包两侧的写入、启停、关系重指与母包改名。
    /// </summary>
    private static PackageCacheSignal GetSignal(ISqlSugarClient db)
    {
        var participantQuery = db.Queryable<TenantGameAppPackageEntity>()
                                 .Where(p => p.IsEnabled == true && p.IsDeleted == false)
                                 .Where(p => p.ParentPackageId != null || p.PackageKind == PackageKindEnum.Master);

        var count = participantQuery.Count();
        var maxUpdateTime = participantQuery.Max<long?>(p => p.UpdateTime) ?? 0L;
        return new PackageCacheSignal(count, maxUpdateTime);
    }

    private readonly struct PackageCacheSignal : IEquatable<PackageCacheSignal>
    {
        public readonly int Count;
        public readonly long MaxUpdateTime;

        public PackageCacheSignal(int count, long maxUpdateTime)
        {
            Count = count;
            MaxUpdateTime = maxUpdateTime;
        }

        public bool Equals(PackageCacheSignal other)
        {
            return Count == other.Count && MaxUpdateTime == other.MaxUpdateTime;
        }

        public override bool Equals(object? obj)
        {
            return obj is PackageCacheSignal other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Count, MaxUpdateTime);
        }

        public static bool operator ==(PackageCacheSignal left, PackageCacheSignal right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PackageCacheSignal left, PackageCacheSignal right)
        {
            return !left.Equals(right);
        }
    }

    private sealed class TenantPackageCache
    {
        public TenantPackageCache(PackageCacheSignal signal, ConcurrentDictionary<string, string> packages)
        {
            Signal = signal;
            Packages = packages;
        }

        public PackageCacheSignal Signal { get; }

        public ConcurrentDictionary<string, string> Packages { get; }
    }
}