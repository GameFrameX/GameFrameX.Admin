namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 队列总览 Online 查询请求载荷（Admin → Online 线缆契约，query_match_queue_overview；mock 先行，联调以 vault C5 详表回写）。
/// <para>不含作用域字段——作用域三元组由 <c>IOnlineServerClient</c> 按 C339 契约注入。</para>
/// </summary>
public class OnlineMatchOverviewQueryRequest
{
    /// <summary>
    /// 匹配模式过滤（null = 全部模式）。
    /// </summary>
    public string? Mode { get; set; }

    /// <summary>
    /// 地区过滤（null = 全部地区）。
    /// </summary>
    public string? Region { get; set; }
}
