using System.Security.Cryptography;
using GameFrameX.Foundation.Logger;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Tenant.LoginTicket.TenantGameLoginTicketKey.Dto;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.User;

#pragma warning disable CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Tenant.LoginTicket.TenantGameLoginTicketKey;

/// <summary>
/// 租户 Game 登录票据签名密钥管理服务。
/// <para>私钥永不回显：所有输出仅含公钥与元数据。密钥由系统签发或手动轮换生成，不支持手动新增 / 删除。</para>
/// <para>作用域：租户 + 应用。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 114)]
public class TenantGameLoginTicketKeyEntityService : BaseService<TenantGameLoginTicketKeyEntity>
{
    private readonly CurrentUserContext _currentUserContext;

    public TenantGameLoginTicketKeyEntityService(SqlSugarRepository<TenantGameLoginTicketKeyEntity> repository, CurrentUserContext currentUserContext)
        : base(repository)
    {
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// 分页查询签名密钥（仅公钥与元数据，私钥永不回显）。
    /// </summary>
    [HttpPost("page")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantGameLoginTicketKeyResponse>> PageAsync(PageTenantGameLoginTicketKeyRequest input)
    {
        var appId = input.AppId;
        return await Repository.AsQueryable()
                               .Where(u => u.IsDeleted == false)
                               .ApplyAppIdFilter(appId)
                               .WhereIF(input.IsActive.HasValue, u => u.IsEnabled == input.IsActive.Value)
                               .OrderBy(u => u.CreatedTime, OrderByType.Desc)
                               .Select<TenantGameLoginTicketKeyResponse>()
                               .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 密钥详情（仅公钥与元数据，私钥永不回显）。
    /// </summary>
    [HttpGet("detail")]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<TenantGameLoginTicketKeyResponse> DetailAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        return entity.Adapt<TenantGameLoginTicketKeyResponse>();
    }

    /// <summary>
    /// 手动轮换（C373）：立即为指定应用生成新 key（ExpireTime 为 null 则永不过期）；
    /// 旧 key 打 RotatedTime 标记并收窄到期时间到退场时刻（默认 now + 7 天、上限 now + 30 天），
    /// 到点被生效查询自然排除——轮换后旧公钥在退场窗口内仍可验签在途票据，窗口外自然退场。
    /// </summary>
    [HttpPost("rotate")]
    [ApiDescriptionSettings(Name = "Rotate")]
    public async Task RotateAsync(TenantGameLoginTicketKeyRotateRequest input)
    {
        var now = TimerHelper.UnixTimeSeconds();
        var oldKeyExpireTime = input.OldKeyExpireTime ?? now + OldKeyDefaultExpireSeconds;
        if (oldKeyExpireTime <= now)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, "旧密钥退场时间必须晚于当前时间");
        }

        if (oldKeyExpireTime > now + OldKeyMaxExpireSeconds)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, "旧密钥退场时间不能超过 30 天");
        }

        using (var rsa = RSA.Create(2048))
        {
            var entity = new TenantGameLoginTicketKeyEntity
            {
                AppId = input.AppId,
                Kid = $"admin-rotate-app-{input.AppId}-{TimerHelper.TimestampSecondsToDateTime(now, true):yyyyMMddHHmmss}",
                Algorithm = "RS256",
                PrivateKeyPem = TenantSecretCrypto.Encrypt(rsa.ExportPkcs8PrivateKeyPem()) ?? string.Empty,
                PublicKeyPem = rsa.ExportSubjectPublicKeyInfoPem(),
                NotBeforeTime = now,
                ExpireTime = input.ExpireTime,
                IsEnabled = true,
                Remark = "Admin 手动轮换生成",
            };
            await Repository.InsertAsync(entity);
            await MarkOldKeyRotatedAsync(input.AppId, entity.Id, now, oldKeyExpireTime);

            // 审计（C373）：密钥生命周期操作结构化审计行；DB 操作日志管线断链期先落应用日志，管线修复后统一接入
            LogHelper.Info($"[KeyAudit] action=TicketKeyRotate tenant={_currentUserContext.TenantId} app={entity.AppId} newKid={entity.Kid} oldKeyExpireTime={oldKeyExpireTime} operator={_currentUserContext.Account}");
        }
    }

    /// <summary>
    /// 旧钥退场默认窗口：7 天。
    /// </summary>
    public const int OldKeyDefaultExpireSeconds = 7 * 24 * 3600;

    /// <summary>
    /// 旧钥退场上限窗口：30 天（对齐票据固定到期 30 天上限口径，C142）。
    /// </summary>
    public const int OldKeyMaxExpireSeconds = 30 * 24 * 3600;

    /// <summary>
    /// 标记旧钥退场：最新一把未标记的生效旧钥打 RotatedTime=now 并收窄 ExpireTime（与传输签名密钥环 MarkOldKeyRotatedAsync 同模型）。
    /// </summary>
    private async Task MarkOldKeyRotatedAsync(long appId, long newKeyId, long now, long oldKeyExpireTime)
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
        oldKey.ExpireTime = oldKeyExpireTime;
        await Repository.AsUpdateable(oldKey).UpdateColumns(e => new { e.RotatedTime, e.ExpireTime, }).ExecuteCommandAsync();
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
    /// 禁用密钥。
    /// </summary>
    [HttpPost("disable")]
    [ApiDescriptionSettings(Name = "Disable")]
    public async Task DisableAsync(BaseIdRequest input)
    {
        await SetEnabledAsync(input.Id, false);
    }

    /// <summary>
    /// 禁止删除签名密钥：历史 key 是 Server 验签在途票据的依据，删除会导致验签失败；请改用禁用或轮换。
    /// </summary>
    public override async Task DeleteAsync(BaseDeleteRequest input)
    {
        throw BusinessError.Oh("签名密钥不允许删除，请改用禁用或轮换");
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
}
#pragma warning restore CS8629