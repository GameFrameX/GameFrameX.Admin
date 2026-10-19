using GameFrameX.Entity.Routing;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.System;

/// <summary>
/// 系统动态插件表
/// </summary>
[SugarTable(null, "系统动态插件表")]
[PlatformTable]
[IncrementTable]
public class SystemPluginEntity : EntityTenantBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>
    /// C#代码
    /// </summary>
    [SugarColumn(ColumnDescription = "C#代码", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public virtual string CsharpCode { get; set; } = string.Empty;

    /// <summary>
    /// 程序集名称
    /// </summary>
    [SugarColumn(ColumnDescription = "程序集名称", Length = EntityStringLength.Length1024)]
    public string? AssemblyName { get; set; }

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
}