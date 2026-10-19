using System.Text.Json;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services.Email;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Base;
using GameFrameX.Entity.Tenant.ClientApiConfig;

namespace GameFrameX.Client.Api.Services.ClientApiTenantOptions;

/// <summary>
/// 数据库驱动的 Client API 请求期租户配置 Provider。
/// <para>作用域选择策略：应用级配置（TenantId + AppId）优先于租户默认配置（TenantId, AppId=0）。</para>
/// <para>缺少租户上下文时抛出 <see cref="MissingTenantContextException"/>，禁止回落主库或进程级业务参数。</para>
/// </summary>
public sealed class ClientApiTenantOptionsProvider : IClientApiTenantOptionsProvider
{
    private readonly ClientApiTenantDbProvider _tenantDbProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 请求级缓存：租户默认行（AppId=0，令牌节唯一消费来源）。Scoped 生命周期内单次加载。
    /// </summary>
    private TenantClientApiPolicyEntity? _tenantDefaultPolicyRow;

    /// <summary>
    /// 请求级缓存：<see cref="_tenantDefaultPolicyRow"/> 是否已查过（区分「未查」与「查无行」）。
    /// </summary>
    private bool _tenantDefaultPolicyRowFetched;

    /// <summary>
    /// 请求级缓存：生效行（应用级行优先，AppId=0 兜底；票据节/输出节消费来源）。
    /// </summary>
    private TenantClientApiPolicyEntity? _effectivePolicyRow;

    /// <summary>
    /// 请求级缓存：<see cref="_effectivePolicyRow"/> 是否已查过。
    /// </summary>
    private bool _effectivePolicyRowFetched;

    /// <summary>
    /// </summary>
    public ClientApiTenantOptionsProvider(ClientApiTenantDbProvider tenantDbProvider, IHttpContextAccessor httpContextAccessor)
    {
        _tenantDbProvider = tenantDbProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public ClientApiTenantResolution ResolveContext()
    {
        var context = _httpContextAccessor.HttpContext;
        var tenantId = ResolveTenantId(context);
        if (!tenantId.HasValue)
        {
            throw new MissingTenantContextException("缺少租户上下文，Client API 请求期业务配置必须依赖当前 TenantId，禁止回落到主库或进程级业务参数。");
        }

        return new ClientApiTenantResolution
        {
            TenantId = tenantId.Value,
            AppId = ResolveAppId(context),
        };
    }

    /// <inheritdoc />
#pragma warning disable CS8603 // 接口 IClientApiTenantOptionsProvider 签名非空（跨项目共享），但语义允许返回 null（未配置）；改返回类型会级联到测试 fake
    public async Task<TenantEmailOptions> GetEmailOptionsAsync()
    {
        var ctx = ResolveContext();
        var db = _tenantDbProvider.GetTenantDb(ctx.TenantId);
        var entity = await TenantEmailConfigSelector.SelectAsync(db, ctx, 0);
        return entity == null
                   ? null
                   : new TenantEmailOptions
                   {
                       FromEmailAddress = entity.FromEmailAddress,
                       // C372（GFX-838）：凭证读取边界解密（C371 起库内密文；无前缀明文直通）
                       FromEmailSecretId = TenantSecretCrypto.DecryptCached(entity.FromEmailSecretId) ?? string.Empty,
                       FromEmailSecretKey = TenantSecretCrypto.DecryptCached(entity.FromEmailSecretKey) ?? string.Empty,
                   };
    }
#pragma warning restore CS8603

    /// <inheritdoc />
#pragma warning disable CS8603 // 接口 IClientApiTenantOptionsProvider 签名非空（跨项目共享），但语义允许返回 null（未配置）；改返回类型会级联到测试 fake
    public async Task<TenantAdvertisementCallbackOptions> GetAdvertisementCallbackOptionsAsync()
    {
        var entity = await PickAdvertisementCallbackConfigAsync();
        if (entity == null)
        {
            return null;
        }

        // C372（GFX-838）：广告回调读取边界解密——ConfigJson 字段级（自带密文→明文进程缓存）。
        // ConfigValues / ConfigJson 一律携带明文，消费方（奖励服务 / WebHook 验签）不感知密文形态。
        var decryptedConfigJson = AdvertisementCallbackConfigCrypto.DecryptConfigJson(entity.Provider, entity.ConfigJson);
        var configValues = ParseConfigValues(decryptedConfigJson);
        // ConfigJson 是验签材料唯一来源：AdMob 必填 verificationKeys，其余平台必填 signingSecret，缺失即 fail-closed。
        var requiredKey = NormalizeAdvertisementProvider(entity.Provider) == "admob" ? "verificationKeys" : "signingSecret";
        if (string.IsNullOrWhiteSpace(GetConfigValue(configValues, requiredKey)))
        {
            throw new InvalidOperationException($"广告回调配置缺少必填验签材料 {requiredKey}（Provider={entity.Provider}），ConfigJson 必须包含该字段");
        }

        var signingSecret = GetConfigValue(configValues, "signingSecret");
        var verificationKeys = GetConfigValue(configValues, "verificationKeys");
        return new TenantAdvertisementCallbackOptions
        {
            Provider = entity.Provider,
            CallbackUrl = GetConfigValue(configValues, "callbackUrl", entity.CallbackUrl),
            SigningSecret = signingSecret,
            VerificationKeys = verificationKeys,
            ParameterPurposeJson = entity.ParameterPurposeJson,
            ConfigJson = decryptedConfigJson,
            ConfigValues = configValues,
            PangleSecurityKey = signingSecret,
            AdMobKeys = ParseAdMobKeys(verificationKeys),
        };
    }
#pragma warning restore CS8603

    private async Task<TenantAdvertisementCallbackConfigEntity?> PickAdvertisementCallbackConfigAsync()
    {
        var ctx = ResolveContext();
        var db = _tenantDbProvider.GetTenantDb(ctx.TenantId);
        var appId = ctx.AppId.GetValueOrDefault();
        var provider = ResolvePublicCallbackProvider(_httpContextAccessor.HttpContext);
        var query = db.Queryable<TenantAdvertisementCallbackConfigEntity>()
                      .Where(c => c.TenantId == ctx.TenantId &&
                                  c.IsEnabled == true &&
                                  (c.AppId == appId || c.AppId == 0));
        var candidates = await query.ToListAsync();
        if (!string.IsNullOrWhiteSpace(provider))
        {
            candidates = candidates
                         .Where(c => string.Equals(NormalizeAdvertisementProvider(c.Provider), provider, StringComparison.OrdinalIgnoreCase))
                         .ToList();
        }

        return candidates
               .OrderByDescending(c => appId > 0 && c.AppId == appId)
               .ThenByDescending(c => c.Id)
               .FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task<TenantClientApiTokenPolicyOptions> GetTokenPolicyAsync()
    {
        var entity = await GetTenantDefaultPolicyRowAsync();
        return new TenantClientApiTokenPolicyOptions
        {
            // C372（GFX-838）：令牌签名密钥读取边界解密（C371 起库内密文；无前缀明文直通）
            SigningKey = string.IsNullOrWhiteSpace(entity?.SigningKey) ? null : TenantSecretCrypto.DecryptCached(entity.SigningKey),
            LifetimeMinutes = entity == null
                                  ? ClientApiPlatformDefaults.ClientApiTokenLifetimeMinutesDefault
                                  : ClientApiPlatformDefaults.ClampTokenLifetimeMinutes(entity.LifetimeMinutes),
        };
    }

    /// <inheritdoc />
    public async Task<TenantGameLoginTicketPolicyOptions> GetGameLoginTicketPolicyAsync()
    {
        var entity = await GetEffectivePolicyRowAsync();
        var now = TimerHelper.UnixTimeSeconds();
        return new TenantGameLoginTicketPolicyOptions
        {
            TicketExpireTime = ClientApiPlatformDefaults.NormalizeGameLoginTicketExpireTime(entity?.TicketExpireTime ?? 0, now),
            EnableKeyAutoCreate = entity?.EnableKeyAutoCreate ?? ClientApiPlatformDefaults.GameLoginTicketKeyAutoCreateDefault,
        };
    }

    /// <inheritdoc />
    public async Task<TenantOrderPolicyOptions> GetOrderPolicyAsync()
    {
        var entity = await GetEffectivePolicyRowAsync();
        return new TenantOrderPolicyOptions
        {
            EnableSandBoxRecharge = entity?.EnableSandBoxRecharge ?? false,
        };
    }

    /// <inheritdoc />
    public async Task<TenantAreaPolicyOptions> GetAreaPolicyAsync()
    {
        var entity = await GetEffectivePolicyRowAsync();
        return new TenantAreaPolicyOptions
        {
            EnableReturnHttpManager = entity?.EnableReturnHttpManager ?? false,
        };
    }

    /// <summary>
    /// 读取租户默认策略行（AppId=0，启用态，Id 降序取首）。令牌节唯一消费来源（C378：维持原租户级语义）。
    /// <para>请求级缓存：同一 Scoped 实例内四个策略 getter 只查一次租户默认行。</para>
    /// </summary>
    private async Task<TenantClientApiPolicyEntity?> GetTenantDefaultPolicyRowAsync()
    {
        if (_tenantDefaultPolicyRowFetched)
        {
            return _tenantDefaultPolicyRow;
        }

        var ctx = ResolveContext();
        var db = _tenantDbProvider.GetTenantDb(ctx.TenantId);
        _tenantDefaultPolicyRow = await db.Queryable<TenantClientApiPolicyEntity>()
                                          .Where(c => c.TenantId == ctx.TenantId && c.AppId == 0 && c.IsEnabled == true)
                                          .OrderByDescending(c => c.Id)
                                          .FirstAsync();
        _tenantDefaultPolicyRowFetched = true;
        return _tenantDefaultPolicyRow;
    }

    /// <summary>
    /// 读取生效策略行：应用级（AppId &gt; 0 且匹配）优先，否则租户默认（AppId == 0）；票据节/输出节消费来源。
    /// <para>请求级缓存：同一 Scoped 实例内只查一次生效行。</para>
    /// </summary>
    private async Task<TenantClientApiPolicyEntity?> GetEffectivePolicyRowAsync()
    {
        if (_effectivePolicyRowFetched)
        {
            return _effectivePolicyRow;
        }

        var ctx = ResolveContext();
        var db = _tenantDbProvider.GetTenantDb(ctx.TenantId);
        var appId = ctx.AppId.GetValueOrDefault();
        if (appId > 0)
        {
            _effectivePolicyRow = await db.Queryable<TenantClientApiPolicyEntity>()
                                          .Where(c => c.TenantId == ctx.TenantId && c.IsEnabled == true && c.AppId == appId)
                                          .OrderByDescending(c => c.Id)
                                          .FirstAsync();
        }

        if (_effectivePolicyRow == null)
        {
            _effectivePolicyRow = await db.Queryable<TenantClientApiPolicyEntity>()
                                          .Where(c => c.TenantId == ctx.TenantId && c.IsEnabled == true && c.AppId == 0)
                                          .OrderByDescending(c => c.Id)
                                          .FirstAsync();
        }

        _effectivePolicyRowFetched = true;
        return _effectivePolicyRow;
    }

    /// <summary>
    /// 解析当前请求的 TenantId；仅承认 <see cref="TenantContextAccessor.TenantIdKey"/>，不接受 query/header 绕过。
    /// </summary>
    private static long? ResolveTenantId(HttpContext? context)
    {
        if (context == null)
        {
            return null;
        }

        if (context.Items.TryGetValue(TenantContextAccessor.TenantIdKey, out var value) &&
            long.TryParse(value?.ToString(), out var tenantId))
        {
            return tenantId;
        }

        return null;
    }

    /// <summary>
    /// 解析当前请求的 AppId；仅承认 <see cref="AppResolutionMiddleware.AppIdKey"/>，无法取得时返回 null。
    /// </summary>
    private static long? ResolveAppId(HttpContext? context)
    {
        if (context == null)
        {
            return null;
        }

        if (context.Items.TryGetValue(AppResolutionMiddleware.AppIdKey, out var value) &&
            long.TryParse(value?.ToString(), out var appId))
        {
            return appId;
        }

        return null;
    }

    private static string ResolvePublicCallbackProvider(HttpContext? context)
    {
        return context != null &&
               context.Items.TryGetValue(PublicCallbackRouteContext.ItemKey, out var routeContextObj) &&
               routeContextObj is PublicCallbackRouteContext routeContext
                   ? NormalizeAdvertisementProvider(routeContext.Provider)
                   : string.Empty;
    }

    internal static string NormalizeAdvertisementProvider(string provider)
    {
        return provider?.Trim().ToLowerInvariant() switch
        {
            "topon" or "top_on"                                    => "topon",
            "googleadmob" or "google_admob" or "admob" or "ad_mob" => "admob",
            "pangle" or "pangleglobal" or "pangle_global"          => "pangle",
            "unityads" or "unity_ads"                              => "unityads",
            _                                                      => string.Empty,
        };
    }

    /// <summary>
    /// 将 AdMob 配置（key_id=base64key, 多个逗号分隔）解析为字典，忽略空项与格式错误项。
    /// </summary>
    internal static IReadOnlyDictionary<string, string> ParseAdMobKeys(string raw)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return result;
        }

        foreach (var pair in raw.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var val = parts[1].Trim();
                if (key.Length > 0 && val.Length > 0)
                {
                    result[key] = val;
                }
            }
        }

        return result;
    }

    private static IReadOnlyDictionary<string, string> ParseConfigValues(string? raw)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return result;
        }

        try
        {
            using var document = JsonDocument.Parse(raw);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return result;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                result[property.Name] = property.Value.ValueKind == JsonValueKind.String
                                            ? property.Value.GetString() ?? string.Empty
                                            : property.Value.ToString();
            }
        }
        catch (JsonException)
        {
            return result;
        }

        return result;
    }

    private static string GetConfigValue(IReadOnlyDictionary<string, string> values, string key, string? fallback = null)
    {
        return values != null && values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                   ? value
                   : fallback ?? string.Empty;
    }
}