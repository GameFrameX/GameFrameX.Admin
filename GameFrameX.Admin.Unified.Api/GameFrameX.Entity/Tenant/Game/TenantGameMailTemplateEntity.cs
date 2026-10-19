using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 邮件模板
/// </summary>
[SugarTable(null, "邮件模板")]
[SugarIndex("idx_tenant_game_mail_template_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameMailTemplateEntity : EntityBasicSelect, IAppScoped
{
    /// <summary>
    /// 应用Id。应用内容作用域（TenantId + AppId），必须 &gt; 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 分类Id
    /// </summary>
    [SugarColumn(ColumnDescription = "分类Id", DefaultValue = "0")]
    public long CategoryId { get; set; }

    /// <summary>
    /// 标题模板
    /// </summary>
    [SugarColumn(ColumnDescription = "标题模板", Length = EntityStringLength.Length1024)]
    public string TitleTemplate { get; set; } = null!;

    /// <summary>
    /// 副标题模板
    /// </summary>
    [SugarColumn(ColumnDescription = "副标题模板", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string SubTitleTemplate { get; set; } = null!;

    /// <summary>
    /// 内容模板（支持{{变量名}}占位符）
    /// </summary>
    [SugarColumn(ColumnDescription = "内容模板", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string BodyTemplate { get; set; } = null!;

    /// <summary>
    /// 默认附件JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "默认附件", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string DefaultAttachmentsJson { get; set; } = null!;

    /// <summary>
    /// 默认邮件类型
    /// </summary>
    [SugarColumn(ColumnDescription = "默认邮件类型", IsNullable = true)]
    public GameMailType? DefaultMailType { get; set; }

    /// <summary>
    /// 默认过期天数
    /// </summary>
    [SugarColumn(ColumnDescription = "默认过期天数", IsNullable = true)]
    public int? DefaultExpirationDays { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int Sort { get; set; }
}