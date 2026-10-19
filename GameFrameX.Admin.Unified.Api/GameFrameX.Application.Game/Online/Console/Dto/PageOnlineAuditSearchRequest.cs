namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 统一审计检索分页输入参数（五维过滤：操作者/对象/作用域/命令类型/时间窗；change C344 / vault:C9 S8.2）。
/// <para>
/// 作用域维度（AppId/ServerId）在此是可选过滤而非调用前置：本地两来源（Admin 操作日志 / 域命令记录）
/// 支持无作用域全量检索；Online 审计事件来源仅在 AppId + ServerId 均提供时参与聚合
/// （C339 作用域三元组是每次 Online 调用硬前置），未锚定时该来源静默跳过（三来源口径见 OnlineAuditSourceMap）。
/// </para>
/// </summary>
public class PageOnlineAuditSearchRequest
{
    /// <summary>
    /// 操作者过滤（Admin 操作日志按账号/姓名匹配，Online/域命令按操作者 Id；null = 不过滤）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? Operator { get; set; }

    /// <summary>
    /// 目标对象过滤（玩家 Id 等；null = 不过滤。Admin 操作日志来源经 RequestParam 提取尽力匹配，提取不到的行不命中）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? TargetPlayerId { get; set; }

    /// <summary>
    /// 应用作用域过滤（null = 全部应用；Online 审计事件来源要求与 ServerId 同时提供才参与聚合）。
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "应用Id必须大于0")]
    public long? AppId { get; set; }

    /// <summary>
    /// 区服作用域过滤（null = 全部区服；Online 审计事件来源要求与 AppId 同时提供才参与聚合）。
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long? ServerId { get; set; }

    /// <summary>
    /// 命令类型过滤（substring 匹配，如 penalty / grant / rollout；null = 不过滤）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
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
    /// 游标（上一页返回的 NextCursor = 末行 OccurredAt Unix 秒的时间倒序 keyset；首页不传）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? Cursor { get; set; }

    /// <summary>
    /// 页大小（1～100，默认 20）。
    /// </summary>
    [Range(1, 100, ErrorMessage = "页大小必须在1~100之间")]
    public int PageSize { get; set; } = 20;
}
