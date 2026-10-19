using GameFrameX.Entity.Base;
using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.Global;

/// <summary>
/// 系统字典值表
/// </summary>
[SugarTable(null, "系统字典值表")]
[GlobalTable]
[IncrementTable]
public class GlobalDictionaryDataEntity : EntityBasic
{
    /// <summary>
    /// 字典类型Id
    /// </summary>
    [SugarColumn(ColumnDescription = "字典类型Id", DefaultValue = "0")]
    public long DictionaryTypeId { get; set; }

    /// <summary>
    /// 字典类型
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(DictionaryTypeId))]
    public GlobalDictionaryTypeEntity DictionaryType { get; set; } = null!;

    /// <summary>
    /// 值
    /// </summary>
    [SugarColumn(ColumnDescription = "值", Length = EntityStringLength.Length256)]
    public virtual string Value { get; set; } = string.Empty;

    /// <summary>
    /// 编码
    /// </summary>
    [SugarColumn(ColumnDescription = "编码", Length = EntityStringLength.Length256)]
    public virtual string Code { get; set; } = string.Empty;

    /// <summary>
    /// 显示样式-标签颜色
    /// </summary>
    [SugarColumn(ColumnDescription = "显示样式-标签颜色", Length = EntityStringLength.Length256)]
    public string? TagType { get; set; }

    /// <summary>
    /// 显示样式-Style(控制显示样式)
    /// </summary>
    [SugarColumn(ColumnDescription = "显示样式-Style", Length = EntityStringLength.Length1024)]
    public string? StyleSetting { get; set; }

    /// <summary>
    /// 显示样式-Class(控制显示样式)
    /// </summary>
    [SugarColumn(ColumnDescription = "显示样式-Class", Length = EntityStringLength.Length1024)]
    public string? ClassSetting { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Remark { get; set; }

    /// <summary>
    /// 拓展数据(保存业务功能的配置项)
    /// </summary>
    [SugarColumn(ColumnDescription = "拓展数据(保存业务功能的配置项)", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? ExtData { get; set; }
}