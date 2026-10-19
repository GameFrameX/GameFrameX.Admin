using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.PlatformLogging;

/// <summary>
/// 平台日志运行时配置提供器。维护同一 Admin Host 内的内存快照：启动期由
/// <see cref="GameFrameX.Web.Api.Handlers.PlatformLoggingStartupLoader"/> 调 <see cref="RefreshAsync"/>
/// 从主库 <see cref="SystemConfigEntity"/> 加载；SuperAdmin 保存配置后由配置服务调用 <see cref="RefreshAsync"/>
/// 刷新快照，使后续 <c>DatabaseLoggingWriter</c> / <c>ElasticSearchLoggingWriter</c> 写路径读到新开关。
/// </summary>
public sealed class PlatformLoggingConfigProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IServiceScopeFactory _scopeFactory;
    private PlatformLoggingSettings _snapshot = CreateDefaultSettings();

    public PlatformLoggingConfigProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// 获取当前不可变快照副本。写路径（日志 Writer）同步调用，不在这里执行数据库 I/O。
    /// </summary>
    public PlatformLoggingSettings GetSnapshot()
    {
        var settings = _snapshot;
        return CloneSettings(settings);
    }

    /// <summary>
    /// 启动或配置保存后刷新当前 Host 内存快照；缺失记录会写入与旧 Logging.json 等价的安全默认值。
    /// </summary>
    public async Task RefreshAsync()
    {
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemConfigEntity>>();
            var config = await repository.GetFirstAsync(u => u.Code == PlatformLoggingConfigConstants.ConfigCode);
            if (config == null || string.IsNullOrWhiteSpace(config.Value))
            {
                var defaults = CreateDefaultSettings();
                await repository.InsertAsync(new SystemConfigEntity
                {
                    Name = "平台日志配置",
                    Code = PlatformLoggingConfigConstants.ConfigCode,
                    Value = Serialize(defaults),
                    SysFlag = YesNoEnum.Y,
                    GroupCode = "Security",
                    OrderNo = 40,
                    Remark = "平台私有日志运行时配置",
                });

                _snapshot = defaults;
                return;
            }

            var settings = Deserialize(config.Value);
            Validate(settings);
            _snapshot = settings;
        }
    }

    /// <summary>
    /// 创建与旧 Logging.json 安全默认等价的默认配置（文件/数据库开启、ES 关闭）。
    /// </summary>
    public static PlatformLoggingSettings CreateDefaultSettings()
    {
        return new PlatformLoggingSettings();
    }

    /// <summary>
    /// 反序列化平台日志配置。格式非法时抛业务错误（500）。
    /// </summary>
    public static PlatformLoggingSettings Deserialize(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<PlatformLoggingSettings>(value, SerializerOptions) ?? throw new JsonException();
        }
        catch (JsonException)
        {
            throw BusinessError.Oh("平台日志配置格式无效").StatusCode(500);
        }
    }

    /// <summary>
    /// 序列化并校验长度，确保不超 <see cref="SystemConfigEntity"/> 的 Value 列长度。
    /// </summary>
    public static string Serialize(PlatformLoggingSettings settings)
    {
        Validate(settings);
        var value = JsonSerializer.Serialize(settings, SerializerOptions);
        if (value.Length > PlatformLoggingConfigConstants.MaxSerializedLength)
        {
            throw BusinessError.Oh($"平台日志配置过长，不能超过 {PlatformLoggingConfigConstants.MaxSerializedLength} 个字符");
        }

        return value;
    }

    /// <summary>
    /// 校验平台日志配置：级别名可解析、阈值在区间内、JSON 行为合法。
    /// </summary>
    public static void Validate(PlatformLoggingSettings settings)
    {
        if (settings == null)
        {
            throw BusinessError.Oh("平台日志配置不能为空").StatusCode(500);
        }

        if (settings.File == null || settings.Database == null || settings.ElasticSearch == null || settings.Monitor == null)
        {
            throw BusinessError.Oh("平台日志配置分组缺失").StatusCode(500);
        }

        if (settings.File.FileSizeLimitBytes < 1024 || settings.File.FileSizeLimitBytes > 1073741824)
        {
            throw BusinessError.Oh("日志单文件大小必须在 1024 到 1073741824 字节之间").StatusCode(500);
        }

        if (settings.File.MaxRollingFiles < 1 || settings.File.MaxRollingFiles > 365)
        {
            throw BusinessError.Oh("日志滚动文件数必须在 1 到 365 之间").StatusCode(500);
        }

        if (settings.Monitor.ReturnValueThreshold < 0 || settings.Monitor.ReturnValueThreshold > 100000)
        {
            throw BusinessError.Oh("监控返回值阈值必须在 0 到 100000 之间").StatusCode(500);
        }

        EnsureLogLevelName(settings.Database.MinimumLevel, "数据库日志级别");
        EnsureLogLevelName(settings.Monitor.BahLogLevel, "监控业务日志级别");
        EnsureJsonBehavior(settings.Monitor.JsonBehavior);

        if (settings.ElasticSearch.ServerUris == null)
        {
            settings.ElasticSearch.ServerUris = new List<string>();
        }
    }

    private static void EnsureLogLevelName(string levelName, string fieldTitle)
    {
        if (string.IsNullOrWhiteSpace(levelName) || !System.Enum.TryParse<LogLevel>(levelName, true, out _))
        {
            throw BusinessError.Oh($"{fieldTitle}必须是合法的 LogLevel 名称").StatusCode(500);
        }
    }

    private static void EnsureJsonBehavior(string jsonBehavior)
    {
        if (string.IsNullOrWhiteSpace(jsonBehavior)
            || (!string.Equals(jsonBehavior, "None", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(jsonBehavior, "OnlyJson", StringComparison.OrdinalIgnoreCase)))
        {
            throw BusinessError.Oh("监控 JSON 行为必须是 None 或 OnlyJson").StatusCode(500);
        }
    }

    private static PlatformLoggingSettings CloneSettings(PlatformLoggingSettings source)
    {
        return new PlatformLoggingSettings
        {
            File = new PlatformLoggingFileSettings
            {
                IsEnabled = source.File.IsEnabled,
                FileName = source.File.FileName,
                Append = source.File.Append,
                FileSizeLimitBytes = source.File.FileSizeLimitBytes,
                MaxRollingFiles = source.File.MaxRollingFiles,
            },
            Database = new PlatformLoggingDatabaseSettings
            {
                IsEnabled = source.Database.IsEnabled,
                MinimumLevel = source.Database.MinimumLevel,
            },
            ElasticSearch = new PlatformLoggingElasticSearchSettings
            {
                IsEnabled = source.ElasticSearch.IsEnabled,
                ServerUris = source.ElasticSearch.ServerUris != null ? new List<string>(source.ElasticSearch.ServerUris) : new List<string>(),
                DefaultIndex = source.ElasticSearch.DefaultIndex,
            },
            Monitor = new PlatformLoggingMonitorSettings
            {
                GlobalEnabled = source.Monitor.GlobalEnabled,
                BahLogLevel = source.Monitor.BahLogLevel,
                WithReturnValue = source.Monitor.WithReturnValue,
                ReturnValueThreshold = source.Monitor.ReturnValueThreshold,
                JsonBehavior = source.Monitor.JsonBehavior,
                JsonIndented = source.Monitor.JsonIndented,
                UseUtcTimestamp = source.Monitor.UseUtcTimestamp,
            },
        };
    }
}