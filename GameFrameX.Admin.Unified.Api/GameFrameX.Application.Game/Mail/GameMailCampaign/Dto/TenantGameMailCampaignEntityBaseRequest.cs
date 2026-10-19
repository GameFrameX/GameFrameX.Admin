// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Entity.Game.Mail;

namespace GameFrameX.Application.Game.Mail.GameMailCampaign.Dto;

/// <summary>
/// 运营邮件 Campaign 实体输入基类（AddAsync / UpdateAsync 公共字段）。
/// <para>多语言内容 / 附件 / 过滤条件以 JSON 字符串承载（与实体列一致），由前端编辑器序列化后传入。</para>
/// </summary>
public abstract class TenantGameMailCampaignEntityBaseRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用Id。事实数据应用作用域，必须 &gt; 0。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服Id。用于解析目标游戏服 HttpManageUrl 调用 Server 发布接口。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    public long AreaId { get; set; }

    /// <summary>
    /// Campaign 名称（Admin 侧识别用）。
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 模板 Id（0 = 无模板，裸文案发布）。
    /// </summary>
    public long TemplateId { get; set; }

    /// <summary>
    /// 邮件类型（System/Operation/Compensation），对齐 Server MailType。
    /// </summary>
    public MailCampaignMailType MailType { get; set; }

    /// <summary>
    /// 多语言标题 JSON（ListAsync&lt;MailLocalizedContent&gt;）。
    /// </summary>
    [Required(ErrorMessage = "多语言标题不能为空")]
    public string TitlesJson { get; set; } = null!;

    /// <summary>
    /// 多语言正文 JSON（ListAsync&lt;MailLocalizedContent&gt;）。
    /// </summary>
    public string ContentsJson { get; set; } = null!;

    /// <summary>
    /// 多语言发件人名称 JSON（ListAsync&lt;MailLocalizedContent&gt;）。
    /// </summary>
    public string SenderNamesJson { get; set; } = null!;

    /// <summary>
    /// 附件列表 JSON（ListAsync&lt;MailCampaignAttachment&gt;）。
    /// </summary>
    public string AttachmentsJson { get; set; } = null!;

    /// <summary>
    /// 服务器 Id 白名单 JSON（ListAsync&lt;int&gt;）。
    /// </summary>
    public string ServerIdsJson { get; set; } = null!;

    /// <summary>
    /// 渠道 Id 白名单 JSON（ListAsync&lt;int&gt;）。
    /// </summary>
    public string ChannelIdsJson { get; set; } = null!;

    /// <summary>
    /// 最低等级（含），0 表示不限。
    /// </summary>
    public int MinLevel { get; set; }

    /// <summary>
    /// 最高等级（含），0 表示不限。
    /// </summary>
    public int MaxLevel { get; set; }

    /// <summary>
    /// 附件过期处置策略（B4）。
    /// </summary>
    public MailCampaignExpirePolicy ExpireAttachmentPolicy { get; set; }

    /// <summary>
    /// 过期时间（Unix 秒，UTC），0 表示永不过期。
    /// </summary>
    public long ExpireTime { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string Remark { get; set; } = null!;
}