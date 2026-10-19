using System.ComponentModel;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.App.CredentialCenter.Dto;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Entity.Tenant.ObjectStorage;

namespace GameFrameX.Application.Game.App.CredentialCenter;

/// <summary>
/// 凭证中心聚合只读服务（C374）：单端点按应用聚合四类凭证状态卡片（U14 四类凭证分类法）。
/// <para>路由 <c>GET api/tenantAppCredentialOverview/get?appId=</c>；只读、零密钥材料（仅状态/计数/时间戳/路由元数据，
/// Response 图由 <c>TenantAppCredentialOverviewTests</c> 反射守护）。写操作走各实体既有端点，本服务不合并权限码。</para>
/// <para>应用归属校验复用 <see cref="GameAppOwnershipValidator"/>（C194，依赖仓储租户隔离：查不到即不属于当前租户）。</para>
/// <para>密钥「可用」口径与运行时验签一致：启用 + 未删 + 已生效（NotBefore ≤ now）+ 未到期（Expire 为空或 &gt; now）；
/// RotatedTime 是纯元数据（C373 租约模型），已标记旧钥在退场时刻前仍计入可用。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 113)]
public class TenantAppCredentialOverviewService : IAdminApiController
{
    private readonly SqlSugarRepository<TenantGameAppEntity> _gameAppRepository;
    private readonly SqlSugarRepository<TenantAppSignatureKeyEntity> _signatureKeyRepository;
    private readonly SqlSugarRepository<TenantGameAppOpenAccessEntity> _openAccessRepository;
    private readonly SqlSugarRepository<TenantGameLoginTicketKeyEntity> _ticketKeyRepository;
    private readonly SqlSugarRepository<TenantClientApiPolicyEntity> _policyRepository;
    private readonly SqlSugarRepository<TenantSessionKeyEntity> _sessionKeyRepository;
    private readonly SqlSugarRepository<TenantGamePaymentConfigEntity> _paymentRepository;
    private readonly SqlSugarRepository<TenantEmailConfigEntity> _emailRepository;
    private readonly SqlSugarRepository<TenantSmsConfigEntity> _smsRepository;
    private readonly SqlSugarRepository<TenantWechatConfigEntity> _wechatRepository;
    private readonly SqlSugarRepository<TenantAdvertisementCallbackConfigEntity> _advertisementCallbackRepository;
    private readonly SqlSugarRepository<TenantObjectStorageConfigEntity> _objectStorageRepository;

    /// <summary>
    /// 构造函数（各凭证实体独立仓储，依赖多租户仓储租户隔离过滤）。
    /// </summary>
    public TenantAppCredentialOverviewService(
        SqlSugarRepository<TenantGameAppEntity> gameAppRepository,
        SqlSugarRepository<TenantAppSignatureKeyEntity> signatureKeyRepository,
        SqlSugarRepository<TenantGameAppOpenAccessEntity> openAccessRepository,
        SqlSugarRepository<TenantGameLoginTicketKeyEntity> ticketKeyRepository,
        SqlSugarRepository<TenantClientApiPolicyEntity> policyRepository,
        SqlSugarRepository<TenantSessionKeyEntity> sessionKeyRepository,
        SqlSugarRepository<TenantGamePaymentConfigEntity> paymentRepository,
        SqlSugarRepository<TenantEmailConfigEntity> emailRepository,
        SqlSugarRepository<TenantSmsConfigEntity> smsRepository,
        SqlSugarRepository<TenantWechatConfigEntity> wechatRepository,
        SqlSugarRepository<TenantAdvertisementCallbackConfigEntity> advertisementCallbackRepository,
        SqlSugarRepository<TenantObjectStorageConfigEntity> objectStorageRepository)
    {
        _gameAppRepository = gameAppRepository;
        _signatureKeyRepository = signatureKeyRepository;
        _openAccessRepository = openAccessRepository;
        _ticketKeyRepository = ticketKeyRepository;
        _policyRepository = policyRepository;
        _sessionKeyRepository = sessionKeyRepository;
        _paymentRepository = paymentRepository;
        _emailRepository = emailRepository;
        _smsRepository = smsRepository;
        _wechatRepository = wechatRepository;
        _advertisementCallbackRepository = advertisementCallbackRepository;
        _objectStorageRepository = objectStorageRepository;
    }

    /// <summary>
    /// 应用凭证概览：① 应用身份 ② 传输签名 ③ 票据与令牌 ④ 出站对接四类卡片 + 用途矩阵（后端常量单一事实来源）。
    /// </summary>
    /// <param name="appId">目标应用 Id（须属于当前租户）。</param>
    [HttpGet("get")]
    [ApiDescriptionSettings(Name = "Get")]
    [DisplayName("应用凭证概览")]
    public async Task<TenantAppCredentialOverviewResponse> GetAsync([FromQuery] long appId)
    {
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(appId);
        var app = await _gameAppRepository.GetFirstAsync(u => u.Id == appId && u.IsDeleted == false);
        var now = TimerHelper.UnixTimeSeconds();

        // 密钥环「可用」行（三处谓词一致：启用+已生效+未到期；RotatedTime 纯元数据不排除）
        var signatureKeys = await _signatureKeyRepository.AsQueryable()
                                .Where(u => u.AppId == appId && u.IsDeleted == false && u.IsEnabled == true
                                            && (u.NotBeforeTime == null || u.NotBeforeTime <= now)
                                            && (u.ExpireTime == null || u.ExpireTime > now))
                                .ToListAsync();
        var ticketKeys = await _ticketKeyRepository.AsQueryable()
                             .Where(u => u.AppId == appId && u.IsDeleted == false && u.IsEnabled == true
                                         && (u.NotBeforeTime == null || u.NotBeforeTime <= now)
                                         && (u.ExpireTime == null || u.ExpireTime > now))
                             .ToListAsync();
        var sessionKeys = await _sessionKeyRepository.AsQueryable()
                              .Where(u => u.AppId == appId && u.IsDeleted == false && u.IsEnabled == true
                                          && (u.NotBeforeTime == null || u.NotBeforeTime <= now)
                                          && (u.ExpireTime == null || u.ExpireTime > now))
                              .ToListAsync();

        var openAccess = await _openAccessRepository.AsQueryable()
                             .Where(u => u.AppId == appId && u.IsDeleted == false)
                             .ToListAsync();
        var openAccessActive = openAccess
                                   .Where(u => u.RevokedTime == null && u.IsEnabled == true
                                               && (u.ExpiresTime == null || u.ExpiresTime > now))
                                   .ToList();

        var policy = await ResolvePolicyAsync(appId);
        var payments = await _paymentRepository.AsQueryable()
                           .Where(u => u.IsDeleted == false)
                           .Where(u => (u.GameAppIdNormalized == 0 && u.PackageIdNormalized == 0 && u.AppId == 0)
                                       || (u.GameAppIdNormalized == appId && u.PackageIdNormalized == 0)
                                       || (u.AppId == appId && u.PackageIdNormalized == 0))
                           .ToListAsync();
        var emails = await LoadScopedRowsAsync(_emailRepository.AsQueryable(), appId);
        var sms = await LoadScopedRowsAsync(_smsRepository.AsQueryable(), appId);
        var wechats = await LoadScopedRowsAsync(_wechatRepository.AsQueryable(), appId);
        var advertisements = await LoadScopedRowsAsync(_advertisementCallbackRepository.AsQueryable(), appId);
        var objectStorages = await _objectStorageRepository.AsQueryable()
                                 .Where(u => u.IsDeleted == false)
                                 .ToListAsync();

        return new TenantAppCredentialOverviewResponse
        {
            AppId = appId,
            AppName = app?.Name ?? string.Empty,
            Identity = new TenantAppCredentialOverviewResponse.IdentityCardResponse
            {
                AppSecretConfigured = app != null && !string.IsNullOrEmpty(app.AppSecretHash),
                AppSecretLastRotatedTime = app?.LastRotatedTime,
                SignatureRequired = app?.SignatureRequired ?? false,
            },
            Transport = new TenantAppCredentialOverviewResponse.TransportCardResponse
            {
                LatestHmacKid = signatureKeys
                    .OrderByDescending(u => u.NotBeforeTime ?? 0)
                    .ThenByDescending(u => u.Id)
                    .FirstOrDefault()?.Kid,
                HmacActiveKeyCount = signatureKeys.Count,
                HmacNearestExpireTime = MinNullable(signatureKeys.Select(u => u.ExpireTime)),
                OpenAccessActiveCount = openAccessActive.Count,
                OpenAccessLastRotatedTime = openAccess.Count == 0
                    ? null
                    : openAccess.Max(u => u.UpdateTime ?? u.CreatedTime),
            },
            TicketToken = new TenantAppCredentialOverviewResponse.TicketTokenCardResponse
            {
                TicketKeyActiveCount = ticketKeys.Count,
                TicketKeyNearestExpireTime = MinNullable(ticketKeys.Select(u => u.ExpireTime)),
                TicketKeyAutoCreateEnabled = policy?.EnableKeyAutoCreate ?? false,
                TokenLifetimeMinutes = policy?.LifetimeMinutes ?? 0,
                TokenSigningKeyConfigured = policy != null && !string.IsNullOrWhiteSpace(policy.SigningKey),
                PolicyScope = policy == null
                    ? TenantAppCredentialOverviewResponse.ScopeNone
                    : (policy.AppId == appId
                        ? TenantAppCredentialOverviewResponse.ScopeApp
                        : TenantAppCredentialOverviewResponse.ScopeTenantDefault),
                SessionKeyActiveCount = sessionKeys.Count,
            },
            Outbound = new List<TenantAppCredentialOverviewResponse.OutboundConfigResponse>
            {
                BuildPaymentItem(payments, appId),
                BuildScopedItem("email", "邮件", "/app/appCenter/tenantEmailConfigEntity",
                    emails, appId, u => u.Priority),
                BuildScopedItem("sms", "SMS", "/tenant/system/tenantSmsConfigEntity",
                    sms, appId, u => u.Priority),
                BuildScopedItem("wechat", "微信", "/tenant/system/tenantWechatConfigEntity",
                    wechats, appId, u => u.Priority),
                BuildScopedItem("adCallback", "广告回调", "/app/appCenter/tenantAdvertisementCallbackConfigEntity",
                    advertisements, appId, prioritySelector: null),
                BuildTenantItem(objectStorages),
            },
            UsageMatrix = TenantAppCredentialOverviewResponse.BuildUsageMatrix(),
        };
    }

    /// <summary>
    /// 解析 Client API 策略（C378 合并表）：应用行优先，缺省回退租户默认行（AppId=0）。
    /// </summary>
    private async Task<TenantClientApiPolicyEntity?> ResolvePolicyAsync(long appId)
    {
        var appRow = await _policyRepository.GetFirstAsync(u => u.AppId == appId && u.IsDeleted == false);
        if (appRow != null)
        {
            return appRow;
        }

        return await _policyRepository.GetFirstAsync(u => u.AppId == 0 && u.IsDeleted == false);
    }

    /// <summary>
    /// 载入作用域配置行（应用行 + 租户默认行一起取，命中判定交给 <see cref="BuildScopedItem"/>：应用行优先）。
    /// </summary>
    private static async Task<List<TScoped>> LoadScopedRowsAsync<TScoped>(ISugarQueryable<TScoped> queryable, long appId)
        where TScoped : Entity.Base.EntityBasic, Entity.Base.IAppScoped, new()
    {
        return await queryable
            .Where(u => u.AppId == appId || u.AppId == 0)
            .Where(u => u.IsDeleted == false)
            .ToListAsync();
    }

    /// <summary>
    /// 构建支付条目：作用域解析与 <c>PaymentOptionSettingProvider</c> 同口径（应用级命中优先，仅全局行命中为租户默认）。
    /// </summary>
    private static TenantAppCredentialOverviewResponse.OutboundConfigResponse BuildPaymentItem(List<TenantGamePaymentConfigEntity> rows, long appId)
    {
        var appScoped = rows.Any(u => u.GameAppIdNormalized == appId);
        return BuildItem("payment", "支付", "/app/appCenter/gamePaymentConfigEntity",
            rows.Count == 0
                ? TenantAppCredentialOverviewResponse.ScopeNone
                : (appScoped ? TenantAppCredentialOverviewResponse.ScopeApp : TenantAppCredentialOverviewResponse.ScopeTenantDefault),
            rows.Count, rows.Any(u => u.IsEnabled == true), priority: null);
    }

    /// <summary>
    /// 构建作用域配置条目（邮件/SMS/微信/广告回调）：应用行（AppId=appId）优先，缺省回退租户默认行（AppId=0）。
    /// </summary>
    private static TenantAppCredentialOverviewResponse.OutboundConfigResponse BuildScopedItem<TScoped>(string kind, string title, string routePath,
        List<TScoped> rows, long appId, Func<TScoped, int>? prioritySelector)
        where TScoped : Entity.Base.EntityBasic, Entity.Base.IAppScoped, new()
    {
        var matched = rows.Any(u => u.AppId == appId) ? rows.Where(u => u.AppId == appId).ToList() : rows;
        int? priority = prioritySelector != null && matched.Count > 0 ? matched.Min(prioritySelector) : null;
        return BuildItem(kind, title, routePath,
            matched.Count == 0
                ? TenantAppCredentialOverviewResponse.ScopeNone
                : (matched[0].AppId == appId ? TenantAppCredentialOverviewResponse.ScopeApp : TenantAppCredentialOverviewResponse.ScopeTenantDefault),
            matched.Count, matched.Any(u => u.IsEnabled == true), priority);
    }

    /// <summary>
    /// 构建对象存储条目（租户级配置，无应用维度字段）。
    /// </summary>
    private static TenantAppCredentialOverviewResponse.OutboundConfigResponse BuildTenantItem(List<TenantObjectStorageConfigEntity> rows)
    {
        return BuildItem("objectStorage", "对象存储", "/tenant/system/tenantObjectStorageConfigEntity",
            rows.Count == 0 ? TenantAppCredentialOverviewResponse.ScopeNone : TenantAppCredentialOverviewResponse.ScopeTenant,
            rows.Count, rows.Any(u => u.IsEnabled == true), priority: null);
    }

    /// <summary>
    /// 出站条目纯工厂（只装元数据，不触任何凭证本体）。
    /// </summary>
    private static TenantAppCredentialOverviewResponse.OutboundConfigResponse BuildItem(string kind, string title, string routePath,
        string scope, int configuredCount, bool enabled, int? priority)
    {
        return new TenantAppCredentialOverviewResponse.OutboundConfigResponse
        {
            Kind = kind,
            Title = title,
            Scope = scope,
            ConfiguredCount = configuredCount,
            Enabled = enabled,
            Priority = priority,
            RoutePath = routePath,
        };
    }

    /// <summary>
    /// 取最小非空到期时间（全部为空返回 null；客户端枚举，无表达式树 Nullable 翻译问题）。
    /// </summary>
    private static long? MinNullable(IEnumerable<long?> values)
    {
        var filtered = values.Where(u => u.HasValue).ToList();
        return filtered.Count == 0 ? null : filtered.Min(u => u!.Value);
    }
}
