using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏邮件
/// </summary>
[SugarTable(null, "游戏邮件")]
[SugarIndex("idx_tenant_game_mail_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameMailEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Admin 群发由后台当前应用上下文写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服ID
    /// </summary>
    [SugarColumn(ColumnDescription = "区服ID", DefaultValue = "0")]
    public long AreaId { get; set; }

    /// <summary>
    /// 接收者的唯一标识符。
    /// </summary>
    [SugarColumn(ColumnDescription = "玩家ID列表", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string PlayerIdListJson { get; set; } = null!;

    /// <summary>
    /// 是否发全部用户。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否发全部用户", DefaultValue = "false")]
    public bool IsAllPost { get; set; } = true;

    /// <summary>
    /// 邮件的标题或主题。
    /// </summary>
    [SugarColumn(ColumnDescription = "标题")]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 副标题。
    /// </summary>
    [SugarColumn(ColumnDescription = "副标题")]
    public string SubTitle { get; set; } = null!;

    /// <summary>
    /// 邮件的详细信息内容。
    /// </summary>
    [SugarColumn(ColumnDescription = "详细内容", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Body { get; set; } = null!;

    /// <summary>
    /// 附件列表
    /// </summary>
    [SugarColumn(ColumnDescription = "附件列表", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string AttachmentsJson { get; set; } = null!;

    /// <summary>
    /// 邮件状态，包括已读、未读、删除等件。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件状态", DefaultValue = "0")]
    public GameMailStatus Status { get; set; }

    /// <summary>
    /// 邮件的过期日期。
    /// </summary>
    [SugarColumn(IsNullable = true, ColumnDescription = "邮件的过期日期")]
    public long ExpirationTime { get; set; }

    /// <summary>
    /// 邮件类型。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件类型", DefaultValue = "0")]
    public GameMailType MailType { get; set; }

    /// <summary>
    /// 邮件是否本地化
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件是否本地话")]
    public bool? IsTranslate { get; set; }
}