namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 异常 Ticket Online 查询请求载荷（Admin → Online 线缆契约，query_match_abnormal_tickets；mock 先行，联调以 vault C5 详表回写）。
/// <para>不含作用域字段——作用域三元组由 <c>IOnlineServerClient</c> 按 C339 契约注入。</para>
/// </summary>
public class OnlineMatchAbnormalTicketQueryRequest
{
    /// <summary>
    /// 异常类别（Admin 自有检索枚举，透传 Online 侧过滤）。
    /// </summary>
    public AbnormalKind AbnormalKind { get; set; }

    /// <summary>
    /// 游标（首页不传）。
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// 页大小。
    /// </summary>
    public int PageSize { get; set; }
}
