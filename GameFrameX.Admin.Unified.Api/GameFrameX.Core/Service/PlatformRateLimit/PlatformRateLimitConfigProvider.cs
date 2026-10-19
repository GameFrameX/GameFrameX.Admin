using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.PlatformRateLimit;

/// <summary>
/// 平台限流运行时配置提供器。
/// </summary>
public sealed class PlatformRateLimitConfigProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IServiceScopeFactory _scopeFactory;
    private PlatformRateLimitSettings _snapshot = CreateDefaultSettings();

    public PlatformRateLimitConfigProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// 获取当前请求使用的不可变快照。请求限流路径是同步入口，不在这里执行数据库 I/O。
    /// </summary>
    public PlatformRateLimitSettings GetSnapshot()
    {
        var settings = _snapshot;
        return new PlatformRateLimitSettings
        {
            IsEnabled = settings.IsEnabled,
            Version = settings.Version,
            PermitLimit = settings.PermitLimit,
            WindowSeconds = settings.WindowSeconds,
            QueueLimit = settings.QueueLimit,
        };
    }

    /// <summary>
    /// 启动或配置保存后刷新当前 Host 内存快照；缺失记录会写入与旧硬编码等价的默认值。
    /// </summary>
    public async Task RefreshAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemConfigEntity>>();
        var config = await repository.GetFirstAsync(u => u.Code == PlatformRateLimitConfigConstants.ConfigCode);
        if (config == null || string.IsNullOrWhiteSpace(config.Value))
        {
            var defaults = CreateDefaultSettings();
            await repository.InsertAsync(new SystemConfigEntity
            {
                Name = "平台限流配置",
                Code = PlatformRateLimitConfigConstants.ConfigCode,
                Value = Serialize(defaults),
                SysFlag = YesNoEnum.Y,
                GroupCode = "Security",
                OrderNo = 30,
                Remark = "平台私有请求限流配置",
            });

            _snapshot = defaults;
            return;
        }

        var settings = Deserialize(config.Value);
        Validate(settings);
        _snapshot = settings;
    }

    /// <summary>
    /// 创建与旧 Startup 硬编码等价的默认配置。
    /// </summary>
    public static PlatformRateLimitSettings CreateDefaultSettings()
    {
        return new PlatformRateLimitSettings();
    }

    /// <summary>
    /// 生成包含配置版本的 limiter 分区键。
    /// </summary>
    public static string BuildPartitionKey(PlatformRateLimitSettings settings, string ip)
    {
        return $"v{settings.Version}:{ip}";
    }

    /// <summary>
    /// 反序列化平台限流配置。
    /// </summary>
    public static PlatformRateLimitSettings Deserialize(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<PlatformRateLimitSettings>(value, SerializerOptions) ?? throw new JsonException();
        }
        catch (JsonException)
        {
            throw BusinessError.Oh("平台限流配置格式无效").StatusCode(500);
        }
    }

    /// <summary>
    /// 序列化并校验长度。
    /// </summary>
    public static string Serialize(PlatformRateLimitSettings settings)
    {
        Validate(settings);
        var value = JsonSerializer.Serialize(settings, SerializerOptions);
        if (value.Length > PlatformRateLimitConfigConstants.MaxSerializedLength)
        {
            throw BusinessError.Oh($"平台限流配置过长，不能超过 {PlatformRateLimitConfigConstants.MaxSerializedLength} 个字符");
        }

        return value;
    }

    /// <summary>
    /// 校验平台限流配置。
    /// </summary>
    public static void Validate(PlatformRateLimitSettings settings)
    {
        if (settings == null)
        {
            throw BusinessError.Oh("平台限流配置不能为空").StatusCode(500);
        }

        if (settings.Version <= 0)
        {
            throw BusinessError.Oh("平台限流配置版本无效").StatusCode(500);
        }

        if (settings.PermitLimit <= 0 || settings.PermitLimit > 100000)
        {
            throw BusinessError.Oh("平台限流请求数必须在 1 到 100000 之间").StatusCode(500);
        }

        if (settings.WindowSeconds <= 0 || settings.WindowSeconds > 3600)
        {
            throw BusinessError.Oh("平台限流窗口秒数必须在 1 到 3600 之间").StatusCode(500);
        }

        if (settings.QueueLimit < 0 || settings.QueueLimit > 100000)
        {
            throw BusinessError.Oh("平台限流排队数必须在 0 到 100000 之间").StatusCode(500);
        }
    }
}