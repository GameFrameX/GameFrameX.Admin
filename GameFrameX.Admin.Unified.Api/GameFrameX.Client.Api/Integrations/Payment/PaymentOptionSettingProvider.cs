using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Entity.Tenant.Game.App;
using System.Text.Json;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.AliPay;
using GameFrameX.Payment.Wechat;
using SqlSugar;

namespace GameFrameX.Client.Api.Integrations.Payment;

/// <summary>
/// 支付配置提供器实现
/// </summary>
public class PaymentOptionSettingProvider : IPaymentOptionSettingProvider
{
    public const string PaymentIsSandboxKey = "PaymentIsSandbox";
    public const string PackageIdKey = "PackageId";
    public const string PackageIdHeader = "GameFrameX-Package-Id";
    public const string PackageNameHeader = "GameFrameX-Package-Name";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly ClientApiTenantDbProvider _tenantDbProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 支付配置提供器实现
    /// </summary>
    public PaymentOptionSettingProvider(ClientApiTenantDbProvider tenantDbProvider, IHttpContextAccessor httpContextAccessor)
    {
        _tenantDbProvider = tenantDbProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 获取指定支付渠道的配置
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置</returns>
    public IPaymentOptionSetting GetOptionSetting(string channelName)
    {
#pragma warning disable CS8603 // 接口 IPaymentOptionSettingProvider 签名非空（跨项目），未匹配时 FirstOrDefault 返回 null 是契约语义
        return GetOptionSettings(channelName).FirstOrDefault();
#pragma warning restore CS8603
    }

    /// <summary>
    /// 获取指定支付渠道的候选配置
    /// </summary>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置列表</returns>
    public IEnumerable<IPaymentOptionSetting> GetOptionSettings(string channelName)
    {
        var normalizedChannel = NormalizeChannel(channelName);
        if (string.IsNullOrWhiteSpace(normalizedChannel))
        {
            return Array.Empty<IPaymentOptionSetting>();
        }

        var context = _httpContextAccessor.HttpContext;
        return GetPaymentConfigs(normalizedChannel)
               .Where(config => !string.IsNullOrWhiteSpace(config.ConfigJson))
               // C372（GFX-838）：支付配置读取边界字段级解密（C371 起库内敏感字段密文；
               // DecryptConfigJson 自带密文→明文进程缓存；未注册渠道 schema 未知的字段原样透传）
               .Select(config => DeserializeOptionSetting(normalizedChannel, PaymentConfigCrypto.DecryptConfigJson(normalizedChannel, config.ConfigJson)))
               .Select(setting => ApplyPublicCallbackRoute(normalizedChannel, setting, context))
               .OfType<IPaymentOptionSetting>()
               .ToList();
    }

    /// <summary>
    /// 获取指定类型的支付配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置</returns>
    public T GetOptionSetting<T>(string channelName) where T : class, IPaymentOptionSetting
    {
#pragma warning disable CS8603 // 接口 IPaymentOptionSettingProvider 签名非空（跨项目），类型不匹配时 as 返回 null 是契约语义
        return GetOptionSetting(channelName) as T;
#pragma warning restore CS8603
    }

    /// <summary>
    /// 获取指定类型的候选支付配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="channelName">渠道名称</param>
    /// <returns>支付配置列表</returns>
    public IEnumerable<T> GetOptionSettings<T>(string channelName) where T : class, IPaymentOptionSetting
    {
        return GetOptionSettings(channelName).OfType<T>().ToList();
    }

    private List<TenantGamePaymentConfigEntity> GetPaymentConfigs(string channelName)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            LogHelper.Warning($"获取支付配置失败，缺少 HTTP 上下文 - channel:{channelName}");
            return new List<TenantGamePaymentConfigEntity>();
        }

        var tenantId = GetCurrentTenantId(context);
        if (!tenantId.HasValue)
        {
            LogHelper.Warning($"获取支付配置失败，缺少租户上下文 - channel:{channelName}");
            return new List<TenantGamePaymentConfigEntity>();
        }

        var appId = GetCurrentAppId(context);
        var db = _tenantDbProvider.GetTenantDb(tenantId.Value);
        var packageId = ResolveCurrentPackageId(context, tenantId.Value, appId, db);
        var environments = GetRequestedEnvironments(context);

        // 可空判断在表达式树外求值为局部布尔：SqlSugar 对闭包内 Nullable.HasValue 的 SQL 翻译
        // 生成非法片段（SQLite 报 near "IS"、PG 报 near "NOT"），提为常量语义等价且两方言均合法。
        var hasAppId = appId.HasValue;
        var hasPackageId = packageId.HasValue;
        var appIdValue = appId.GetValueOrDefault();
        var packageIdValue = packageId.GetValueOrDefault();
        var query = db.Queryable<TenantGamePaymentConfigEntity>()
                      .Where(u => u.TenantId == tenantId.Value &&
                                  u.PayChannel == channelName &&
                                  (
                                      (u.GameAppIdNormalized == 0 && u.PackageIdNormalized == 0 && u.AppId == 0) ||
                                      (hasAppId &&
                                       (
                                           (u.GameAppIdNormalized == appIdValue && u.PackageIdNormalized == 0) ||
                                           (hasPackageId && u.GameAppIdNormalized == appIdValue && u.PackageIdNormalized == packageIdValue)
                                       ))
                                  ));
        if (environments.Length == 1)
        {
            var isSandbox = environments[0];
            query = query.Where(u => u.IsSandbox == isSandbox);
        }

        return query.ToList()
                    .GroupBy(u => u.IsSandbox)
                    .Select(group => PickBestConfig(group, appId, packageId))
                    .OfType<TenantGamePaymentConfigEntity>()
                    .OrderBy(u => u.IsSandbox)
                    .ToList();
    }

    private static IPaymentOptionSetting? DeserializeOptionSetting(string channelName, string configJson)
    {
        var optionType = PaymentChannelRegistry.GetOptionType(channelName);
        if (optionType == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize(configJson, optionType, SerializerOptions) as IPaymentOptionSetting;
        }
        catch (JsonException ex)
        {
            LogHelper.Error($"支付配置JSON解析失败 - channel:{channelName}, error:{ex.Message}");
            return null;
        }
    }

    private static IPaymentOptionSetting? ApplyPublicCallbackRoute(string channelName, IPaymentOptionSetting? setting, HttpContext? context)
    {
        if (setting == null || context == null)
        {
            return setting;
        }

        var isAliPay = string.Equals(channelName, "alipay", StringComparison.OrdinalIgnoreCase);
        var isWechat = string.Equals(channelName, "wechat", StringComparison.OrdinalIgnoreCase);
        if (!isAliPay && !isWechat)
        {
            return setting;
        }

        var tenantId = GetCurrentTenantId(context);
        var appId = GetCurrentAppId(context);
        if (!tenantId.HasValue || !appId.HasValue)
        {
            return setting;
        }

        var routeContext = new PublicCallbackRouteContext(
            tenantId.Value,
            appId.Value,
            isAliPay ? "alipay" : "wechat",
            "payment",
            GetCurrentPackageId(context));

        if (setting is AliPayOptionSetting aliPayOptionSetting)
        {
            aliPayOptionSetting.AliPayNotifyUrl = PublicCallbackRouteResolver.AppendRouteParameters(aliPayOptionSetting.AliPayNotifyUrl, routeContext);
        }
        else if (setting is WechatPayOptionSetting wechatPayOptionSetting)
        {
            wechatPayOptionSetting.WechatPayNotifyUrl = PublicCallbackRouteResolver.AppendRouteParameters(wechatPayOptionSetting.WechatPayNotifyUrl, routeContext);
        }

        return setting;
    }

    internal static long? GetCurrentTenantId(HttpContext? context)
    {
        if (context == null)
        {
            return null;
        }

        if (context.Items.TryGetValue(TenantContextAccessor.TenantIdKey, out var tenantIdObj) &&
            tenantIdObj != null &&
            long.TryParse(tenantIdObj.ToString(), out var tenantIdFromContext))
        {
            return tenantIdFromContext;
        }

        return null;
    }

    /// <summary>
    /// 解析当前请求的 AppId；仅承认 <see cref="AppResolutionMiddleware.AppIdKey"/>，禁止回退到请求头或查询参数。
    /// <para>应用上下文由 <c>AppResolutionMiddleware</c>（凭证校验）或公开回调路由解析（<c>PublicCallbackTenantResolutionMiddleware</c>）写入；
    /// 支付配置的应用作用域选择必须只信任中间件解析后的 AppId，未命中时按「无应用上下文」语义返回 null，
    /// 与 <see cref="ClientApiTenantOptions.ClientApiTenantOptionsProvider"/> 的权威来源保持一致。</para>
    /// </summary>
    /// <param name="context">当前 HTTP 上下文</param>
    /// <returns>中间件解析出的 AppId；缺少应用上下文时返回 null</returns>
    internal static long? GetCurrentAppId(HttpContext? context)
    {
        if (context == null)
        {
            return null;
        }

        if (context.Items.TryGetValue(AppResolutionMiddleware.AppIdKey, out var appIdObj) &&
            appIdObj != null &&
            long.TryParse(appIdObj.ToString(), out var appIdFromContext))
        {
            return appIdFromContext;
        }

        return null;
    }

    private long? ResolveCurrentPackageId(HttpContext context, long tenantId, long? appId, ISqlSugarClient db)
    {
        if (!appId.HasValue)
        {
            return null;
        }

        var packageId = GetCurrentPackageId(context);
        if (packageId.HasValue)
        {
            var package = db.Queryable<TenantGameAppPackageEntity>()
                            .First(u => u.Id == packageId.Value &&
                                        u.TenantId == tenantId &&
                                        u.GameAppId == appId.Value &&
                                        u.IsEnabled != false);
            if (PackageMatchesScope(package, tenantId, appId.Value))
            {
                return packageId;
            }

            LogHelper.Warning($"忽略无效支付分包范围 - tenant:{tenantId}, app:{appId}, package:{packageId}");
            return null;
        }

        var packageName = GetCurrentPackageName(context);
        if (string.IsNullOrWhiteSpace(packageName))
        {
            return null;
        }

        var namedPackage = db.Queryable<TenantGameAppPackageEntity>()
                             .First(u => u.TenantId == tenantId &&
                                         u.GameAppId == appId.Value &&
                                         u.PackageName == packageName.Trim() &&
                                         u.IsEnabled != false);
        return PackageMatchesScope(namedPackage, tenantId, appId.Value) ? namedPackage.Id : null;
    }

    public static bool PackageMatchesScope(TenantGameAppPackageEntity package, long tenantId, long appId)
    {
        return package != null &&
               package.TenantId == tenantId &&
               package.GameAppId == appId &&
               package.IsEnabled != false;
    }

    private static long? GetCurrentPackageId(HttpContext context)
    {
        if (context == null)
        {
            return null;
        }

        if (context.Items.TryGetValue(PackageIdKey, out var packageIdObj) &&
            packageIdObj != null &&
            long.TryParse(packageIdObj.ToString(), out var packageIdFromContext))
        {
            return packageIdFromContext;
        }

        var packageIdText = context.Request.Headers[PackageIdHeader].FirstOrDefault();
        if (long.TryParse(packageIdText, out var packageIdFromHeader))
        {
            return packageIdFromHeader;
        }

        packageIdText = context.Request.Query["packageId"].FirstOrDefault();
        return long.TryParse(packageIdText, out var packageIdFromQuery) ? packageIdFromQuery : null;
    }

    private static string? GetCurrentPackageName(HttpContext context)
    {
        if (context == null)
        {
            return null;
        }

        var packageName = context.Request.Headers[PackageNameHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(packageName))
        {
            return packageName;
        }

        packageName = context.Request.Query["packageName"].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(packageName) ? packageName : null;
    }

    private static bool[] GetRequestedEnvironments(HttpContext context)
    {
        if (context == null)
        {
            return new[] { false, true, };
        }

        if (context.Items.TryGetValue(PaymentIsSandboxKey, out var sandboxObj) &&
            sandboxObj != null &&
            bool.TryParse(sandboxObj.ToString(), out var sandboxFromContext))
        {
            return new[] { sandboxFromContext, };
        }

        var envText = context.Request.Query["env"].FirstOrDefault() ??
                      context.Request.Headers["GameFrameX-Payment-Env"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(envText))
        {
            return NormalizeEnvironment(envText) switch
            {
                "sandbox" => new[] { true, },
                "prod"    => new[] { false, },
                _         => new[] { false, true, },
            };
        }

        var sandboxText = context.Request.Headers["GameFrameX-Is-Sandbox"].FirstOrDefault();
        if (bool.TryParse(sandboxText, out var sandboxFromHeader))
        {
            return new[] { sandboxFromHeader, };
        }

        sandboxText = context.Request.Query["isSandbox"].FirstOrDefault();
        return bool.TryParse(sandboxText, out var sandboxFromQuery)
                   ? new[] { sandboxFromQuery, }
                   : new[] { false, true, };
    }

    private static string? NormalizeChannel(string channelName)
    {
        return channelName?.Trim().ToLowerInvariant();
    }

    private static TenantGamePaymentConfigEntity? PickBestConfig(
        IEnumerable<TenantGamePaymentConfigEntity> configs,
        long? gameAppId,
        long? packageId)
    {
        var normalizedGameAppId = gameAppId.GetValueOrDefault() > 0 ? gameAppId.GetValueOrDefault() : 0;
        var normalizedPackageId = packageId.GetValueOrDefault() > 0 ? packageId.GetValueOrDefault() : 0;
        return configs
               .OrderByDescending(config => GetScopePriority(config, normalizedGameAppId, normalizedPackageId))
               .FirstOrDefault(config => GetScopePriority(config, normalizedGameAppId, normalizedPackageId) > 0);
    }

    private static int GetScopePriority(TenantGamePaymentConfigEntity config, long gameAppId, long packageId)
    {
        var configGameAppId = config.GameAppIdNormalized;
        var configPackageId = config.PackageIdNormalized;
        if (gameAppId > 0 && packageId > 0 && configGameAppId == gameAppId && configPackageId == packageId)
        {
            return 3;
        }

        if (gameAppId > 0 && configGameAppId == gameAppId && configPackageId == 0)
        {
            return 2;
        }

        return configGameAppId == 0 && configPackageId == 0 ? 1 : 0;
    }

    private static string NormalizeEnvironment(string env)
    {
        return env.Trim().ToLowerInvariant() switch
        {
            "production" => "prod",
            "prod"       => "prod",
            "false"      => "prod",
            "0"          => "prod",
            "sandbox"    => "sandbox",
            "test"       => "sandbox",
            "true"       => "sandbox",
            "1"          => "sandbox",
            _            => string.Empty,
        };
    }
}