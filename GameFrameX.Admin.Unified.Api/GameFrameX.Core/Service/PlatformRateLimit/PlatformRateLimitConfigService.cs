using GameFrameX.Core.Service.PlatformRateLimit.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.PlatformRateLimit;

/// <summary>
/// 平台限流配置服务。
/// <para>宿主表说明（GFX-706）：本服务不持有独立宿主表，共享主库 SystemConfigEntity（system_config 表），以 Code 分区行 <c>platform_rate_limit_config</c>（PlatformRateLimitConfigConstants.ConfigCode）存储本域配置 JSON。Platform* 前缀指平台级配置域，名实相符保留类名。</para>
/// </summary>
[ApiDescriptionSettings(Order = 443)]
public sealed class PlatformRateLimitConfigService : IAdminApiController
{
    private readonly SqlSugarRepository<SystemConfigEntity> _systemConfigRepository;
    private readonly PlatformRateLimitConfigProvider _configProvider;

    public PlatformRateLimitConfigService(SqlSugarRepository<SystemConfigEntity> systemConfigRepository, PlatformRateLimitConfigProvider configProvider)
    {
        _systemConfigRepository = systemConfigRepository;
        _configProvider = configProvider;
    }

    /// <summary>
    /// 获取平台限流配置详情。
    /// </summary>
    [ApiDescriptionSettings(Name = "Detail")]
    [DisplayName("获取平台限流配置详情")]
    public async Task<PlatformRateLimitConfigResponse> DetailAsync()
    {
        var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformRateLimitConfigConstants.ConfigCode);
        var settings = config == null || string.IsNullOrWhiteSpace(config.Value)
                           ? PlatformRateLimitConfigProvider.CreateDefaultSettings()
                           : PlatformRateLimitConfigProvider.Deserialize(config.Value);

        PlatformRateLimitConfigProvider.Validate(settings);
        return ToOutput(settings, config?.UpdateTime);
    }

    /// <summary>
    /// 保存平台限流配置。每次成功保存都会递增版本，使后续请求切换到新的 limiter 分区。
    /// </summary>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新平台限流配置")]
    [SqlSugarTransaction]
    public async Task UpdateAsync(PlatformRateLimitConfigUpdateRequest input)
    {
        const int maxAttempts = 8;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformRateLimitConfigConstants.ConfigCode);
            var current = config == null || string.IsNullOrWhiteSpace(config.Value)
                              ? PlatformRateLimitConfigProvider.CreateDefaultSettings()
                              : PlatformRateLimitConfigProvider.Deserialize(config.Value);
            var next = ToSettings(input, Math.Max(current.Version, 0) + 1);
            var value = PlatformRateLimitConfigProvider.Serialize(next);

            if (config == null)
            {
                try
                {
                    await _systemConfigRepository.InsertAsync(new SystemConfigEntity
                    {
                        Name = "平台限流配置",
                        Code = PlatformRateLimitConfigConstants.ConfigCode,
                        Value = value,
                        SysFlag = YesNoEnum.Y,
                        GroupCode = "Security",
                        OrderNo = 30,
                        Remark = "平台私有请求限流配置",
                    });
                }
                catch (Exception) when (attempt < maxAttempts - 1)
                {
                    continue;
                }
            }
            else
            {
                var originalValue = config.Value;
                var now = TimerHelper.UnixTimeSeconds();
                var affected = await _systemConfigRepository.AsUpdateable()
                                                     .SetColumns(u => new SystemConfigEntity
                                                     {
                                                         Name = "平台限流配置",
                                                         Value = value,
                                                         SysFlag = YesNoEnum.Y,
                                                         GroupCode = "Security",
                                                         Remark = "平台私有请求限流配置",
                                                         UpdateTime = now,
                                                     })
                                                     .Where(u => u.Id == config.Id && u.Value == originalValue)
                                                     .ExecuteCommandAsync();

                if (affected == 0)
                {
                    if (attempt < maxAttempts - 1)
                    {
                        continue;
                    }

                    throw BusinessError.Oh("平台限流配置保存冲突，请重试");
                }
            }

            await _configProvider.RefreshAsync();
            return;
        }

        throw BusinessError.Oh("平台限流配置保存冲突，请重试");
    }

    internal static PlatformRateLimitConfigResponse ToOutput(PlatformRateLimitSettings settings, long? updateTime)
    {
        return new PlatformRateLimitConfigResponse
        {
            IsEnabled = settings.IsEnabled,
            Version = settings.Version,
            PermitLimit = settings.PermitLimit,
            WindowSeconds = settings.WindowSeconds,
            QueueLimit = settings.QueueLimit,
            UpdateTime = updateTime,
        };
    }

    internal static PlatformRateLimitSettings ToSettings(PlatformRateLimitConfigUpdateRequest input, long version)
    {
        var settings = new PlatformRateLimitSettings
        {
            IsEnabled = input.IsEnabled,
            Version = version,
            PermitLimit = input.PermitLimit,
            WindowSeconds = input.WindowSeconds,
            QueueLimit = input.QueueLimit,
        };
        PlatformRateLimitConfigProvider.Validate(settings);
        return settings;
    }
}