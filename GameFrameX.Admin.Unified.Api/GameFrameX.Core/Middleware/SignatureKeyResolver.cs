using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Middleware;

/// <summary>
/// 主库驱动的签名密钥环。每次读取当前启用状态与租约，密文解密使用有界进程缓存。
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
        var entity = await GetKeyAsync(kid);
        if (entity == null)
        {
            return new SignatureKeyLookup(SignatureKeyLookupResult.NotFound, kid, null);
        }

        var result = Judge(entity, TimerHelper.UnixTimeSeconds());
        return new SignatureKeyLookup(result, kid, result == SignatureKeyLookupResult.Active ? RuntimeKeyMaterialCache.Decrypt(entity.Secret) : null);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SignatureKeyMaterial>> GetActiveKeysAsync()
    {
        var now = TimerHelper.UnixTimeSeconds();
        var keys = await _tenantDbProvider.Queryable<TenantAppSignatureKeyEntity>().ClearFilter()
            .Where(key => key.IsDeleted == false && key.IsEnabled == true
                && (key.NotBeforeTime == null || key.NotBeforeTime <= now)
                && (key.ExpireTime == null || key.ExpireTime > now)).ToListAsync();
        return keys.Where(x => Judge(x, now) == SignatureKeyLookupResult.Active)
                   .OrderByDescending(x => x.CreatedTime)
                   .ThenByDescending(x => x.Id)
                   .Select(x => new SignatureKeyMaterial(x.Kid, RuntimeKeyMaterialCache.Decrypt(x.Secret)))
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

    private async Task<TenantAppSignatureKeyEntity?> GetKeyAsync(string kid)
    {
        // 元数据不缓存，跨宿主轮换、同秒撤销、删除与租约修改在下一请求立即生效。
        return await _tenantDbProvider.Queryable<TenantAppSignatureKeyEntity>().ClearFilter()
            .Where(key => key.Kid == kid && key.IsDeleted == false).FirstAsync();
    }

}
