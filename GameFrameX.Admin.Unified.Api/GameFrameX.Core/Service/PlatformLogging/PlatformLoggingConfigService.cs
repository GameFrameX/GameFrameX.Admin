using GameFrameX.Core.Service.PlatformLogging.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.PlatformLogging;

/// <summary>
/// 平台日志配置服务（SuperAdmin-only）。DetailAsync 不回显 ES 明文 URI；UpdateAsync 用 CAS 乐观锁写回并刷新同 Host 快照。
/// <para>宿主表说明（GFX-706）：本服务不持有独立宿主表，共享主库 SystemConfigEntity（system_config 表），以 Code 分区行 <c>platform_logging_config</c>（PlatformLoggingConfigConstants.ConfigCode）存储本域配置 JSON。Platform* 前缀指平台级配置域，名实相符保留类名。</para>
/// </summary>
[ApiDescriptionSettings(Order = 444)]
public sealed class PlatformLoggingConfigService : IAdminApiController
{
    private readonly SqlSugarRepository<SystemConfigEntity> _systemConfigRepository;
    private readonly PlatformLoggingConfigProvider _configProvider;

    public PlatformLoggingConfigService(SqlSugarRepository<SystemConfigEntity> systemConfigRepository, PlatformLoggingConfigProvider configProvider)
    {
        _systemConfigRepository = systemConfigRepository;
        _configProvider = configProvider;
    }

    /// <summary>
    /// 获取平台日志配置详情。ES ServerUris 元素统一掩码为 <c>"***"</c>，仅保留数量。
    /// </summary>
    [ApiDescriptionSettings(Name = "Detail")]
    [DisplayName("获取平台日志配置详情")]
    public async Task<PlatformLoggingConfigResponse> DetailAsync()
    {
        var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformLoggingConfigConstants.ConfigCode);
        var settings = config == null || string.IsNullOrWhiteSpace(config.Value)
                           ? PlatformLoggingConfigProvider.CreateDefaultSettings()
                           : PlatformLoggingConfigProvider.Deserialize(config.Value);

        PlatformLoggingConfigProvider.Validate(settings);
        return ToOutput(settings, config?.UpdateTime);
    }

    /// <summary>
    /// 保存平台日志配置。ES ServerUris 全为掩码占位时保留库内原值；CAS 乐观锁冲突重试；成功后刷新同 Host 快照。
    /// </summary>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新平台日志配置")]
    [SqlSugarTransaction]
    public async Task UpdateAsync(PlatformLoggingConfigUpdateRequest input)
    {
        const int maxAttempts = 8;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformLoggingConfigConstants.ConfigCode);
            var current = config == null || string.IsNullOrWhiteSpace(config.Value)
                              ? PlatformLoggingConfigProvider.CreateDefaultSettings()
                              : PlatformLoggingConfigProvider.Deserialize(config.Value);
            var next = ToSettings(input, current);
            var value = PlatformLoggingConfigProvider.Serialize(next);

            if (config == null)
            {
                try
                {
                    await _systemConfigRepository.InsertAsync(new SystemConfigEntity
                    {
                        Name = "平台日志配置",
                        Code = PlatformLoggingConfigConstants.ConfigCode,
                        Value = value,
                        SysFlag = YesNoEnum.Y,
                        GroupCode = "Security",
                        OrderNo = 40,
                        Remark = "平台私有日志运行时配置",
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
                                                         Name = "平台日志配置",
                                                         Value = value,
                                                         SysFlag = YesNoEnum.Y,
                                                         GroupCode = "Security",
                                                         Remark = "平台私有日志运行时配置",
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

                    throw BusinessError.Oh("平台日志配置保存冲突，请重试");
                }
            }

            await _configProvider.RefreshAsync();
            return;
        }

        throw BusinessError.Oh("平台日志配置保存冲突，请重试");
    }

    internal static PlatformLoggingConfigResponse ToOutput(PlatformLoggingSettings settings, long? updateTime)
    {
        var esUriCount = settings.ElasticSearch.ServerUris?.Count ?? 0;
        var maskedUris = new List<string>(esUriCount);
        for (var i = 0; i < esUriCount; i++)
        {
            maskedUris.Add(PlatformLoggingConfigConstants.ServerUriMaskPlaceholder);
        }

        return new PlatformLoggingConfigResponse
        {
            File = new PlatformLoggingFileResponse
            {
                IsEnabled = settings.File.IsEnabled,
                FileName = settings.File.FileName,
                Append = settings.File.Append,
                FileSizeLimitBytes = settings.File.FileSizeLimitBytes,
                MaxRollingFiles = settings.File.MaxRollingFiles,
            },
            Database = new PlatformLoggingDatabaseResponse
            {
                IsEnabled = settings.Database.IsEnabled,
                MinimumLevel = settings.Database.MinimumLevel,
            },
            ElasticSearch = new PlatformLoggingElasticSearchResponse
            {
                IsEnabled = settings.ElasticSearch.IsEnabled,
                ServerUris = maskedUris,
                DefaultIndex = settings.ElasticSearch.DefaultIndex,
            },
            Monitor = new PlatformLoggingMonitorResponse
            {
                GlobalEnabled = settings.Monitor.GlobalEnabled,
                BahLogLevel = settings.Monitor.BahLogLevel,
                WithReturnValue = settings.Monitor.WithReturnValue,
                ReturnValueThreshold = settings.Monitor.ReturnValueThreshold,
                JsonBehavior = settings.Monitor.JsonBehavior,
                JsonIndented = settings.Monitor.JsonIndented,
                UseUtcTimestamp = settings.Monitor.UseUtcTimestamp,
            },
            UpdateTime = updateTime,
        };
    }

    /// <summary>
    /// 输入转持久化配置。ES ServerUris 全为掩码占位时沿用当前库内值；否则按非占位项替换；空列表清空。
    /// </summary>
    internal static PlatformLoggingSettings ToSettings(PlatformLoggingConfigUpdateRequest input, PlatformLoggingSettings current)
    {
        var serverUris = ResolveServerUris(input.ElasticSearch.ServerUris, current.ElasticSearch.ServerUris);

        var settings = new PlatformLoggingSettings
        {
            File = new PlatformLoggingFileSettings
            {
                IsEnabled = input.File.IsEnabled,
                FileName = input.File.FileName,
                Append = input.File.Append,
                FileSizeLimitBytes = input.File.FileSizeLimitBytes,
                MaxRollingFiles = input.File.MaxRollingFiles,
            },
            Database = new PlatformLoggingDatabaseSettings
            {
                IsEnabled = input.Database.IsEnabled,
                MinimumLevel = input.Database.MinimumLevel,
            },
            ElasticSearch = new PlatformLoggingElasticSearchSettings
            {
                IsEnabled = input.ElasticSearch.IsEnabled,
                ServerUris = serverUris,
                DefaultIndex = input.ElasticSearch.DefaultIndex,
            },
            Monitor = new PlatformLoggingMonitorSettings
            {
                GlobalEnabled = input.Monitor.GlobalEnabled,
                BahLogLevel = input.Monitor.BahLogLevel,
                WithReturnValue = input.Monitor.WithReturnValue,
                ReturnValueThreshold = input.Monitor.ReturnValueThreshold,
                JsonBehavior = input.Monitor.JsonBehavior,
                JsonIndented = input.Monitor.JsonIndented,
                UseUtcTimestamp = input.Monitor.UseUtcTimestamp,
            },
        };
        PlatformLoggingConfigProvider.Validate(settings);
        return settings;
    }

    private static List<string> ResolveServerUris(List<string> inputUris, List<string> currentUris)
    {
        if (inputUris == null || inputUris.Count == 0)
        {
            return new List<string>();
        }

        if (inputUris.All(u => u == PlatformLoggingConfigConstants.ServerUriMaskPlaceholder))
        {
            return currentUris != null ? new List<string>(currentUris) : new List<string>();
        }

        return inputUris.Where(u => u != PlatformLoggingConfigConstants.ServerUriMaskPlaceholder).ToList();
    }
}