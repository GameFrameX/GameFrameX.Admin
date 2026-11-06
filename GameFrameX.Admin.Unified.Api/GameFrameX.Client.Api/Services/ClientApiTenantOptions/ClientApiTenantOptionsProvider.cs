using GameFrameX.Core.Service.Tenant;
using SqlSugar;
using System.Text.Json;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services.Email;
using GameFrameX.Entity.Tenant.ClientApiConfig;

namespace GameFrameX.Client.Api.Services.ClientApiTenantOptions;

/// <summary>
/// 从唯一主库读取 Client API 请求期业务配置。
/// <para>所有策略节共享启用且 Id 最大的配置行，并在请求内缓存。</para>
/// </summary>
public sealed class ClientApiTenantOptionsProvider : IClientApiTenantOptionsProvider
{
    private readonly ISqlSugarClient _database;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private TenantClientApiPolicyEntity? _effectivePolicyRow;
    private bool _effectivePolicyRowFetched;

    public ClientApiTenantOptionsProvider(ISqlSugarClient database, IHttpContextAccessor httpContextAccessor)
    {
        _database = database;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
#pragma warning disable CS8603 // 接口 IClientApiTenantOptionsProvider 签名非空（跨项目共享），但语义允许返回 null（未配置）；改返回类型会级联到测试 fake
    public async Task<TenantEmailOptions> GetEmailOptionsAsync()
    {
        var db = _database;
        var entity = await TenantEmailConfigSelector.SelectAsync(db, 0);
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
        var db = _database;
        var provider = ResolvePublicCallbackProvider(_httpContextAccessor.HttpContext);
        var query = db.Queryable<TenantAdvertisementCallbackConfigEntity>()
                      .Where(c => c.IsEnabled == true);
        var candidates = await query.ToListAsync();
        if (!string.IsNullOrWhiteSpace(provider))
        {
            candidates = candidates
                         .Where(c => string.Equals(NormalizeAdvertisementProvider(c.Provider), provider, StringComparison.OrdinalIgnoreCase))
                         .ToList();
        }

        return candidates
               .OrderByDescending(c => c.Id)
               .FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task<TenantClientApiTokenPolicyOptions> GetTokenPolicyAsync()
    {
        var entity = await GetEffectivePolicyRowAsync();
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

    private async Task<TenantClientApiPolicyEntity?> GetEffectivePolicyRowAsync()
    {
        if (!_effectivePolicyRowFetched)
        {
            _effectivePolicyRow = await _database.Queryable<TenantClientApiPolicyEntity>()
                .Where(c => c.IsEnabled == true)
                .OrderByDescending(c => c.Id)
                .FirstAsync();
            _effectivePolicyRowFetched = true;
        }

        return _effectivePolicyRow;
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
            using (var document = JsonDocument.Parse(raw))
            {
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