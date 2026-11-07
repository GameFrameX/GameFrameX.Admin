using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Business.Game;

/// <summary>
/// 游戏资源
/// </summary>
[SugarTable(null, "商品")]
[BusinessTable]
[IncrementTable]
public class GameCommodityEntity : EntityBasic
{

    /// <summary>
    ///Status
    ///</summary>
    [SugarColumn(ColumnDescription = "状态")]
    public string Status { get; set; } = null!;

    /// <summary>
    /// 类别Id。引用 GameGoodsCatalogueEntity.Id（由 string 统一为 long，对齐 GameGoodsEntity.ClassifyId 模式）。
    ///</summary>
    [SugarColumn(ColumnDescription = "类别Id", DefaultValue = "0")]
    public long CategoryId { get; set; }

    /// <summary>
    ///Cost
    ///</summary>
    [SugarColumn(ColumnDescription = "多少rmb", DefaultValue = "false")]
    public bool Cost { get; set; }

    /// <summary>
    ///Number
    ///</summary>
    [SugarColumn(ColumnDescription = " 数量", DefaultValue = "0")]
    public long Number { get; set; }
}
