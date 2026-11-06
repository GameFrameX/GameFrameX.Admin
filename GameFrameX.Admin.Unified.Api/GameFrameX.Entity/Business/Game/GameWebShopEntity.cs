using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Business.Game;

/// <summary>
/// 游戏邮件
/// </summary>
[SugarTable(null, "网页商城")]
[BusinessTable]
[IncrementTable]
public class GameWebShopEntity : EntityBasicSelect
{

    /// <summary>
    /// 商品图
    /// </summary>
    [SugarColumn(ColumnDescription = "商品图")]
    public string Logo { get; set; } = null!;

    /// <summary>
    /// 商品关键词
    /// </summary>
    [SugarColumn(ColumnDescription = "商品关键词")]
    public string KeyWords { get; set; } = null!;

    /// <summary>
    /// 零售价
    /// </summary>
    [SugarColumn(ColumnDescription = "零售价")]
    public string Price { get; set; } = null!;

    /// <summary>
    /// 实际价格
    /// </summary>
    [SugarColumn(ColumnDescription = "实际价格")]
    public string RealPrice { get; set; } = null!;

    /// <summary>
    /// 道具列表
    /// </summary>
    [SugarColumn(ColumnDescription = "道具列表", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string ItemJson { get; set; } = null!;

    /// <summary>
    /// 是否上架
    /// </summary>
    [SugarColumn(ColumnDescription = "是否上架", DefaultValue = "false")]
    public bool IsUp { get; set; }

    /// <summary>
    /// 销量
    /// </summary>
    [SugarColumn(ColumnDescription = "销量")]
    public int? Sales { get; set; }

    /// <summary>
    /// 权重
    /// </summary>
    [SugarColumn(ColumnDescription = "销量")]
    public int? Weight { get; set; }
}