using GameFrameX.Entity.System;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.PlatformJwt;

/// <summary>
/// 平台 JWT 运行时配置提供器。
/// </summary>
public sealed class PlatformJwtRuntimeProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly SqlSugarRepository<SystemConfigEntity> _systemConfigRepository;

    public PlatformJwtRuntimeProvider(SqlSugarRepository<SystemConfigEntity> systemConfigRepository)
    {
        _systemConfigRepository = systemConfigRepository;
    }

    /// <summary>
    /// 首次启动确保主库存在平台 JWT 配置记录；缺失时自动生成随机签名密钥的缺省配置写入。已存在记录（含格式损坏）不自动覆盖，保留安全基线交由后续 <see cref="GetRequiredSettingsAsync"/> 校验处理。
    /// </summary>
    public async Task EnsureDefaultSettingsAsync()
    {
        var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformJwtConfigConstants.ConfigCode);
        if (config != null && !string.IsNullOrWhiteSpace(config.Value))
        {
            return;
        }

        var settings = BuildDefaultSettings();
        Validate(settings, true);

        var value = JsonSerializer.Serialize(settings, SerializerOptions);
        await _systemConfigRepository.InsertAsync(new SystemConfigEntity
        {
            Name = "平台JWT配置",
            Code = PlatformJwtConfigConstants.ConfigCode,
            Value = value,
            SysFlag = YesNoEnum.Y,
            GroupCode = "Security",
            OrderNo = 11,
            Remark = "平台私有JWT签名与验证配置（首次启动自动生成，请尽快修改签名密钥）",
        });

        Console.WriteLine("[PlatformJWT] 已生成默认平台JWT配置(随机签名密钥)，请尽快在【平台配置】中修改签名密钥");
    }

    /// <summary>
    /// 构造缺省平台 JWT 配置；每次调用生成新的随机签名密钥，确保各部署互不共享。
    /// </summary>
    public static PlatformJwtSettings BuildDefaultSettings()
    {
        return new PlatformJwtSettings
        {
            IssuerSigningKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            ValidIssuer = "GameFrameX.Admin",
            ValidAudience = "GameFrameX.Admin",
        };
    }

    /// <summary>
    /// 读取当前平台 JWT 配置；配置缺失或不合法时拒绝继续认证或签发。
    /// </summary>
    public async Task<PlatformJwtSettings> GetRequiredSettingsAsync()
    {
        var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformJwtConfigConstants.ConfigCode);
        if (config == null || string.IsNullOrWhiteSpace(config.Value))
        {
            throw BusinessError.Oh("平台JWT配置缺失，请先完成部署配置").StatusCode(500);
        }

        PlatformJwtSettings? settings;
        try
        {
            settings = JsonSerializer.Deserialize<PlatformJwtSettings>(config.Value, SerializerOptions);
        }
        catch (JsonException)
        {
            throw BusinessError.Oh("平台JWT配置格式无效").StatusCode(500);
        }

        if (settings == null)
        {
            throw BusinessError.Oh("平台JWT配置格式无效").StatusCode(500);
        }

        Validate(settings, true);
        return settings;
    }

    /// <summary>
    /// 构造当前请求使用的 TokenValidationParameters。多实例通过每次读取持久化配置观察版本和密钥变化。
    /// </summary>
    public async Task<TokenValidationParameters> CreateValidationParametersAsync()
    {
        var settings = await GetRequiredSettingsAsync();
        return CreateValidationParameters(settings);
    }

    /// <summary>
    /// 构造给定配置的 TokenValidationParameters。
    /// </summary>
    public static TokenValidationParameters CreateValidationParameters(PlatformJwtSettings settings)
    {
        Validate(settings, true);
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.IssuerSigningKey)),
            ValidateIssuer = true,
            ValidIssuer = settings.ValidIssuer,
            ValidateAudience = true,
            ValidAudience = settings.ValidAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(settings.ClockSkew),
            RequireExpirationTime = true,
        };
    }

    /// <summary>
    /// 校验平台 JWT 安全基线，禁止保存或使用会降低签名、签发方、签收方、生命周期验证强度的配置。
    /// </summary>
    public static void Validate(PlatformJwtSettings settings, bool requireSigningKey)
    {
        if (settings == null)
        {
            throw BusinessError.Oh("平台JWT配置不能为空").StatusCode(500);
        }

        if (!settings.ValidateIssuerSigningKey || !settings.ValidateIssuer || !settings.ValidateAudience || !settings.ValidateLifetime)
        {
            throw BusinessError.Oh("平台JWT配置必须启用签名、签发方、签收方和生命周期验证").StatusCode(500);
        }

        if (!string.Equals(settings.Algorithm, PlatformJwtConfigConstants.Algorithm, StringComparison.Ordinal))
        {
            throw BusinessError.Oh("平台JWT仅支持HS256算法").StatusCode(500);
        }

        if (requireSigningKey && string.IsNullOrWhiteSpace(settings.IssuerSigningKey))
        {
            throw BusinessError.Oh("平台JWT签名密钥不能为空").StatusCode(500);
        }

        if (!string.IsNullOrWhiteSpace(settings.IssuerSigningKey) && Encoding.UTF8.GetByteCount(settings.IssuerSigningKey) < 32)
        {
            throw BusinessError.Oh("平台JWT签名密钥长度不足").StatusCode(500);
        }

        if (string.IsNullOrWhiteSpace(settings.ValidIssuer))
        {
            throw BusinessError.Oh("平台JWT签发方不能为空").StatusCode(500);
        }

        if (string.IsNullOrWhiteSpace(settings.ValidAudience))
        {
            throw BusinessError.Oh("平台JWT签收方不能为空").StatusCode(500);
        }

        if (settings.ClockSkew < 0 || settings.ClockSkew > 300)
        {
            throw BusinessError.Oh("平台JWT过期时间容错值必须在0到300秒之间").StatusCode(500);
        }

        if (settings.TokenVersion <= 0)
        {
            throw BusinessError.Oh("平台JWT令牌版本无效").StatusCode(500);
        }
    }
}