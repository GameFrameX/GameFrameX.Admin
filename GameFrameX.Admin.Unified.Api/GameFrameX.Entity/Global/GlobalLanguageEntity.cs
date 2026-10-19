using GameFrameX.Entity.Base;
using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.Global;

[SugarTable(null, "全局语言配置表")]
[GlobalTable]
[IncrementTable]
public class GlobalLanguageEntity : EntityBasicSelect
{
    /// <summary>
    /// 语言代码，ISO 3166 双字母国家/地区代码
    /// </summary>
    [SugarColumn(ColumnDescription = "语言代码，ISO 3166 双字母国家/地区代码")]
    public string? Code { get; set; }

    /// <summary>
    /// 是否为默认
    /// </summary>
    [SugarColumn(ColumnDescription = "是否默认")]
    public bool? IsDefault { get; set; }
}