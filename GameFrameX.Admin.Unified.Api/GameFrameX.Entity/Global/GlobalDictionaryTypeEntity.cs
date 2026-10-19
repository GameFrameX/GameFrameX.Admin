using GameFrameX.Entity.Base;
using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.Global;

/// <summary>
/// 系统字典类型表
/// </summary>
[SugarTable(null, "系统字典类型表")]
[GlobalTable]
[IncrementTable]
public class GlobalDictionaryTypeEntity : EntityBasic
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// 编码
    /// </summary>
    [SugarColumn(ColumnDescription = "编码", Length = EntityStringLength.Length256)]
    public virtual string Code { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length512)]
    public string? Remark { get; set; }

    /// <summary>
    /// 字典值集合
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(GlobalDictionaryDataEntity.DictionaryTypeId))]
    public List<GlobalDictionaryDataEntity> Children { get; set; } = new();
}