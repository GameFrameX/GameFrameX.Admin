using GameFrameX.Application.Game.Tenant.Security.TenantSessionKey;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Client.Api.Services.Auth;

/// <summary>
/// 登录期会话密钥签发服务。
/// <para>每次登录生成一把 Kid 会话密钥写入会话密钥环（<see cref="TenantSessionKeyEntity"/>，AES-256-GCM 信封对称密钥），
/// NotBefore=now、Expire 与 AccessToken 到期时间对齐，随登录响应一次性回显 Kid/Secret。</para>
/// <para>Kid 使用随机标识；Secret 复用管理面 <see cref="TenantSessionKeyEntityService"/> 的安全随机源。</para>
/// </summary>
public sealed class ClientApiSessionKeyIssuer
{
    private readonly SqlSugarRepository<TenantSessionKeyEntity> _repository;

    /// <summary>
    ///
    /// </summary>
    /// <param name="repository">会话密钥仓储（主库）。</param>
    public ClientApiSessionKeyIssuer(SqlSugarRepository<TenantSessionKeyEntity> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 签发一把 per-session 会话密钥并落库，同时懒清理主库已过期密钥。
    /// </summary>
    /// <param name="expireAtUnixSeconds">到期时间（Unix 秒），与 AccessToken ExpireTime 对齐。</param>
    public async Task<ClientApiSessionKeyIssueResponse> IssueAsync(long expireAtUnixSeconds)
    {
        var now = TimerHelper.UnixTimeSeconds();

        // ponytail: 每次登录插一行，过期行（ExpireTime < now）靠下一次登录签发顺带硬删；
        // 若登录频次高到清理追不上堆积，需升级为定时清理任务。硬删不触碰禁用/已软删行（连接级查询过滤器自动叠加）。
        await _repository.AsDeleteable()
                         .Where(e => e.ExpireTime != null && e.ExpireTime < now)
                         .ExecuteCommandAsync();

        var plainSecret = TenantSessionKeyEntityService.GenerateSecret();
        var entity = new TenantSessionKeyEntity
        {
            Kid = Guid.NewGuid().ToString("N"),
            Secret = GameFrameX.Core.Service.Tenant.TenantSecretCrypto.Encrypt(plainSecret) ?? string.Empty,
            NotBeforeTime = now,
            ExpireTime = expireAtUnixSeconds,
            IsEnabled = true,
            Remark = "C7 auto-issue on login",
        };
        await _repository.InsertAsync(entity);

        // 登录响应一次性回显明文（C371：库内密文，响应明文，与 Admin Add/Rotate 同构）
        return new ClientApiSessionKeyIssueResponse(entity.Kid, plainSecret, expireAtUnixSeconds);
    }
}