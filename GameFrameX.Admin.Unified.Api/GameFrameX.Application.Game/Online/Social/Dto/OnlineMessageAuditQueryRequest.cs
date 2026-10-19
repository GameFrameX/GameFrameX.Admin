using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// Online 消息审计查询请求体（Admin → Online 线缆契约；mock 先行，联调以 vault 详表回写）。
/// </summary>
public class OnlineMessageAuditQueryRequest
{
    /// <summary>
    /// 频道过滤（null = 不过滤）。
    /// </summary>
    public OnlineChatChannelKind? ChannelKind { get; set; }

    /// <summary>
    /// 参与者玩家 Id 过滤（null = 不过滤）。
    /// </summary>
    public string? ParticipantPlayerId { get; set; }

    /// <summary>
    /// 内容关键词过滤（null = 不过滤）。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 时间窗起点（Unix 秒；null = 不限）。
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 时间窗终点（Unix 秒；null = 不限）。
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 游标（首页不传）。
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// 页大小（1～100）。
    /// </summary>
    public int PageSize { get; set; } = 20;
}
