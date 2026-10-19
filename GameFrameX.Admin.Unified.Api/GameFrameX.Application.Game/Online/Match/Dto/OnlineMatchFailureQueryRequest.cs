namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 匹配失败原因 Online 查询请求载荷（Admin → Online 线缆契约，query_match_ticket_failures；mock 先行，联调以 vault C5 详表回写）。
/// <para>不含作用域字段——作用域三元组由 <c>IOnlineServerClient</c> 按 C339 契约注入。</para>
/// </summary>
public class OnlineMatchFailureQueryRequest
{
    /// <summary>
    /// 失败原因码过滤（Online 原值；null = 不过滤）。
    /// </summary>
    public string? ReasonCode { get; set; }

    /// <summary>
    /// 发生时间窗起点（Unix 秒；null = 不限）。
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 发生时间窗终点（Unix 秒；null = 不限）。
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 游标（首页不传）。
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// 页大小。
    /// </summary>
    public int PageSize { get; set; }
}
