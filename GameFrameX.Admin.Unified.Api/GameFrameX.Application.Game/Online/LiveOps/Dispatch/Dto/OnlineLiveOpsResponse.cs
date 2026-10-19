namespace GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;

/// <summary>
/// Online LiveOps 命令 Data 载荷回执（首版形态，来源 change C343 / vault:C8 S7.7；vault 详表落盘前以此首版为准）。
/// <para>
/// 机器判断已由 <c>OnlineServerClient</c> 在信封层完成（Code 非 0 即抛镜像码），本类型只承接成功 Data 载荷：
/// <c>ConfirmVersion</c> 为 Online 确认版本回执（存入版本行 / 分群行，作一致性断言依据）；
/// <c>SegmentId</c> 仅 <c>syncPlayerSegment</c> 命令回填（Online 侧分配的分群标识，Admin 侧存 LastOnlineSegmentId）。
/// 字段缺省值 = Online 未返回该字段，调用方按可空语义消费，不误判失败。
/// </para>
/// </summary>
public sealed class OnlineLiveOpsResponse
{
    /// <summary>
    /// Online 确认版本回执（配置生效版本表示；空 = Online 未返回版本回执）。
    /// </summary>
    public string? ConfirmVersion { get; set; }

    /// <summary>
    /// Online 侧分配的分群标识（仅 syncPlayerSegment 命令回填；0 = 未返回）。
    /// </summary>
    public long SegmentId { get; set; }

    /// <summary>
    /// 提示消息（Online 侧原文，日志/排查辅助）。
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Online 服务端时间戳（Unix 秒；0 = 未返回）。
    /// </summary>
    public long ServerTime { get; set; }
}
