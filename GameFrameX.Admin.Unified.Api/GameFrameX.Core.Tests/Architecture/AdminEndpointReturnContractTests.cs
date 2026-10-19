using System.Reflection;
using Microsoft.AspNetCore.Http;
using GameFrameX.Core.Api;
using GameFrameX.Core.Service.Base;
using GameFrameX.Foundation.Orm.Entity;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.Architecture;

/// <summary>
/// C225 + C228 防回潮契约：Admin host 的 API action 返回签名禁止裸 string / object / dynamic，
/// 且禁止任何数据库 Entity 出现在返回签名（含 ListAsync&lt;Entity&gt; / SqlSugarPagedList&lt;Entity&gt; 集合泛型与
/// HttpJsonResultData&lt;Entity&gt; 信封泛型）——业务载荷一律强类型 Output（纯投影 POCO），
/// 交 AdminResultFilter 自动包装或显式返回 HttpJsonResultData&lt;Output&gt;（Filter 透传，防双重信封）。
/// 豁免：SystemCacheService.GetValue（缓存值天然任意标量）；[NonUnify] / [NonAction] 方法；
/// apiJson/post 返回 JsonObject 属类型化输出，天然不违规。
/// </summary>
public sealed class AdminEndpointReturnContractTests
{
    /// <summary>
    /// 豁免方法（声明类型全名 + 方法名）：返回 object/string 属天然语义、无法强类型化的端点。
    /// </summary>
    private static readonly HashSet<string> ExcludedEndpoints = new(StringComparer.Ordinal)
    {
        "GameFrameX.Core.Service.Cache.SystemCacheService.GetValue",
    };

    /// <summary>
    /// Output 敏感字段豁免（类型全名 + 属性名）：Service 层保证值恒为脱敏占位（******）或一次性明文回传的
    /// 显示字段，以及名字命中 token 但语义为策略配置 / 会话令牌 / 布尔标志的合法字段。
    /// 原值类敏感字段（Password 哈希 / AccessSecret 原值）零豁免。
    /// </summary>
    private static readonly HashSet<string> SensitiveDisplayFieldAllowlist = new(StringComparer.Ordinal)
    {
        // GameApp 创建/重置一次性回传明文，列表与详情恒 ******（TenantGameAppEntityService）
        "GameFrameX.Application.Game.App.GameApp.Dto.GameAppResponse.AppSecret",
        // App 签名密钥环：列表/详情恒 ******（PageAsync 强制重置）；AddAsync/RotateAsync 响应一次性回传明文（C2 TenantAppSignatureKeyEntityService）
        "GameFrameX.Application.Game.Tenant.Security.TenantAppSignatureKey.Dto.TenantAppSignatureKeyResponse.Secret",
        "GameFrameX.Application.Game.Tenant.Security.TenantAppSignatureKey.Dto.TenantAppSignatureKeyRevealResponse.Secret",
        // App 会话密钥环：列表/详情恒 ******（PageAsync/DetailAsync 强制重置）；AddAsync/RotateAsync 响应一次性回传明文（C5 TenantSessionKeyEntityService）
        "GameFrameX.Application.Game.Tenant.Security.TenantSessionKey.Dto.TenantSessionKeyResponse.Secret",
        "GameFrameX.Application.Game.Tenant.Security.TenantSessionKey.Dto.TenantSessionKeyRevealResponse.Secret",
        // OpenAccess 凭证签发/轮换响应一次性回传明文，列表与详情不再输出（TenantGameAppOpenAccessService）
        "GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess.Dto.TenantGameAppOpenAccessSecretResponse.AccessSecret",
        // MaskSensitiveFields 输出 ******，表单回显依赖（SystemEmailProviderService）
        "GameFrameX.Application.Game.System.Email.Dto.SystemEmailProviderResponse.Password",
        "GameFrameX.Application.Game.System.Email.Dto.SystemEmailProviderResponse.SecretId",
        "GameFrameX.Application.Game.System.Email.Dto.SystemEmailProviderResponse.SecretKey",
        // TenantClientApiConfigMaskHelper / 动态脱敏输出 ******（各 Config Service）
        "GameFrameX.Application.Game.Tenant.Integration.TenantAdvertisementCallbackConfig.Dto.TenantAdvertisementCallbackConfigResponse.SigningSecret",
        "GameFrameX.Application.Game.Tenant.Integration.TenantAdvertisementCallbackConfig.Dto.TenantAdvertisementCallbackConfigResponse.VerificationKeys",
        "GameFrameX.Application.Game.Tenant.ClientApiPolicy.TenantClientApiPolicy.Dto.TenantClientApiPolicyResponse.SigningKey",
        "GameFrameX.Application.Game.Tenant.Integration.TenantEmailConfig.Dto.TenantEmailConfigResponse.FromEmailSecretId",
        "GameFrameX.Application.Game.Tenant.Integration.TenantEmailConfig.Dto.TenantEmailConfigResponse.FromEmailSecretKey",
        "GameFrameX.Application.Game.Tenant.Integration.TenantSmsConfig.Dto.TenantSmsConfigResponse.AccessKeySecret",
        "GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto.TenantWechatConfigResponse.WechatAppSecret",
        "GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto.TenantWechatConfigResponse.WxOpenSecret",
        // 平台 JWT 配置：IssuerSigningKey 恒为掩码常量（C244），Has*/Validate* 为布尔标志/配置（PlatformJwtConfigService）
        "GameFrameX.Core.Service.PlatformJwt.Dto.PlatformJwtConfigResponse.IssuerSigningKey",
        "GameFrameX.Core.Service.PlatformJwt.Dto.PlatformJwtConfigResponse.HasSigningKey",
        "GameFrameX.Core.Service.PlatformJwt.Dto.PlatformJwtConfigResponse.ValidateIssuerSigningKey",
        // 认证会话令牌：登录/刷新端点的业务载荷即令牌本身（AuthService）
        "GameFrameX.Core.Service.Auth.Dto.LoginResponse.AccessToken",
        "GameFrameX.Core.Service.Auth.Dto.LoginResponse.RefreshToken",
        "GameFrameX.Core.Service.Auth.Dto.RefreshTokenResponse.RefreshToken",
        // 布尔初始化状态位：控制登录页首装向导显隐，非密码原值/哈希（SystemAuthService.GetLoginConfig）
        "GameFrameX.Core.Service.Auth.Dto.LoginConfigResponse.SuperAdminPasswordConfigured",
        // 一次性明文回传（response-envelope 规则 5）：重置密码初始值、OpenAccess 凭证签发
        "GameFrameX.Core.Service.User.Dto.ResetPwdResponse.NewPassword",
        "GameFrameX.Application.System.OpenAccess.Dto.SecretCreateResponse.Secret",
        // Client host（C227 批次 2 之前暂豁免）：游戏用户令牌 / Google OAuth 交换令牌为端点业务载荷本身
        "GameFrameX.Client.Api.GameUser.Dto.GetTenantGameUserEntityResponse.AccessToken",
        "GameFrameX.Client.Api.GameUser.Dto.GoogleAuthExchangeResponse.AccessToken",
        // C237 后缀统一后 Client host Response 进入本扫描集：登录类响应一次性回显的会话密钥（AES-256-GCM 信封对称密钥，仅登录非空，其余响应恒 null）
        "GameFrameX.Client.Api.GameUser.Dto.GetTenantGameUserEntityResponse.SessionKeySecret",
        // C290 后缀统一（Result→Response）进入本扫描集：登录期 per-session 会话密钥签发响应，Secret 仅签发一次性回显（ClientApiSessionKeyIssuer）
        "GameFrameX.Client.Api.Services.Auth.ClientApiSessionKeyIssueResponse.Secret",
        // 密码策略配置：正则/文案/开关名命中 password，值为策略配置而非密码（PlatformPasswordPolicyService）
        "GameFrameX.Core.Service.PlatformPasswordPolicy.Dto.PlatformPasswordPolicyConfigResponse.PasswordStrengthValidation",
        "GameFrameX.Core.Service.PlatformPasswordPolicy.Dto.PlatformPasswordPolicyConfigResponse.PasswordStrengthValidationMsg",
        "GameFrameX.Core.Service.PlatformPasswordPolicy.Dto.PlatformPasswordPolicyConfigResponse.StrongPassword",
    };

    /// <summary>
    /// 敏感字段黑名单 token（属性名 OrdinalIgnoreCase 命中即违规）。
    /// <para><c>secret</c> 泛覆盖 AppSecret / SecretId / SecretKey / SigningSecret / AccessKeySecret 等全部形态；
    /// 命中后按 <see cref="SensitiveDisplayFieldAllowlist"/> 豁免脱敏显示与一次性明文字段。</para>
    /// </summary>
    private static readonly string[] SensitiveNameTokens =
    {
        "password",
        "secret",
        "accesstoken",
        "refreshtoken",
        "signingkey",
        "verificationkey",
    };

    [Fact]
    public void AdminApiActions_DoNotReturnRawStringOrObject()
    {
        var violations = LoadAdminAssemblies()
                         .SelectMany(SafeGetTypes)
                         .Where(IsAdminController)
                         .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                         .Where(method => method.DeclaringType != typeof(object))
                         .Where(method => !IsExcluded(method))
                         .Select(method => new { Method = method, ReturnType = UnwrapTask(method.ReturnType), })
                         .Where(item => item.ReturnType == typeof(string) || item.ReturnType == typeof(object))
                         .Select(item => $"{item.Method.DeclaringType!.FullName}.{item.Method.Name}")
                         .Order(StringComparer.Ordinal)
                         .Distinct(StringComparer.Ordinal)
                         .ToList();

        Assert.True(
            violations.Count == 0,
            "Admin API action 不得返回裸 string/object/dynamic（U9 统一信封决策：业务载荷用强类型 DTO 交 AdminResultFilter 包装，"
            + "或显式返回 HttpJsonResultData<Output>）。违规方法：" + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// C228 契约：Admin API action 返回签名（含集合 / 分页 / 信封泛型参数）禁止出现数据库 Entity。
    /// </summary>
    [Fact]
    public void AdminApiActions_DoNotReturnDatabaseEntities()
    {
        var violations = LoadAdminAssemblies()
                         .SelectMany(SafeGetTypes)
                         .Where(IsAdminController)
                         .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                         .Where(method => method.DeclaringType != typeof(object))
                         .Where(method => !IsExcluded(method))
                         .Select(method => new { Method = method, EntityPayloads = CollectEntityPayloads(method.ReturnType), })
                         .Where(item => item.EntityPayloads.Count > 0)
                         .Select(item => $"{item.Method.DeclaringType!.FullName}.{item.Method.Name} -> {string.Join(", ", item.EntityPayloads.Select(t => t.Name))}")
                         .Order(StringComparer.Ordinal)
                         .Distinct(StringComparer.Ordinal)
                         .ToList();

        Assert.True(
            violations.Count == 0,
            "Admin API action 不得返回数据库 Entity（C228：CRUD 全链路 Entity→Output 强类型化，"
            + "含 List<Entity> / SqlSugarPagedList<Entity> / HttpJsonResultData<Entity> 等泛型参数）。违规方法："
            + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// C228 契约：全部 Output 类型禁止出现原值类敏感字段（Password / 哈希 / Secret 原值 / Token）。
    /// 脱敏显示字段（值恒 ******）与一次性明文字段须登记 <see cref="SensitiveDisplayFieldAllowlist"/>。
    /// </summary>
    [Fact]
    public void OutputTypes_DoNotContainSensitiveFields()
    {
        var violations = LoadAdminAssemblies()
                         .SelectMany(SafeGetTypes)
                         .Where(type => type.IsClass && type.IsPublic && !type.IsAbstract && type.Name.EndsWith("Response", StringComparison.Ordinal))
                         .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                                 .Select(property => new { Type = type, Property = property, }))
                         .Where(item => IsSensitivePropertyName(item.Property.Name)
                                        && !SensitiveDisplayFieldAllowlist.Contains($"{item.Type.FullName}.{item.Property.Name}"))
                         .Select(item => $"{item.Type.FullName}.{item.Property.Name}")
                         .Order(StringComparer.Ordinal)
                         .ToList();

        Assert.True(
            violations.Count == 0,
            "Output 类型禁止出现原值类敏感字段（C228：构造性安全——敏感字段不在 Output 属性中即无处可泄）。"
            + "脱敏显示字段（Service 保证值恒 ****** 或一次性回传）请登记 SensitiveDisplayFieldAllowlist 并注明理由。违规："
            + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// C228 契约：GenericCrudService 子类的 TResponse 必含 Id 与 RowVersion（乐观锁编辑链路与默认排序依赖，
    /// 缺失时 Mapster 投影静默丢字段、编辑乐观锁静默失效）。
    /// </summary>
    [Fact]
    public void CrudServiceOutputs_ContainIdAndRowVersion()
    {
        var crudBaseDefinition = typeof(GenericCrudService<,,,,>);
        var violations = LoadAdminAssemblies()
                         .SelectMany(SafeGetTypes)
                         .Where(type => type.IsClass && !type.IsAbstract)
                         .Select(type => new { Type = type, CrudBase = FindCrudBaseType(type, crudBaseDefinition), })
                         .Where(item => item.CrudBase != null)
                         .Select(item => new { item.Type, OutputType = item.CrudBase!.GetGenericArguments()[3], })
                         .Select(item => new
                         {
                             item.Type,
                             item.OutputType,
                             Missing = new[] { "Id", "RowVersion", }
                                       .Where(name => item.OutputType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance) == null)
                                       .ToList(),
                         })
                         .Where(item => item.Missing.Count > 0)
                         .Select(item => $"{item.Type.FullName} TResponse={item.OutputType.Name} 缺 {string.Join("/", item.Missing)}")
                         .Order(StringComparer.Ordinal)
                         .ToList();

        Assert.True(
            violations.Count == 0,
            "CRUD TResponse 必含 Id 与 RowVersion（C228：乐观锁编辑与默认排序依赖；继承拆纯 POCO 时遗漏将静默破坏编辑链路）。违规："
            + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    /// <summary>
    /// 从返回类型中收集数据库 Entity 载荷：解包 Task&lt;T&gt; / HttpJsonResultData&lt;T&gt;，
    /// 再检查自身与全部泛型参数（ListAsync&lt;T&gt; / SqlSugarPagedList&lt;T&gt; 等）。IActionResult / IResult 属文件与视图载荷，跳过。
    /// </summary>
    private static List<Type> CollectEntityPayloads(Type returnType)
    {
        var payloads = new List<Type>();
        var payload = UnwrapTask(returnType);
        if (typeof(IActionResult).IsAssignableFrom(payload) || typeof(IResult).IsAssignableFrom(payload))
        {
            return payloads;
        }

        if (IsDatabaseEntity(payload))
        {
            payloads.Add(payload);
        }

        if (payload.IsGenericType)
        {
            foreach (var argument in payload.GetGenericArguments())
            {
                if (IsDatabaseEntity(argument))
                {
                    payloads.Add(argument);
                }
            }
        }

        return payloads;
    }

    /// <summary>
    /// 数据库 Entity 判据：GameFrameX.Entity 命名空间下且继承 Foundation EntityBaseId。
    /// </summary>
    private static bool IsDatabaseEntity(Type type)
    {
        return type.IsClass
               && type.Name.EndsWith("Entity", StringComparison.Ordinal)
               && type.FullName != null
               && type.FullName.StartsWith("GameFrameX.Entity.", StringComparison.Ordinal)
               && typeof(EntityBaseId).IsAssignableFrom(type);
    }

    private static bool IsSensitivePropertyName(string propertyName)
    {
        foreach (var token in SensitiveNameTokens)
        {
            if (propertyName.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static Type FindCrudBaseType(Type type, Type genericDefinition)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == genericDefinition)
            {
                return current;
            }

            current = current.BaseType;
        }

        return null!;
    }

    /// <summary>
    /// 加载测试输出目录下全部 GameFrameX 程序集——与 AdminApiControllerFeatureProvider 的全程序集
    /// IAdminApiController 判据保持一致，不维护易漂移的名单。
    /// </summary>
    private static IEnumerable<Assembly> LoadAdminAssemblies()
    {
        return Directory.EnumerateFiles(AppContext.BaseDirectory, "GameFrameX*.dll")
                        .Select(path => Assembly.Load(AssemblyName.GetAssemblyName(path)))
                        .ToList();
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetExportedTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type != null).Cast<Type>();
        }
    }

    /// <summary>
    /// 与 AdminApiControllerFeatureProvider 同判据：实现 IAdminApiController 的具体 public 类即为 Admin API 控制器。
    /// </summary>
    private static bool IsAdminController(Type type)
    {
        return typeof(IAdminApiController).IsAssignableFrom(type)
               && type.IsClass
               && type.IsPublic
               && !type.IsAbstract
               && !type.ContainsGenericParameters;
    }

    private static bool IsExcluded(MethodInfo method)
    {
        if (method.GetCustomAttributes(true).OfType<NonActionAttribute>().Any())
        {
            return true;
        }

        if (method.GetCustomAttributes(true).OfType<NonUnifyAttribute>().Any())
        {
            return true;
        }

        var declaringType = method.DeclaringType;
        if (declaringType != null && declaringType.GetCustomAttributes(true).OfType<NonUnifyAttribute>().Any())
        {
            return true;
        }

        return ExcludedEndpoints.Contains($"{declaringType?.FullName}.{method.Name}");
    }

    private static Type UnwrapTask(Type returnType)
    {
        if (returnType == typeof(Task) || returnType == typeof(void))
        {
            return typeof(void);
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return returnType.GetGenericArguments()[0];
        }

        return returnType;
    }
}