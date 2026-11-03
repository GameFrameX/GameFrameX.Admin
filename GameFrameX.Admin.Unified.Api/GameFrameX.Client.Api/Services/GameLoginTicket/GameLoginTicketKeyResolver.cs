using SqlSugar;
using System.Security.Cryptography;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;

namespace GameFrameX.Client.Api.Services.GameLoginTicket;

/// <summary>
/// 数据库驱动的 Game 登录票据签名 Key 解析服务。
/// <para>生产签名唯一路径为主库 RSA（<see cref="TenantGameLoginTicketKeyEntity"/>）；首次签发自动建 key 的开关由运行票据策略 <see cref="IClientApiTenantOptionsProvider"/> 控制。</para>
/// <para>签名 key 默认永不过期（ExpireTime = null）；手动轮换时可指定到期日，到期后被查询自然排除。不再自动预生成轮换。</para>
/// <para>HMAC fallback 随 C13 删除（P1 决策），不再保留兼容分支。</para>
/// </summary>
public sealed class GameLoginTicketKeyResolver : IGameLoginTicketKeyResolver
{
    private readonly ISqlSugarClient _tenantDbProvider;
    private readonly IClientApiTenantOptionsProvider _optionsProvider;

    public GameLoginTicketKeyResolver(ISqlSugarClient tenantDbProvider, IClientApiTenantOptionsProvider optionsProvider)
    {
        _tenantDbProvider = tenantDbProvider;
        _optionsProvider = optionsProvider;
    }

    public async Task<GameLoginTicketSigningKey> ResolveSigningKeyAsync()
    {
        var now = TimerHelper.UnixTimeSeconds();
        var keys = await GetActiveKeyEntitiesAsync(now);
        if (keys.Count == 0)
        {
            var policy = await _optionsProvider.GetGameLoginTicketPolicyAsync();
            if (policy.EnableKeyAutoCreate)
            {
                // ponytail: 首次请求自动建 key 没有分布式锁；多实例高并发可能各自建一把，
                // 建后统一回读 CreatedTime 最新的一把收敛，保证多实例签发用同一把 key。
                await CreateRsaKeyEntityAsync(now);
                keys = await GetActiveKeyEntitiesAsync(now);
            }
        }

        if (keys.Count == 0)
        {
            throw new InvalidOperationException("Game login ticket signing key is not configured.");
        }

        // C372（GFX-838）：签发读取边界解密（C371 起库内为密文；无前缀明文直通兼容存量行；
        // 进程级密文缓存兜底，避免每次签发 PBKDF2 派生）
        var privateKeyPem = GameFrameX.Core.Service.Tenant.TenantSecretCrypto.DecryptCached(keys[0].PrivateKeyPem);
        return GameLoginTicketSigningKey.CreateRsa(keys[0].Kid, privateKeyPem, keys[0].PublicKeyPem);
    }

    public async Task<IReadOnlyCollection<GameLoginTicketPublicKey>> GetPublicKeysAsync()
    {
        var now = TimerHelper.UnixTimeSeconds();
        var keys = await GetActiveKeyEntitiesAsync(now);
        if (keys.Count == 0)
        {
            return Array.Empty<GameLoginTicketPublicKey>();
        }

        return keys.Select(key => new GameLoginTicketPublicKey(key.Kid, key.Algorithm, key.PublicKeyPem, key.NotBeforeTime, key.ExpireTime)).ToList();
    }

    /// <summary>
    /// 查询生效中全部 key：启用、算法匹配、已生效、未过期（ExpireTime 为 null 视为永不过期），按创建时间倒序（最新在前）。
    /// </summary>
    private async Task<List<TenantGameLoginTicketKeyEntity>> GetActiveKeyEntitiesAsync(long now)
    {
        var db = _tenantDbProvider;
        return await db.Queryable<TenantGameLoginTicketKeyEntity>()
                       .Where(x => x.IsEnabled != false &&
                                   x.Algorithm == GameLoginTicketSigningKey.AlgorithmRs256 &&
                                   (x.NotBeforeTime == null || x.NotBeforeTime <= now) &&
                                   (x.ExpireTime == null || x.ExpireTime > now))
                       .OrderByDescending(x => x.CreatedTime)
                       .OrderByDescending(x => x.Id)
                       .ToListAsync();
    }

    /// <summary>
    /// 生成并持久化一把新的 RSA 签名 key，默认永不过期（ExpireTime = null）。
    /// </summary>
    private async Task CreateRsaKeyEntityAsync(long now)
    {
        var db = _tenantDbProvider;
        using (var rsa = RSA.Create(2048))
        {
            var entity = new TenantGameLoginTicketKeyEntity
            {
                Kid = Guid.NewGuid().ToString("N"),
                Algorithm = GameLoginTicketSigningKey.AlgorithmRs256,
                PrivateKeyPem = GameFrameX.Core.Service.Tenant.TenantSecretCrypto.Encrypt(rsa.ExportPkcs8PrivateKeyPem()) ?? string.Empty,
                PublicKeyPem = rsa.ExportSubjectPublicKeyInfoPem(),
                NotBeforeTime = now,
                ExpireTime = null,
                IsEnabled = true,
                Remark = "Client API 自动生成",
            };

            await db.Insertable(entity).ExecuteCommandAsync();
        }
    }
}