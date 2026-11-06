using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Entity.Business.Game.App;
using System.Text.Json;
using GameFrameX.Application.ClientDelivery.GamePaymentConfig;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Service.Business;
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

    /// <summary>
    /// 支付宝公开回调端点路由段（{packageId} 可选路由模板，不含分包段）。
    /// </summary>
    private const string AliPayNotifyCallbackRoute = "api/GameOrder/NotifyAliPay";

    /// <summary>
    /// 微信支付公开回调端点路由段（{packageId} 可选路由模板，不含分包段）。
    /// </summary>
    private const string WechatPayNotifyCallbackRoute = "api/GameOrder/NotifyWeChatPay";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly ISqlSugarClient _dbProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 支付配置提供器实现
    /// </summary>
    public PaymentOptionSettingProvider(ISqlSugarClient dbProvider, IHttpContextAccessor httpContextAccessor)
    {
        _dbProvider = dbProvider;
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
        var packageId = ResolveCurrentPackageId(context, _dbProvider);
        return GetPaymentConfigs(normalizedChannel, packageId)
               .Where(config => !string.IsNullOrWhiteSpace(config.ConfigJson))
               // C372（GFX-838）：支付配置读取边界字段级解密（C371 起库内敏感字段密文；
               // DecryptConfigJson 自带密文→明文进程缓存；未注册渠道 schema 未知的字段原样透传）
               .Select(config => DeserializeOptionSetting(normalizedChannel, PaymentConfigCrypto.DecryptConfigJson(normalizedChannel, config.ConfigJson)))
               .OfType<IPaymentOptionSetting>()
               // C401（GFX-1213）：回调通知地址按当前 {packageId} 回调路由运行期补全/规范化，
               // 保证提交给网关的地址能命中公开回调端点（详见 NormalizeCallbackNotifyUrl）
               .Select(setting => NormalizeCallbackNotifyUrl(normalizedChannel, setting, packageId))
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

    /// <summary>
    /// 查询指定渠道的候选支付配置（packageId 由调用方一次解析后传入，供查询与回调地址规范化共用）。
    /// </summary>
    /// <param name="channelName">渠道名称（已小写规范化）。</param>
    /// <param name="packageId">当前上下文分包标识。</param>
    /// <returns>候选支付配置列表。</returns>
    private List<GamePaymentConfigEntity> GetPaymentConfigs(string channelName, long? packageId)
    {
        var context = _httpContextAccessor.HttpContext;
        var db = _dbProvider;
        var environments = GetRequestedEnvironments(context);
        var hasPackageId = packageId.HasValue;
        var packageIdValue = packageId.GetValueOrDefault();
        var query = db.Queryable<GamePaymentConfigEntity>()
                      .Where(u => u.PayChannel == channelName && u.IsEnabled == true &&
                                  (u.PackageIdNormalized == 0 ||
                                   (hasPackageId && u.PackageIdNormalized == packageIdValue)));
        if (environments.Length == 1)
        {
            var isSandbox = environments[0];
            query = query.Where(u => u.IsSandbox == isSandbox);
        }

        return query.ToList()
                    .GroupBy(u => u.IsSandbox)
                    .Select(group => GamePaymentConfigService.PickBestConfig(group, packageId))
                    .OfType<GamePaymentConfigEntity>()
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

    /// <summary>
    /// 按公开回调路由规范化支付渠道的回调通知地址（C401，GFX-1213）：站点根地址自动补全路由段
    /// （含当前分包段），旧「回调/{tenantId}/{appId}/{packageId}」三段地址自动去段重建；
    /// 无法识别的地址置空并由支付处理器按「通知地址未配置」拒绝下单，避免网关回调打在无法命中的地址上
    /// 导致已付款订单不发货。其余渠道地址原样透传。
    /// </summary>
    /// <param name="channelName">渠道名称（已小写规范化）。</param>
    /// <param name="setting">反序列化后的支付配置。</param>
    /// <param name="packageId">当前上下文分包标识。</param>
    /// <returns>回调地址规范化后的支付配置。</returns>
    private static IPaymentOptionSetting NormalizeCallbackNotifyUrl(string channelName, IPaymentOptionSetting setting, long? packageId)
    {
        if (setting is AliPayOptionSetting aliPayOptionSetting)
        {
            aliPayOptionSetting.AliPayNotifyUrl = BuildCallbackNotifyUrl(channelName, aliPayOptionSetting.AliPayNotifyUrl, AliPayNotifyCallbackRoute, packageId) ?? string.Empty;
            return aliPayOptionSetting;
        }

        if (setting is WechatPayOptionSetting wechatPayOptionSetting)
        {
            wechatPayOptionSetting.WechatPayNotifyUrl = BuildCallbackNotifyUrl(channelName, wechatPayOptionSetting.WechatPayNotifyUrl, WechatPayNotifyCallbackRoute, packageId) ?? string.Empty;
            return wechatPayOptionSetting;
        }

        return setting;
    }

    /// <summary>
    /// 单条配置的回调地址规范化：空值保持原样（由处理器既有「未配置」校验拒绝），
    /// 可识别地址交 <see cref="PublicCallbackNotifyUrlBuilder"/> 重建，无法识别返回 null。
    /// </summary>
    /// <param name="channelName">渠道名称。</param>
    /// <param name="configuredNotifyUrl">配置的回调通知地址。</param>
    /// <param name="callbackRoute">回调端点路由段。</param>
    /// <param name="packageId">当前上下文分包标识。</param>
    /// <returns>规范化后的回调地址；无法识别时 null。</returns>
    private static string? BuildCallbackNotifyUrl(string channelName, string? configuredNotifyUrl, string callbackRoute, long? packageId)
    {
        if (string.IsNullOrWhiteSpace(configuredNotifyUrl))
        {
            return configuredNotifyUrl;
        }

        var normalizedNotifyUrl = PublicCallbackNotifyUrlBuilder.Build(configuredNotifyUrl, callbackRoute, packageId);
        if (normalizedNotifyUrl == null)
        {
            LogHelper.Error($"支付回调通知地址无法映射到回调路由，已拒绝：channel={channelName} notifyUrl={configuredNotifyUrl}");
        }

        return normalizedNotifyUrl;
    }

    private static long? ResolveCurrentPackageId(HttpContext? context, ISqlSugarClient db)
    {
        if (context == null)
        {
            return null;
        }

        var packageId = GetCurrentPackageId(context);
        if (packageId.HasValue)
        {
            var package = db.Queryable<GameAppPackageEntity>()
                            .First(u => u.Id == packageId.Value && u.IsEnabled == true);
            return PackageMatchesScope(package) ? packageId : null;
        }

        var packageName = GetCurrentPackageName(context);
        if (string.IsNullOrWhiteSpace(packageName))
        {
            return null;
        }

        var namedPackage = db.Queryable<GameAppPackageEntity>()
                             .First(u => u.PackageName == packageName.Trim() && u.IsEnabled == true);
        return PackageMatchesScope(namedPackage) ? namedPackage.Id : null;
    }

    public static bool PackageMatchesScope(GameAppPackageEntity? package)
    {
        return package != null && package.IsEnabled == true;
    }

    /// <summary>
    /// 读取当前请求上下文的分包标识（中间件注入 Items 优先，其次协议头与查询参数），供回调地址构造复用。
    /// </summary>
    /// <param name="context">当前 HTTP 上下文，可为 null。</param>
    /// <returns>有效的分包标识；未携带时为 null。</returns>
    public static long? GetCurrentPackageId(HttpContext? context)
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

    private static bool[] GetRequestedEnvironments(HttpContext? context)
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
