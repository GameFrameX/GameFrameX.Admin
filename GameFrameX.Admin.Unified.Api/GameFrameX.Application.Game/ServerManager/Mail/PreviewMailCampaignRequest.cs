// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Entity.Game.Mail;

namespace GameFrameX.Application.Game.ServerManager.Mail;

/// <summary>
/// 预览 / 校验 Campaign 请求体。字段与发布请求一致，但不分配 CampaignId。逐字对齐 Server。
/// </summary>
public sealed class PreviewMailCampaignRequest
{
    /// <summary>邮件类型。</summary>
    public MailCampaignMailType MailType { get; set; }

    /// <summary>多语言标题，至少 1 项。</summary>
    public List<MailLocalizedContent> Titles { get; set; } = new();

    /// <summary>多语言正文。</summary>
    public List<MailLocalizedContent> Contents { get; set; } = new();

    /// <summary>附件列表。</summary>
    public List<MailCampaignAttachment> Attachments { get; set; } = new();

    /// <summary>服务器 ID 白名单。</summary>
    public List<int> ServerIds { get; set; } = new();

    /// <summary>渠道 ID 白名单。</summary>
    public List<int> ChannelIds { get; set; } = new();

    /// <summary>最低等级。</summary>
    public int MinLevel { get; set; }

    /// <summary>最高等级。</summary>
    public int MaxLevel { get; set; }

    /// <summary>附件过期处置策略。</summary>
    public MailCampaignExpirePolicy ExpireAttachmentPolicy { get; set; }

    /// <summary>过期时间（Unix 秒，UTC）。</summary>
    public long ExpireTime { get; set; }
}