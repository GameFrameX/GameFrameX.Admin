namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 玩家时间线查询线缆请求（vault:C9 契约；mock 先行，联调以 vault 详表为准）。
/// <para>作用域三元组由 OnlineScopeContext 注入，不在线缆体内重复。</para>
/// </summary>
public class OnlinePlayerTimelineQueryRequest
{
    /// <summary>
    /// 目标玩家 Id。
    /// </summary>
    public string PlayerId { get; set; } = null!;

    /// <summary>
    /// 事件分组过滤（Online 侧原值；null = 全部分组）。
    /// </summary>
    public string? EventGroup { get; set; }

    /// <summary>
    /// 时间窗起点（Unix 秒；null = 不限）。
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 时间窗终点（Unix 秒；null = 不限）。
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 游标（Online 侧原生游标往返）。
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// 页大小。
    /// </summary>
    public int PageSize { get; set; }
}
