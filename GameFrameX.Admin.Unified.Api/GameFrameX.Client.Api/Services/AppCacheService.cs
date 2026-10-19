using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using GameFrameX.Core.Security;
using SqlSugar;

namespace GameFrameX.Client.Api.Services;

/// <summary>
/// 应用缓存服务：AppId → GameApp（内存缓存，定时刷新）
/// </summary>
public class AppCacheService
{
    private readonly ConcurrentDictionary<long, TenantAppCache> _tenantCaches = new();
    private readonly ClientApiTenantDbProvider _tenantDbProvider;
    private readonly ISqlSugarTenantIdResolver _tenantIdResolver;

    public AppCacheService(ClientApiTenantDbProvider tenantDbProvider, ISqlSugarTenantIdResolver tenantIdResolver)
    {
        _tenantDbProvider = tenantDbProvider;
        _tenantIdResolver = tenantIdResolver;
    }

    /// <summary>
    /// 启动时调用：加载全量应用数据
    /// </summary>
    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// 定时调用：检查数据变化（基于 MAX(UpdateTime)），变化时刷新缓存
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
    /// 获取应用信息（纯内存查找）
    /// </summary>
    public bool TryGetApp(long appId, [NotNullWhen(true)] out TenantGameAppEntity? app)
    {
        var tenantId = _tenantIdResolver.GetTenantId();
        if (!tenantId.HasValue)
        {
            app = null;
            return false;
        }

        return RefreshCacheIfChanged(tenantId.Value).Apps.TryGetValue(appId, out app);
    }

    /// <summary>
    /// 验证应用凭证（AppId + AppSecret）。
    /// <para>仅 <see cref="AppSecretHasher"/> 常量时间 hash 比对；C371（GFX-838）起遗留明文比对与懒升级分支已移除。</para>
    /// <para>AppSecret 永不出现在日志或返回值。</para>
    /// </summary>
    public bool ValidateApp(long appId, string appSecret, [NotNullWhen(true)] out TenantGameAppEntity? app)
    {
        var tenantId = _tenantIdResolver.GetTenantId();
        if (!tenantId.HasValue)
        {
            app = null;
            return false;
        }

        if (RefreshCacheIfChanged(tenantId.Value).Apps.TryGetValue(appId, out app))
        {
            // hash 比对（常量时间）；hash 为空 = 未完成清零迁移的行，一律拒绝（fail-closed）
            if (!string.IsNullOrEmpty(app.AppSecretHash))
            {
                return AppSecretHasher.Verify(appSecret, app.AppSecretHash);
            }

            return false;
        }

        app = null;
        return false;
    }

    private TenantAppCache RefreshCacheIfChanged(long tenantId)
    {
        var db = _tenantDbProvider.GetTenantDb(tenantId);
        db.QueryFilter.ClearAndBackup();
        try
        {
            var signal = GetSignalVersion(db);
            if (_tenantCaches.TryGetValue(tenantId, out var existing) && existing.Signal == signal)
            {
                return existing;
            }

            var refreshed = RefreshCache(db, signal);
            _tenantCaches[tenantId] = refreshed;
            return refreshed;
        }
        finally
        {
            db.QueryFilter.Restore();
        }
    }

    private static TenantAppCache RefreshCache(ISqlSugarClient db, long signal)
    {
        var list = db.Queryable<TenantGameAppEntity>().ToList();

        var cache = new ConcurrentDictionary<long, TenantGameAppEntity>();
        foreach (var item in list)
        {
            cache[item.Id] = item;
        }

        return new TenantAppCache(signal, cache);
    }

    private static long GetSignalVersion(ISqlSugarClient db)
    {
        var max = db.Queryable<TenantGameAppEntity>()
                    .Max<long?>(u => u.UpdateTime);
        return max ?? 0;
    }

    private sealed record TenantAppCache(long Signal, ConcurrentDictionary<long, TenantGameAppEntity> Apps);
}