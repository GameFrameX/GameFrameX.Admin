namespace GameFrameX.Application.Game.App.FeatureSwitch;

/// <summary>
/// 活动/功能开关投放解析入参。
/// <para>作用域维度（<see cref="Platform"/> / <see cref="PackageCandidates"/> / <see cref="Language"/> /
/// <see cref="Channel"/>）为 null 时表示调用方无该维度上下文，只匹配 Default 哨兵行；
/// 有值时同时匹配具体值与 Default 哨兵行，再由作用域评分决定 winner。</para>
/// </summary>
public sealed record FeatureSwitchDeliveryQuery
{
    /// <summary>
    /// 当前租户 ID（必填，租户库路由）
    /// </summary>
    public long TenantId { get; init; }

    /// <summary>
    /// 应用作用域；null / 0 表示只取租户默认层
    /// </summary>
    public long? AppId { get; init; }

    /// <summary>
    /// 下发通道视角，决定消费方标记过滤方向
    /// </summary>
    public FeatureSwitchAudience Audience { get; init; }

    /// <summary>
    /// 平台；null 表示仅匹配 Default 哨兵行
    /// </summary>
    public string? Platform { get; init; }

    /// <summary>
    /// 包名候选；null / 空表示仅匹配 Default 哨兵行。
    /// <para>母包候选由调用方解析后传入——Application.Game 不依赖 Client.Api 的母包解析器。</para>
    /// </summary>
    public IReadOnlyList<string>? PackageCandidates { get; init; }

    /// <summary>
    /// 语言；null 表示仅匹配 Default 哨兵行
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// 渠道；null 表示仅匹配 Default 哨兵行
    /// </summary>
    public string? Channel { get; init; }

    /// <summary>
    /// 稳定设备标识。客户端通道缺失时只命中全量投放记录；服务器通道忽略设备灰度
    /// </summary>
    public string? DeviceId { get; init; }

    /// <summary>
    /// 本次判定的 UTC 服务器时间，用于时间窗过滤（不依赖机器本地时区）
    /// </summary>
    public DateTime NowUtc { get; init; }
}
