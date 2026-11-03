using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏资源
/// </summary>
[SugarTable(null, "游戏资源")]
[TenantTable]
[IncrementTable]
public class TenantGameGoodsEntity : EntityBasicSelect
{

    /// <summary>
    /// 配置ID
    ///</summary>
    [SugarColumn(ColumnDescription = "配置ID", DefaultValue = "0")]
    public int ConfigId { get; set; }

    /// <summary>
    ///类别id
    ///</summary>
    [SugarColumn(ColumnDescription = "类别id", DefaultValue = "0")]
    public long ClassifyId { get; set; }

    /// <summary>
    /// 是否支持星级
    /// </summary>
    [SugarColumn(ColumnDescription = "星级")]
    public bool? Star { get; set; }
}