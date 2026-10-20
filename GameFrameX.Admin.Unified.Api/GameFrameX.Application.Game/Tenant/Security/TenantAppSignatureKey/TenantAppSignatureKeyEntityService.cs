using System.Security.Cryptography;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Tenant.Security.TenantAppSignatureKey.Dto;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Tenant;

#pragma warning disable CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Value 实际安全
namespace GameFrameX.Application.Game.Tenant.Security.TenantAppSignatureKey;

/// <summary>
/// App 传输签名密钥环管理服务（change C2 sig-keyring-server）。
/// <para>密钥明文仅服务端随机生成（不接受外部提交，避免弱密钥入环），仅 AddAsync/RotateAsync 响应一次性回显；
/// 列表/详情一律脱敏 ******；实体禁删（历史 key 是在途请求与多版本客户端的验签依据），退场走 DisableAsync 或租约到期。</para>
/// <para>作用域：租户 + 应用。路由前缀 /api/tenantAppSignatureKeyEntity（动态路由规则：去 Service 后缀 + lowerCamelCase）。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 114)]
public class TenantAppSignatureKeyEntityService : BaseService<TenantAppSignatureKeyEntity>
{
    public TenantAppSignatureKeyEntityService(SqlSugarRepository<TenantAppSignatureKeyEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 分页查询签名密钥（元数据，密钥固定脱敏 ******，明文永不回显）。
    /// </summary>
    [HttpPost("page")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantAppSignatureKeyResponse>> PageAsync(PageTenantAppSignatureKeyRequest input)
    {
        var appId = input.AppId;
        var page = await Repository.AsQueryable()
                                   .Where(u => u.IsDeleted == false)
                                   .ApplyAppIdFilter(appId, u => u.AppId)
                                   .WhereIF(input.IsActive.HasValue, u => u.IsEnabled == input.IsActive.Value)
                                   .OrderBy(u => u.CreatedTime, OrderByType.Desc)
                                   .Select<TenantAppSignatureKeyResponse>()
                                   .ToPagedListAsync(input.Page, input.PageSize);
        foreach (var item in page.Items)
        {
            // SqlSugar 按列名投影会把实体明文带出，此处强制重置为脱敏占位
            item.Secret = SensitiveFieldMask.MaskValue;
        }

        return page;
    }

    /// <summary>
    /// 新增签名密钥：服务端随机生成 Kid + HMAC 密钥明文，响应一次性回显。
    /// <para>适合为同一应用并行放置多把 key（多版本客户端各持一把互不影响）；密钥域归属 AppId。</para>
    /// </summary>
    [HttpPost("add")]
    [ApiDescriptionSettings(Name = "Add")]
    public async Task<TenantAppSignatureKeyRevealResponse> AddAsync(TenantAppSignatureKeyAddRequest input)
    {
        var now = TimerHelper.UnixTimeSeconds();
        var notBefore = input.NotBeforeTime ?? now;
        ValidateLease(notBefore, input.ExpireTime, now);

        var plainSecret = GenerateSecret();
        var entity = new TenantAppSignatureKeyEntity
        {
            AppId = input.AppId,
            Kid = GenerateKid(input.AppId, now),
            Secret = TenantSecretCrypto.Encrypt(plainSecret) ?? string.Empty,
            NotBeforeTime = notBefore,
            ExpireTime = input.ExpireTime,
            IsEnabled = true,
            Remark = input.Remark ?? "Admin 手动新增",
        };
        await Repository.InsertAsync(entity);

        return ToRevealOutput(entity, plainSecret);
    }

    /// <summary>
    /// 轮换签名密钥（双活语义）：新插一把立即生效的 key，旧 key 不禁用不删除 ——
    /// 新旧在旧 key 到期/禁用前同时可验签，客户端可在双活窗口内任意时刻切换，切换零中断。
    /// <para>当前最新生效 key 会被打 RotatedTime 标记（纯元数据，不影响验签）；
    /// 可选同时收窄旧 key 到期时间（OldKeyExpireTime）以控制双活窗口长度。</para>
    /// </summary>
    [HttpPost("rotate")]
    [ApiDescriptionSettings(Name = "Rotate")]
    public async Task<TenantAppSignatureKeyRevealResponse> RotateAsync(TenantAppSignatureKeyRotateRequest input)
    {
        var now = TimerHelper.UnixTimeSeconds();
        if (input.OldKeyExpireTime.HasValue && input.OldKeyExpireTime.Value <= now)
        {
            throw BusinessError.Oh("旧密钥到期时间必须晚于当前时间");
        }

        var plainSecret = GenerateSecret();
        var entity = new TenantAppSignatureKeyEntity
        {
            AppId = input.AppId,
            Kid = GenerateKid(input.AppId, now),
            Secret = TenantSecretCrypto.Encrypt(plainSecret) ?? string.Empty,
            NotBeforeTime = now,
            ExpireTime = null,
            IsEnabled = true,
            Remark = input.Remark ?? "Admin 手动轮换生成",
        };
        await Repository.InsertAsync(entity);

        await MarkOldKeyRotatedAsync(input.AppId, entity.Id, now, input.OldKeyExpireTime);
        return ToRevealOutput(entity, plainSecret);
    }

    /// <summary>
    /// 启用密钥。
    /// </summary>
    [HttpPost("enable")]
    [ApiDescriptionSettings(Name = "Enable")]
    public async Task EnableAsync(BaseIdRequest input)
    {
        await SetEnabledAsync(input.Id, true);
    }

    /// <summary>
    /// 禁用密钥：立即拒签该 key 的请求（60103），不影响同应用其余 key（单 key 失效不殃及全应用，R5）。
    /// </summary>
    [HttpPost("disable")]
    [ApiDescriptionSettings(Name = "Disable")]
    public async Task DisableAsync(BaseIdRequest input)
    {
        await SetEnabledAsync(input.Id, false);
    }

    /// <summary>
    /// 续租：仅更新到期时间（null = 改为永不过期），不产生新密钥、不改动 Kid 与明文。
    /// <para>典型用途：RotateAsync 后客户端切换进度不及预期，延长旧 key 双活窗口。</para>
    /// </summary>
    [HttpPost("renew")]
    [ApiDescriptionSettings(Name = "Renew")]
    public async Task RenewAsync(TenantAppSignatureKeyRenewRequest input)
    {
        var now = TimerHelper.UnixTimeSeconds();
        if (input.ExpireTime.HasValue && input.ExpireTime.Value <= now)
        {
            throw BusinessError.Oh("续租到期时间必须晚于当前时间");
        }

        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        entity.ExpireTime = input.ExpireTime;
        await Repository.AsUpdateable(entity).UpdateColumns(e => new { e.ExpireTime, }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 禁止删除签名密钥：历史 key 是在途请求与未升级客户端的验签依据，删除会直接 60102 打断存量流量；
    /// 请改用禁用（立即拒签）或到期退场（租约自然排除）。
    /// </summary>
    public override async Task DeleteAsync(BaseDeleteRequest input)
    {
        throw BusinessError.Oh("签名密钥不允许删除，请改用禁用或轮换");
    }

    /// <summary>
    /// 双活收尾：给当前最新生效 key 打轮换标记；可选同时改其到期时间收窄双活窗口。
    /// <para>只挑「启用 + 未被轮换过」的最新一把（排除本轮刚插入的新 key —— CreatedTime 为秒级，
    /// 同秒内排序无法区分新旧），不动已禁用/已过期的历史 key。</para>
    /// </summary>
    private async Task MarkOldKeyRotatedAsync(long appId, long newKeyId, long now, long? oldKeyExpireTime)
    {
        var oldKey = await Repository.AsQueryable()
                                     .Where(e => e.AppId == appId && e.Id != newKeyId && e.IsDeleted == false && e.IsEnabled == true && e.RotatedTime == null
                                                 && (e.ExpireTime == null || e.ExpireTime > now))
                                     .OrderByDescending(e => e.CreatedTime)
                                     .OrderByDescending(e => e.Id)
                                     .FirstAsync();
        if (oldKey == null)
        {
            return;
        }

        oldKey.RotatedTime = now;
        if (oldKeyExpireTime.HasValue)
        {
            oldKey.ExpireTime = oldKeyExpireTime.Value;
        }

        await Repository.AsUpdateable(oldKey).UpdateColumns(e => new { e.RotatedTime, e.ExpireTime, }).ExecuteCommandAsync();
    }

    private async Task SetEnabledAsync(long id, bool enabled)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == id);
        if (entity == null)
        {
            throw BusinessError.Oh("签名密钥不存在");
        }

        entity.IsEnabled = enabled;
        await Repository.AsUpdateable(entity).UpdateColumns(e => new { e.IsEnabled, }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 租约合法性校验：ExpireTime 若提供必须晚于当前时间且晚于 NotBeforeTime（生效区间非空）。
    /// </summary>
    private static void ValidateLease(long notBefore, long? expireTime, long now)
    {
        if (expireTime.HasValue && expireTime.Value <= now)
        {
            throw BusinessError.Oh("密钥到期时间必须晚于当前时间");
        }

        if (expireTime.HasValue && expireTime.Value <= notBefore)
        {
            throw BusinessError.Oh("密钥到期时间必须晚于生效时间");
        }
    }

    /// <summary>
    /// 生成 Key ID：sk-app-{appId}-{时间戳}-{随机后缀}。人可读、wire 稳定（对齐 LoginTicket Kid 风格），
    /// 随机后缀避免同秒并发新增/轮换撞名。
    /// </summary>
    private static string GenerateKid(long appId, long now)
    {
        var suffix = Convert.ToHexString(RandomNumberGenerator.GetBytes(2)).ToLowerInvariant();
        return $"sk-app-{appId}-{TimerHelper.TimestampSecondsToDateTime(now, true):yyyyMMddHHmmss}-{suffix}";
    }

    /// <summary>
    /// 生成 HMAC 密钥明文：32 字节 CSPRNG → 64 位小写 hex（URL/Header 安全，256-bit 熵）。
    /// </summary>
    private static string GenerateSecret()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
    }

    /// <summary>
    /// 一次性回显输出：库内密文（C371），响应携带生成时明文。
    /// </summary>
    private static TenantAppSignatureKeyRevealResponse ToRevealOutput(TenantAppSignatureKeyEntity entity, string plainSecret)
    {
        return new TenantAppSignatureKeyRevealResponse
        {
            Id = entity.Id,
            AppId = entity.AppId,
            Kid = entity.Kid,
            Secret = plainSecret,
            NotBeforeTime = entity.NotBeforeTime,
            ExpireTime = entity.ExpireTime,
        };
    }
}
#pragma warning restore CS8629