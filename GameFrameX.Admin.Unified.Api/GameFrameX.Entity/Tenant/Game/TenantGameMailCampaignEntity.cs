// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Entity.Base;
using GameFrameX.Entity.Game.Mail;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 运营邮件 Campaign 草稿/发布记录。Admin 侧承载草稿编辑（多语言内容、附件、过滤、过期策略）+ Server 回写的发布状态。
/// <para>生命周期 <c>Draft → Published → Revoked</c>（+ <c>Failed</c>）；发布后主体字段不可修改（B1）、不可删除，仅可撤回。</para>
/// <para>与既有 <c>TenantGameMailEntity</c>（历史直发 <c>post_mail</c>）是两套独立邮件系统，不合并、字段不混用。</para>
/// </summary>
[SugarTable(null, "运营邮件Campaign")]
[SugarIndex("idx_tenant_game_mail_campaign_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
public class TenantGameMailCampaignEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0；Campaign 面向特定应用玩家群体。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服Id。用于解析目标游戏服 <c>TenantGameAreaEntity.HttpManageUrl</c> 调用 Server 发布接口（与既有直发邮件同口径）。
    /// </summary>
    [SugarColumn(ColumnDescription = "区服Id", DefaultValue = "0")]
    public long AreaId { get; set; }

    /// <summary>
    /// Campaign 名称（Admin 侧识别用，不出现在 Server 契约）。
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 模板 Id。引用既有 <c>TenantGameMailTemplateEntity</c>（0 = 无模板，裸文案发布）；Server 仅作信息引用，内容冻结在本实体。
    /// </summary>
    [SugarColumn(ColumnDescription = "模板Id", DefaultValue = "0")]
    public long TemplateId { get; set; }

    /// <summary>
    /// 邮件类型（System/Operation/Compensation）。值域对齐 Server <c>MailType</c>，不复用既有 <c>GameMailType</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件类型", DefaultValue = "0")]
    public MailCampaignMailType MailType { get; set; }

    /// <summary>
    /// 多语言标题 JSON（<c>List&lt;MailLocalizedContent&gt;</c>）。至少 1 项；缺失语言由客户端回退首项。
    /// </summary>
    [SugarColumn(ColumnDescription = "多语言标题", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string TitlesJson { get; set; } = null!;

    /// <summary>
    /// 多语言正文 JSON（<c>List&lt;MailLocalizedContent&gt;</c>）。规则同 <c>TitlesJson</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "多语言正文", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string ContentsJson { get; set; } = null!;

    /// <summary>
    /// 多语言发件人名称 JSON（<c>List&lt;MailLocalizedContent&gt;</c>）。规则同 <c>TitlesJson</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "多语言发件人名称", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string SenderNamesJson { get; set; } = null!;

    /// <summary>
    /// 附件槽位 JSON（<c>List&lt;MailCampaignAttachment&gt;</c>）。玩家领取时按 SlotId 逐项走统一奖励发放接口。
    /// </summary>
    [SugarColumn(ColumnDescription = "附件列表", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string AttachmentsJson { get; set; } = null!;

    /// <summary>
    /// 命中过滤：游戏服实例 Id 白名单 JSON（<c>List&lt;int&gt;</c>）。空列表表示不限服务器。
    /// </summary>
    [SugarColumn(ColumnDescription = "服务器Id白名单", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string ServerIdsJson { get; set; } = null!;

    /// <summary>
    /// 命中过滤：渠道 Id 白名单 JSON（<c>List&lt;int&gt;</c>）。空列表表示不限渠道。
    /// </summary>
    [SugarColumn(ColumnDescription = "渠道Id白名单", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string ChannelIdsJson { get; set; } = null!;

    /// <summary>
    /// 命中过滤：最低玩家等级（含）。0 表示不设下限。
    /// </summary>
    [SugarColumn(ColumnDescription = "最低等级", DefaultValue = "0")]
    public int MinLevel { get; set; }

    /// <summary>
    /// 命中过滤：最高玩家等级（含）。0 表示不设上限。
    /// </summary>
    [SugarColumn(ColumnDescription = "最高等级", DefaultValue = "0")]
    public int MaxLevel { get; set; }

    /// <summary>
    /// 附件过期处置策略（Server B4）。撤回或自然过期后未领取附件的处理方式。
    /// </summary>
    [SugarColumn(ColumnDescription = "附件过期处置策略", DefaultValue = "0")]
    public MailCampaignExpirePolicy ExpireAttachmentPolicy { get; set; }

    /// <summary>
    /// 邮件过期时间（Unix 秒，UTC）。0 表示永不过期；过期后未领取附件按 <c>ExpireAttachmentPolicy</c> 处理。
    /// </summary>
    [SugarColumn(ColumnDescription = "过期时间(Unix秒)", DefaultValue = "0")]
    public long ExpireTime { get; set; }

    /// <summary>
    /// Admin 侧生命周期状态（Draft/Published/Revoked/Failed）。
    /// </summary>
    [SugarColumn(ColumnDescription = "发布状态", DefaultValue = "0")]
    public MailCampaignPublishStatus PublishStatus { get; set; }

    /// <summary>
    /// Server 分配的 Campaign Id。发布成功前为 0；发布后回写且不可变。
    /// </summary>
    [SugarColumn(ColumnDescription = "Server CampaignId", DefaultValue = "0")]
    public long ServerCampaignId { get; set; }

    /// <summary>
    /// Server 发布版本号。发布成功回写。
    /// </summary>
    [SugarColumn(ColumnDescription = "发布版本号", DefaultValue = "0")]
    public int PublishVersion { get; set; }

    /// <summary>
    /// 发布时间（Unix 秒，UTC）。Server 发布成功回写。
    /// </summary>
    [SugarColumn(ColumnDescription = "发布时间(Unix秒)", DefaultValue = "0")]
    public long PublishedTime { get; set; }

    /// <summary>
    /// 撤回时间（Unix 秒，UTC）。未撤回时为 0；撤回后写入时间戳，不可清零。
    /// </summary>
    [SugarColumn(ColumnDescription = "撤回时间(Unix秒)", DefaultValue = "0")]
    public long RevokedTime { get; set; }

    /// <summary>
    /// 发布操作人（Admin 账号名）。审计字段。
    /// </summary>
    [SugarColumn(ColumnDescription = "发布操作人", Length = EntityStringLength.Length256, IsNullable = true)]
    public string PublishOperator { get; set; } = null!;

    /// <summary>
    /// 撤回操作人（Admin 账号名）。未撤回时为 null。
    /// </summary>
    [SugarColumn(ColumnDescription = "撤回操作人", Length = EntityStringLength.Length256, IsNullable = true)]
    public string RevokeOperator { get; set; } = null!;

    /// <summary>
    /// 最近一次 Server 响应业务码（<c>MailCampaignServerCode</c>）。用于排障与失败重试判定。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近Server业务码", DefaultValue = "0")]
    public MailCampaignServerCode LastServerCode { get; set; }

    /// <summary>
    /// 最近一次 Server 响应提示信息。用于排障与失败原因展示。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近Server消息", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string LastServerMessage { get; set; } = null!;

    /// <summary>
    /// 备注（Admin 自由文本）。
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Remark { get; set; } = null!;
}