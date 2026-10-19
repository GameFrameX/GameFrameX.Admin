using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;

/// <summary>
/// LiveOps 统一下发回执（来源 change C343 / vault:C8 S7.7）。
/// <para>透传 Online 确认版本 / 状态供前端回显；<c>Merged</c> 标记邮件来源「同目标合并返回首次回执」路径。</para>
/// </summary>
public sealed class OnlineLiveOpsDispatchResponse
{
    /// <summary>
    /// 来源类型（回显请求值）。
    /// </summary>
    public LiveOpsSourceKind Source { get; set; }

    /// <summary>
    /// 来源域业务主键（回显请求值）。
    /// </summary>
    public long BusinessKey { get; set; }

    /// <summary>
    /// 实际路由到的下发通道标识：四类配置来源为 Online admin API 命令名（如 <c>publishRemoteConfig</c>）；
    /// 邮件来源为既有链路标识 <c>publish_mail_campaign</c>（MailCampaignServerClient / Fulfillment）。
    /// </summary>
    public string DispatchChannel { get; set; } = null!;

    /// <summary>
    /// Online / Server 确认版本（配置生效版本或 Campaign 发布版本；空 = 链路未返回版本回执）。
    /// </summary>
    public string? OnlineConfirmVersion { get; set; }

    /// <summary>
    /// 是否同目标合并（仅邮件来源：目标 Campaign 已发布，未重复下发，直接返回首次发布回执）。
    /// </summary>
    public bool Merged { get; set; }

    /// <summary>
    /// 回执消息（Online MessageKey / 链路消息原文，排查辅助）。
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 下发时间（Unix 秒，UTC）。
    /// </summary>
    public long DispatchedTime { get; set; }
}
