using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// 消息审计条目展示（脱敏后）。字段经 <c>OnlineMessageAuditDisplayMapper</c> 从 Online 消息对象映射。
/// </summary>
public class OnlineMessageAuditItemResponse
{
    /// <summary>
    /// 消息 Id（Online 侧标识，原样透传）。
    /// </summary>
    public string MessageId { get; set; } = null!;

    /// <summary>
    /// 频道类型（Direct / Party / Group / Global）。
    /// </summary>
    public OnlineChatChannelKind ChannelKind { get; set; }

    /// <summary>
    /// 参与者玩家 Id 列表（Direct=双方、Party/Group=成员、Global=发送者）。
    /// </summary>
    public List<string> ParticipantPlayerIds { get; set; } = new();

    /// <summary>
    /// 消息内容（展示层已按 Online 脱敏规则处理 + Admin 兜底脱敏，见 DisplayMapper）。
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 发生时间（Unix 秒）。
    /// </summary>
    public long OccurredAt { get; set; }
}
