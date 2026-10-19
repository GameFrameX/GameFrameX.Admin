namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// Online 当事人通知发送请求体（Admin → Online 线缆契约；mock 先行，联调以 vault 详表回写）。
/// </summary>
public class OnlineNotificationSendRequest
{
    /// <summary>
    /// 接收玩家 Id（被举报人；举报人侧回执由 Online 案件流负责，不经 Admin）。
    /// </summary>
    public string PlayerId { get; set; } = null!;

    /// <summary>
    /// 通知标题。
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 通知内容（处置结果摘要，不含审计内部字段）。
    /// </summary>
    public string Content { get; set; } = null!;
}
