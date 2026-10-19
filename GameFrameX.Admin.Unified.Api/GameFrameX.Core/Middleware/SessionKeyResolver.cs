using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Middleware;

/// <summary>
/// <see cref="ISessionKeyResolver"/> 默认实现：租户库驱动的 App 会话密钥环解析（change C5 server-envelope-middleware）。
/// <para>形状照 <see cref="SignatureKeyResolver"/>（C2 先例）：缓存照 <see cref="AppCacheService"/> 同款懒刷新，
/// 访问时比对租户库信号值，变化才整租户重载（密钥行数少，全量代价可忽略）。</para>
/// <para>缓存加载<b>不过滤</b>禁用/租约外 key（60105 需要查到「存在但不可用」的 key 并区分原因），
/// 生效判定在每次请求时以当前时刻重新计算（<see cref="Judge"/>），保证到期/启用变更即时生效。</para>
/// </summary>
public sealed class SessionKeyResolver : ISessionKeyResolver
{
    private readonly ConcurrentDictionary<long, TenantSessionKeyCache> _tenantCaches = new();
    private readonly ITenantDbProvider _tenantDbProvider;
    private readonly ISqlSugarTenantIdResolver _tenantIdResolver;

    public SessionKeyResolver(ITenantDbProvider tenantDbProvider, ISqlSugarTenantIdResolver tenantIdResolver)
    {
        _tenantDbProvider = tenantDbProvider;
        _tenantIdResolver = tenantIdResolver;
    }

    /// <inheritdoc />
    public async Task<SessionKeyLookup> ResolveAsync(long appId, string kid)
    {
        var tenantId = _tenantIdResolver.GetTenantId();
        if (!tenantId.HasValue)
        {
            return new SessionKeyLookup(SessionKeyLookupResult.NotFound, kid, null);
        }

        var keys = await GetTenantKeysAsync(tenantId.Value);
        var entity = keys.FirstOrDefault(x => x.AppId == appId && x.Kid == kid);
        if (entity == null)
        {
            return new SessionKeyLookup(SessionKeyLookupResult.NotFound, kid, null);
        }

        var result = Judge(entity, TimerHelper.UnixTimeSeconds());
        return new SessionKeyLookup(result, kid, result == SessionKeyLookupResult.Active ? entity.Secret : null);
    }

    /// <summary>
    /// 密钥生效判定（纯函数，供单测）：启用 且 NotBefore ≤ now &lt; Expire（null 边界 = 无限制）。
    /// <para>结果到中间件拒收信封码的映射见 <see cref="SessionKeyLookupResult"/>；Expire 判定为严格大于
    /// （now == Expire 即到期），与 SignatureKeyResolver / LoginTicket key 查询口径一致。</para>
    /// </summary>
    internal static SessionKeyLookupResult Judge(TenantSessionKeyEntity key, long now)
    {
        if (key.IsEnabled == false)
        {
            return SessionKeyLookupResult.Disabled;
        }

        if (key.NotBeforeTime.HasValue && key.NotBeforeTime.Value > now)
        {
            return SessionKeyLookupResult.NotYetValid;
        }

        if (key.ExpireTime.HasValue && key.ExpireTime.Value <= now)
        {
            return SessionKeyLookupResult.Expired;
        }

        return SessionKeyLookupResult.Active;
    }

    /// <summary>
    /// 取当前租户全量会话密钥（含禁用/租约外），按需懒刷新。
    /// </summary>
    private async Task<List<TenantSessionKeyEntity>> GetTenantKeysAsync(long tenantId)
    {
        var db = _tenantDbProvider.GetTenantDb(tenantId);
        db.QueryFilter.ClearAndBackup();
        try
        {
            var signal = await GetSignalVersionAsync(db);
            if (_tenantCaches.TryGetValue(tenantId, out var existing) && existing.Signal == signal)
            {
                return existing.Keys;
            }

            var refreshed = await db.Queryable<TenantSessionKeyEntity>().ToListAsync();
            // C372（GFX-838）：会话密钥环缓存加载边界解密（C371 起库内为密文；无前缀明文直通兼容存量行）。
            // 缓存持有明文实体，命中路径零派生；signal 变更整租户重载时未变行由 TenantSecretCrypto 进程级密文缓存兜底。
            foreach (var key in refreshed)
            {
                key.Secret = TenantSecretCrypto.DecryptCached(key.Secret) ?? string.Empty;
            }

            var cache = new TenantSessionKeyCache(signal, refreshed);
            _tenantCaches[tenantId] = cache;
            return refreshed;
        }
        finally
        {
            db.QueryFilter.Restore();
        }
    }

    /// <summary>
    /// 租户库变更信号：MAX(UpdateTime) 与 MAX(CreatedTime) 取大（同 SignatureKeyResolver 口径）。
    /// <para>SqlSugar 审计 AOP 只在 Insert 写 CreatedTime、UpdateAsync 写 UpdateTime，
    /// 纯 MAX(UpdateTime) 对「新插入行」不敏感；而密钥环的主变更路径恰是 AddAsync/Rotate 插入新 key，
    /// 故信号必须叠加 MAX(CreatedTime)，否则管理面新增的 key 在缓存里不可见、解密 60104。</para>
    /// <para>ponytail: MAX 信号无法感知「删除非最大行」（不改变最大值即不触发刷新）；
    /// 本实体 API 层已禁删（DeleteAsync 被 override 封死），仅直连 DB 删除可触发该盲区，
    /// 与 <see cref="AppCacheService"/> 同款天花板。升级路径：改行数 COUNT 参与信号或版本表。</para>
    /// </summary>
    private static async Task<long> GetSignalVersionAsync(ISqlSugarClient db)
    {
        var queryable = db.Queryable<TenantSessionKeyEntity>();
        var maxUpdated = await queryable.MaxAsync<long?>(u => u.UpdateTime) ?? 0;
        var maxCreated = await queryable.MaxAsync<long?>(u => u.CreatedTime) ?? 0;
        return Math.Max(maxUpdated, maxCreated);
    }

    private sealed record TenantSessionKeyCache(long Signal, List<TenantSessionKeyEntity> Keys);
}