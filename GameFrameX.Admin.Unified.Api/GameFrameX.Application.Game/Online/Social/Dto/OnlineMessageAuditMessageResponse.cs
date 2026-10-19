using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// Online 消息审计单行数据（Admin → Online 线缆契约；mock 先行，联调以 vault 详表回写）。
/// </summary>
public class OnlineMessageAuditMessageResponse
{
    /// <summary>
    /// 消息 Id（Online 侧标识）。
    /// </summary>
    public string MessageId { get; set; } = null!;

    /// <summary>
    /// 频道类型。
    /// </summary>
    public OnlineChatChannelKind ChannelKind { get; set; }

    /// <summary>
    /// 参与者玩家 Id 集（私聊两端 / 队伍 / 群成员 / 世界频道发言者）。
    /// </summary>
    public List<string> ParticipantPlayerIds { get; set; } = new();

    /// <summary>
    /// 消息原文（未脱敏；仅审计透传，展示侧统一走 Admin Mapper 脱敏）。
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 发生时间（Unix 秒）。
    /// </summary>
    public long OccurredAt { get; set; }
}
