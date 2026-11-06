using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Middleware;

/// <summary>
/// 主库驱动的会话密钥环。每次读取当前启用状态与租约，密文解密使用有界进程缓存。
/// </summary>
public sealed class SessionKeyResolver : ISessionKeyResolver
{
    private readonly ISqlSugarClient _database;

    public SessionKeyResolver(ISqlSugarClient database)
    {
        _database = database;
    }

    /// <inheritdoc />
    public async Task<SessionKeyLookup> ResolveAsync(string kid)
    {
        var entity = await GetKeyAsync(kid);
        if (entity == null)
        {
            return new SessionKeyLookup(SessionKeyLookupResult.NotFound, kid, null);
        }

        var result = Judge(entity, TimerHelper.UnixTimeSeconds());
        return new SessionKeyLookup(result, kid, result == SessionKeyLookupResult.Active ? RuntimeKeyMaterialCache.Decrypt(entity.Secret) : null);
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

    private async Task<TenantSessionKeyEntity?> GetKeyAsync(string kid)
    {
        // 元数据不缓存，跨宿主轮换、同秒撤销、删除与租约修改在下一请求立即生效。
        return await _database.Queryable<TenantSessionKeyEntity>().ClearFilter()
            .Where(key => key.Kid == kid && key.IsDeleted == false).FirstAsync();
    }

}
