using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.PlatformJwt.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.PlatformJwt;

/// <summary>
/// 平台 JWT 配置服务。
/// <para>宿主表说明（GFX-706）：本服务不持有独立宿主表，共享主库 SystemConfigEntity（system_config 表），以 Code 分区行 <c>platform_jwt_settings</c>（PlatformJwtConfigConstants.ConfigCode）存储本域配置 JSON。Platform* 前缀指平台级配置域，名实相符保留类名。</para>
/// </summary>
[ApiDescriptionSettings(Order = 441)]
public sealed class PlatformJwtConfigService : IAdminApiController
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly SqlSugarRepository<SystemConfigEntity> _systemConfigRepository;
    private readonly SystemCacheService _systemCacheService;

    public PlatformJwtConfigService(SqlSugarRepository<SystemConfigEntity> systemConfigRepository, SystemCacheService systemCacheService)
    {
        _systemConfigRepository = systemConfigRepository;
        _systemCacheService = systemCacheService;
    }

    /// <summary>
    /// 获取平台 JWT 配置详情。签名密钥只返回掩码，不返回明文。
    /// </summary>
    [ApiDescriptionSettings(Name = "Detail")]
    [DisplayName("获取平台JWT配置详情")]
    public async Task<PlatformJwtConfigResponse> DetailAsync()
    {
        var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformJwtConfigConstants.ConfigCode);
        if (config == null || string.IsNullOrWhiteSpace(config.Value))
        {
            return new PlatformJwtConfigResponse();
        }

        var settings = Deserialize(config.Value);
        PlatformJwtRuntimeProvider.Validate(settings, false);
        return new PlatformJwtConfigResponse
        {
            HasSigningKey = !string.IsNullOrWhiteSpace(settings.IssuerSigningKey),
            ValidIssuer = settings.ValidIssuer,
            ValidAudience = settings.ValidAudience,
            ClockSkew = settings.ClockSkew,
            Algorithm = settings.Algorithm,
            ValidateIssuerSigningKey = settings.ValidateIssuerSigningKey,
            ValidateIssuer = settings.ValidateIssuer,
            ValidateAudience = settings.ValidateAudience,
            ValidateLifetime = settings.ValidateLifetime,
            TokenVersion = settings.TokenVersion,
            UpdateTime = config.UpdateTime,
        };
    }

    /// <summary>
    /// 保存平台 JWT 配置。每次成功保存都会递增全局令牌版本，使所有旧 AccessToken 和 RefreshToken 立即失效。
    /// </summary>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新平台JWT配置")]
    [SqlSugarTransaction]
    public async Task UpdateAsync(PlatformJwtConfigUpdateRequest input)
    {
        var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformJwtConfigConstants.ConfigCode);
        var current = config == null || string.IsNullOrWhiteSpace(config.Value)
                          ? new PlatformJwtSettings()
                          : Deserialize(config.Value);

        var nextSigningKey = input.IssuerSigningKey == PlatformJwtConfigConstants.MaskedSigningKey
                                 ? current.IssuerSigningKey
                                 : input.IssuerSigningKey?.Trim();

        var next = new PlatformJwtSettings
        {
            IssuerSigningKey = nextSigningKey ?? string.Empty,
            ValidIssuer = input.ValidIssuer?.Trim() ?? string.Empty,
            ValidAudience = input.ValidAudience?.Trim() ?? string.Empty,
            ClockSkew = input.ClockSkew,
            TokenVersion = Math.Max(current.TokenVersion, 0) + 1,
        };
        PlatformJwtRuntimeProvider.Validate(next, true);

        var value = JsonSerializer.Serialize(next, SerializerOptions);
        if (config == null)
        {
            config = new SystemConfigEntity
            {
                Name = "平台JWT配置",
                Code = PlatformJwtConfigConstants.ConfigCode,
                Value = value,
                SysFlag = YesNoEnum.Y,
                GroupCode = "Security",
                OrderNo = 10,
                Remark = "平台私有JWT签名与验证配置",
            };
            await _systemConfigRepository.InsertAsync(config);
        }
        else
        {
            config.Name = "平台JWT配置";
            config.Value = value;
            config.SysFlag = YesNoEnum.Y;
            config.GroupCode = "Security";
            config.Remark = "平台私有JWT签名与验证配置";
            await _systemConfigRepository.AsUpdateable(config).UpdateColumns(u => new { u.Name, u.Value, u.SysFlag, u.GroupCode, u.Remark, u.UpdateTime, }).ExecuteCommandAsync();
        }

        _systemCacheService.Remove(PlatformJwtConfigConstants.ConfigCode);
    }

    private static PlatformJwtSettings Deserialize(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<PlatformJwtSettings>(value, SerializerOptions) ?? throw new JsonException();
        }
        catch (JsonException)
        {
            throw BusinessError.Oh("平台JWT配置格式无效");
        }
    }
}