// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Entity.Game.Mail;

namespace GameFrameX.Application.Game.Mail.GameMailCampaign.Dto;

/// <summary>
/// Campaign 输出 DTO。
/// </summary>
public sealed class TenantGameMailCampaignEntityResponse
{
    /// <summary>主键Id。</summary>
    public long Id { get; set; }

    /// <summary>应用Id。</summary>
    public long AppId { get; set; }

    /// <summary>区服Id。</summary>
    public long AreaId { get; set; }

    /// <summary>名称。</summary>
    public string Name { get; set; } = null!;

    /// <summary>模板Id。</summary>
    public long TemplateId { get; set; }

    /// <summary>邮件类型。</summary>
    public MailCampaignMailType MailType { get; set; }

    /// <summary>多语言标题 JSON。</summary>
    public string TitlesJson { get; set; } = null!;

    /// <summary>多语言正文 JSON。</summary>
    public string ContentsJson { get; set; } = null!;

    /// <summary>多语言发件人名称 JSON。</summary>
    public string SenderNamesJson { get; set; } = null!;

    /// <summary>附件列表 JSON。</summary>
    public string AttachmentsJson { get; set; } = null!;

    /// <summary>服务器Id白名单 JSON。</summary>
    public string ServerIdsJson { get; set; } = null!;

    /// <summary>渠道Id白名单 JSON。</summary>
    public string ChannelIdsJson { get; set; } = null!;

    /// <summary>最低等级。</summary>
    public int MinLevel { get; set; }

    /// <summary>最高等级。</summary>
    public int MaxLevel { get; set; }

    /// <summary>附件过期处置策略。</summary>
    public MailCampaignExpirePolicy ExpireAttachmentPolicy { get; set; }

    /// <summary>过期时间（Unix 秒）。</summary>
    public long ExpireTime { get; set; }

    /// <summary>发布状态。</summary>
    public MailCampaignPublishStatus PublishStatus { get; set; }

    /// <summary>Server CampaignId。</summary>
    public long ServerCampaignId { get; set; }

    /// <summary>发布版本号。</summary>
    public int PublishVersion { get; set; }

    /// <summary>发布时间（Unix 秒）。</summary>
    public long PublishedTime { get; set; }

    /// <summary>撤回时间（Unix 秒）。</summary>
    public long RevokedTime { get; set; }

    /// <summary>发布操作人。</summary>
    public string PublishOperator { get; set; } = null!;

    /// <summary>撤回操作人。</summary>
    public string RevokeOperator { get; set; } = null!;

    /// <summary>最近Server业务码。</summary>
    public MailCampaignServerCode LastServerCode { get; set; }

    /// <summary>最近Server消息。</summary>
    public string LastServerMessage { get; set; } = null!;

    /// <summary>备注。</summary>
    public string Remark { get; set; } = null!;

    /// <summary>创建时间。</summary>
    public long CreatedTime { get; set; }

    /// <summary>行版本号（乐观并发）。</summary>
    public long? RowVersion { get; set; }
}