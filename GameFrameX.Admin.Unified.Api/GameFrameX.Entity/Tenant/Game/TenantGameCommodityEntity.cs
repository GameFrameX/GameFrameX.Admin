using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏资源
/// </summary>
[SugarTable(null, "商品")]
[SugarIndex("idx_tenant_game_commodity_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameCommodityEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。运营内容应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Admin 后台当前应用上下文写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    ///Status
    ///</summary>
    [SugarColumn(ColumnDescription = "状态")]
    public string Status { get; set; } = null!;

    /// <summary>
    /// 类别Id。引用 TenantGameGoodsCatalogueEntity.Id（C391：由 string 统一为 long，对齐 TenantGameGoodsEntity.ClassifyId 模式）。
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