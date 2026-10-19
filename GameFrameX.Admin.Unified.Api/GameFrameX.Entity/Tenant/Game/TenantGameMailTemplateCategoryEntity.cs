using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 邮件模板分类
/// </summary>
[SugarTable(null, "邮件模板分类")]
[SugarIndex("idx_tenant_game_mail_template_category_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameMailTemplateCategoryEntity : EntityBasicSelect, IAppScoped
{
    /// <summary>
    /// 应用Id。应用内容作用域（TenantId + AppId），必须 &gt; 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [SugarColumn(ColumnDescription = "图标", Length = EntityStringLength.Length512, IsNullable = true)]
    public string Icon { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int Sort { get; set; }
}