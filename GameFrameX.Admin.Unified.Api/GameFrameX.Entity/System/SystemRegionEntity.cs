using GameFrameX.Entity.Routing;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.System;

/// <summary>
/// 系统行政地区表
/// </summary>
[SugarTable(null, "系统行政地区表")]
[PlatformTable]
[IncrementTable]
public sealed class SystemRegionEntity : EntityBase
{
    /// <summary>
    /// 父Id
    /// </summary>
    [SugarColumn(ColumnDescription = "父Id", DefaultValue = "0")]
    public long Pid { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 简称
    /// </summary>
    [SugarColumn(ColumnDescription = "简称", Length = EntityStringLength.Length256)]
    public string? ShortName { get; set; }

    /// <summary>
    /// 组合名
    /// </summary>
    [SugarColumn(ColumnDescription = "组合名", Length = EntityStringLength.Length256)]
    public string? MergerName { get; set; }

    /// <summary>
    /// 行政代码
    /// </summary>
    [SugarColumn(ColumnDescription = "行政代码", Length = EntityStringLength.Length256)]
    public string? Code { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    [SugarColumn(ColumnDescription = "邮政编码", Length = EntityStringLength.Length256)]
    public string? ZipCode { get; set; }

    /// <summary>
    /// 区号
    /// </summary>
    [SugarColumn(ColumnDescription = "区号", Length = EntityStringLength.Length256)]
    public string? CityCode { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    [SugarColumn(ColumnDescription = "层级", DefaultValue = "0")]
    public int Level { get; set; }

    /// <summary>
    /// 拼音
    /// </summary>
    [SugarColumn(ColumnDescription = "拼音", Length = EntityStringLength.Length256)]
    public string? PinYin { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    [SugarColumn(ColumnDescription = "经度", DefaultValue = "0")]
    public float Lng { get; set; }

    /// <summary>
    /// 维度
    /// </summary>
    [SugarColumn(ColumnDescription = "维度", DefaultValue = "0")]
    public float Lat { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length256)]
    public string? Remark { get; set; }

    /// <summary>
    /// 机构子项
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<SystemRegionEntity> Children { get; set; } = new();
}