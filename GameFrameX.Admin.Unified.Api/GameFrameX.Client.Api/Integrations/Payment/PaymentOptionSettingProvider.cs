using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Entity.Tenant.Game.App;
using System.Text.Json;
using GameFrameX.Application.ClientDelivery.GamePaymentConfig;
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

    private readonly ISqlSugarClient _tenantDbProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// 支付配置提供器实现
    /// </summary>
    public PaymentOptionSettingProvider(ISqlSugarClient tenantDbProvider, IHttpContextAccessor httpContextAccessor)
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
        var db = _tenantDbProvider;
        var packageId = ResolveCurrentPackageId(context, db);
        var environments = GetRequestedEnvironments(context);
        var hasPackageId = packageId.HasValue;
        var packageIdValue = packageId.GetValueOrDefault();
        var query = db.Queryable<TenantGamePaymentConfigEntity>()
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

    private static long? ResolveCurrentPackageId(HttpContext? context, ISqlSugarClient db)
    {
        if (context == null)
        {
            return null;
        }

        var packageId = GetCurrentPackageId(context);
        if (packageId.HasValue)
        {
            var package = db.Queryable<TenantGameAppPackageEntity>()
                            .First(u => u.Id == packageId.Value && u.IsEnabled == true);
            return PackageMatchesScope(package) ? packageId : null;
        }

        var packageName = GetCurrentPackageName(context);
        if (string.IsNullOrWhiteSpace(packageName))
        {
            return null;
        }

        var namedPackage = db.Queryable<TenantGameAppPackageEntity>()
                             .First(u => u.PackageName == packageName.Trim() && u.IsEnabled == true);
        return PackageMatchesScope(namedPackage) ? namedPackage.Id : null;
    }

    public static bool PackageMatchesScope(TenantGameAppPackageEntity? package)
    {
        return package != null && package.IsEnabled == true;
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
