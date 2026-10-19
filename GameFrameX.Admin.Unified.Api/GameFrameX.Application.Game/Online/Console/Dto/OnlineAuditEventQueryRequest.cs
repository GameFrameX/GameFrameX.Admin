namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// Online 审计事件查询线缆请求（vault:C9 S8.2 契约；mock 先行，联调以 vault 详表为准）。
/// <para>作用域三元组由 OnlineScopeContext 注入，不在线缆体内重复。</para>
/// </summary>
public class OnlineAuditEventQueryRequest
{
    /// <summary>
    /// 操作者 Id 过滤（null = 不过滤）。
    /// </summary>
    public string? OperatorId { get; set; }

    /// <summary>
    /// 目标玩家 Id 过滤（null = 不过滤）。
    /// </summary>
    public string? TargetPlayerId { get; set; }

    /// <summary>
    /// 命令类型过滤（null = 不过滤）。
    /// </summary>
    public string? CommandKind { get; set; }

    /// <summary>
    /// 发生时间窗起点（Unix 秒；null = 不限）。
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 发生时间窗终点（Unix 秒；null = 不限）。
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
