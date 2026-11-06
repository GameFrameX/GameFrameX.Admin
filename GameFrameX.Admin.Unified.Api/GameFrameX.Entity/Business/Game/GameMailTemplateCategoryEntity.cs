using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Business.Game;

/// <summary>
/// 邮件模板分类
/// </summary>
[SugarTable(null, "邮件模板分类")]
[BusinessTable]
[IncrementTable]
public class GameMailTemplateCategoryEntity : EntityBasicSelect
{

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