using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Middleware;

/// <summary>
/// 主库驱动的签名密钥环。每次读取当前启用状态与租约，密文解密使用进程缓存。
/// </summary>
public sealed class SignatureKeyResolver : ISignatureKeyResolver
{
    private readonly ISqlSugarClient _tenantDbProvider;

    public SignatureKeyResolver(ISqlSugarClient tenantDbProvider)
    {
        _tenantDbProvider = tenantDbProvider;
    }

    /// <inheritdoc />
    public async Task<SignatureKeyLookup> ResolveAsync(string kid)
    {
        var keys = await GetKeysAsync();
        var entity = keys.FirstOrDefault(x => x.Kid == kid);
        if (entity == null)
        {
            return new SignatureKeyLookup(SignatureKeyLookupResult.NotFound, kid, null);
        }

        var result = Judge(entity, TimerHelper.UnixTimeSeconds());
        return new SignatureKeyLookup(result, kid, result == SignatureKeyLookupResult.Active ? entity.Secret : null);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SignatureKeyMaterial>> GetActiveKeysAsync()
    {
        var now = TimerHelper.UnixTimeSeconds();
        var keys = await GetKeysAsync();
        return keys.Where(x => Judge(x, now) == SignatureKeyLookupResult.Active)
                   .OrderByDescending(x => x.CreatedTime)
                   .ThenByDescending(x => x.Id)
                   .Select(x => new SignatureKeyMaterial(x.Kid, x.Secret))
                   .ToList();
    }

    /// <summary>
    /// 密钥生效判定（纯函数，供单测）：启用 且 NotBefore ≤ now &lt; Expire（null 边界 = 无限制）。
    /// <para>结果到中间件拒签码的映射见 <see cref="SignatureKeyLookupResult"/>；Expire 判定为严格大于
    /// （now == Expire 即到期），与 LoginTicket key 查询口径一致。</para>
    /// </summary>
    internal static SignatureKeyLookupResult Judge(TenantAppSignatureKeyEntity key, long now)
    {
        if (key.IsEnabled == false)
        {
            return SignatureKeyLookupResult.Disabled;
        }

        if (key.NotBeforeTime.HasValue && key.NotBeforeTime.Value > now)
        {
            return SignatureKeyLookupResult.NotYetValid;
        }

        if (key.ExpireTime.HasValue && key.ExpireTime.Value <= now)
        {
            return SignatureKeyLookupResult.Expired;
        }

        return SignatureKeyLookupResult.Active;
    }

    /// <summary>
    /// 取主库全量密钥（含禁用/租约外），每次读取当前状态。
    /// </summary>
    private async Task<List<TenantAppSignatureKeyEntity>> GetKeysAsync()
    {
        var db = _tenantDbProvider;
        db.QueryFilter.ClearAndBackup();
        try
        {
            var refreshed = await db.Queryable<TenantAppSignatureKeyEntity>().ToListAsync();
            // 状态直接读取主库，避免同秒更新或非最大行删除导致撤销失效。
            foreach (var key in refreshed)
            {
                key.Secret = TenantSecretCrypto.DecryptCached(key.Secret) ?? string.Empty;
            }

            return refreshed;
        }
        finally
        {
            db.QueryFilter.Restore();
        }
    }

}
