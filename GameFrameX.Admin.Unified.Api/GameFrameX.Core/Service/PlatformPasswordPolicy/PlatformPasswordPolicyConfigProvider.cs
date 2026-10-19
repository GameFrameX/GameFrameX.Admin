using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.PlatformPasswordPolicy;

/// <summary>
/// 平台密码策略配置提供器（C173）。维护同一 Admin Host 内的内存快照：启动期由
/// PlatformPasswordPolicyStartupLoader（Web.Core）调 <see cref="RefreshAsync"/>
/// 从主库 <see cref="SystemConfigEntity"/> 加载；SuperAdmin 保存配置后由配置服务调用 <see cref="RefreshAsync"/>
/// 刷新快照并镜像同步 <see cref="CryptogramUtility"/> 密码策略静态属性，使后续 <c>TenantUserService.ChangePwdAsync</c>
/// 读到最新密码强度规则。密钥（CryptoType/PublicKey/PrivateKey）不在此处管理，保持启动期 Secret。
/// </summary>
public sealed class PlatformPasswordPolicyConfigProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly RegexOptions RegexCompileOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
    private readonly IServiceScopeFactory _scopeFactory;
    private PlatformPasswordPolicySettings _snapshot = CreateDefaultSettings();

    public PlatformPasswordPolicyConfigProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// 获取当前不可变快照副本。
    /// </summary>
    public PlatformPasswordPolicySettings GetSnapshot()
    {
        var settings = _snapshot;
        return CloneSettings(settings);
    }

    /// <summary>
    /// 启动或配置保存后刷新当前 Host 内存快照，并镜像同步 CryptogramUtility 密码策略静态属性；
    /// 缺失记录会写入与历史 Cryptogram 等价的安全默认值。
    /// </summary>
    public async Task RefreshAsync()
    {
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemConfigEntity>>();
            var config = await repository.GetFirstAsync(u => u.Code == PlatformPasswordPolicyConfigConstants.ConfigCode);
            if (config == null || string.IsNullOrWhiteSpace(config.Value))
            {
                var defaults = CreateDefaultSettings();
                await repository.InsertAsync(new SystemConfigEntity
                {
                    Name = PlatformPasswordPolicyConfigConstants.ConfigName,
                    Code = PlatformPasswordPolicyConfigConstants.ConfigCode,
                    Value = Serialize(defaults),
                    SysFlag = YesNoEnum.Y,
                    GroupCode = PlatformPasswordPolicyConfigConstants.GroupCode,
                    OrderNo = PlatformPasswordPolicyConfigConstants.OrderNo,
                    Remark = PlatformPasswordPolicyConfigConstants.ConfigRemark,
                });

                ApplyToCryptogramUtility(defaults);
                _snapshot = defaults;
                return;
            }

            var settings = Deserialize(config.Value);
            Validate(settings);
            ApplyToCryptogramUtility(settings);
            _snapshot = settings;
        }
    }

    /// <summary>
    /// 创建与历史 Cryptogram 值等价的默认密码策略（StrongPassword=false + 历史正则 + 历史提示）。
    /// </summary>
    public static PlatformPasswordPolicySettings CreateDefaultSettings()
    {
        return new PlatformPasswordPolicySettings();
    }

    /// <summary>
    /// 反序列化平台密码策略配置。格式非法时抛业务错误（500）。
    /// </summary>
    public static PlatformPasswordPolicySettings Deserialize(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<PlatformPasswordPolicySettings>(value, SerializerOptions) ?? throw new JsonException();
        }
        catch (JsonException)
        {
            throw BusinessError.Oh("平台密码策略配置格式无效").StatusCode(500);
        }
    }

    /// <summary>
    /// 序列化并校验长度，确保不超 <see cref="SystemConfigEntity"/> 的 Value 列长度。
    /// </summary>
    public static string Serialize(PlatformPasswordPolicySettings settings)
    {
        Validate(settings);
        var value = JsonSerializer.Serialize(settings, SerializerOptions);
        if (value.Length > PlatformPasswordPolicyConfigConstants.MaxSerializedLength)
        {
            throw BusinessError.Oh($"平台密码策略配置过长，不能超过 {PlatformPasswordPolicyConfigConstants.MaxSerializedLength} 个字符");
        }

        return value;
    }

    /// <summary>
    /// 校验平台密码策略配置：正则与提示非空，且正则必须可编译（防止 SuperAdmin 保存非法正则导致后续改密全部失败）。
    /// </summary>
    public static void Validate(PlatformPasswordPolicySettings settings)
    {
        if (settings == null)
        {
            throw BusinessError.Oh("平台密码策略配置不能为空").StatusCode(500);
        }

        if (string.IsNullOrWhiteSpace(settings.PasswordStrengthValidation))
        {
            throw BusinessError.Oh("密码强度正则不能为空").StatusCode(500);
        }

        if (string.IsNullOrWhiteSpace(settings.PasswordStrengthValidationMsg))
        {
            throw BusinessError.Oh("密码强度提示不能为空").StatusCode(500);
        }

        EnsureValidRegex(settings.PasswordStrengthValidation);
    }

    private static void EnsureValidRegex(string pattern)
    {
        try
        {
            _ = new Regex(pattern, RegexCompileOptions, TimeSpan.FromMilliseconds(500));
        }
        catch (ArgumentException)
        {
            throw BusinessError.Oh("密码强度正则格式无效").StatusCode(500);
        }
    }

    /// <summary>
    /// 将密码策略快照镜像同步到 CryptogramUtility 静态属性，使 TenantUserService.ChangePwdAsync 读到最新值（同 Host 动态生效）。
    /// </summary>
    private static void ApplyToCryptogramUtility(PlatformPasswordPolicySettings settings)
    {
        CryptogramUtility.ApplyPasswordPolicy(settings.StrongPassword, settings.PasswordStrengthValidation, settings.PasswordStrengthValidationMsg);
    }

    private static PlatformPasswordPolicySettings CloneSettings(PlatformPasswordPolicySettings source)
    {
        return new PlatformPasswordPolicySettings
        {
            StrongPassword = source.StrongPassword,
            PasswordStrengthValidation = source.PasswordStrengthValidation,
            PasswordStrengthValidationMsg = source.PasswordStrengthValidationMsg,
        };
    }
}