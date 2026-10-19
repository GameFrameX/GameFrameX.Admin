using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// 消息审计分页查询输入参数（vault:C7 四类频道；Cursor 翻页，vault:C2 S1.2 定稿）。
/// <para>查询实时透传 Online，Admin 不落消息镜像。</para>
/// </summary>
public class PageOnlineMessageAuditRequest
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
    /// 频道类型过滤（Direct / Party / Group / Global；null = 不过滤）。
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
    /// 发生时间窗起点（Unix 秒；null = 不限）。
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 发生时间窗终点（Unix 秒；null = 不限）。
    /// </summary>
    public long? EndTime { get; set; }

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
