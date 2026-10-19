namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 异常 Ticket 分页查询输入参数（change C341 §3；按异常类别检索，Cursor 翻页透传 Online）。
/// </summary>
public class PageOnlineMatchAbnormalTicketRequest
{
    /// <summary>
    /// 应用作用域（必填，经 OnlineScopeContextFactory 校验租户归属）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id必须大于0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服作用域（必填，经 OnlineScopeContextFactory 校验租户与应用归属）。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long ServerId { get; set; }

    /// <summary>
    /// 异常类别（必填：LongWaiting 长时间未分配 / RepeatedCancel 重复取消 / Orphan 孤儿残留）。
    /// </summary>
    [Required(ErrorMessage = "异常类别不能为空")]
    public AbnormalKind AbnormalKind { get; set; }

    /// <summary>
    /// 游标（上一页返回的 NextCursor；首页不传）。
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// 页大小（1～100，默认 20）。
    /// </summary>
    [Range(1, 100, ErrorMessage = "页大小必须在1~100之间")]
    public int PageSize { get; set; } = 20;
}
