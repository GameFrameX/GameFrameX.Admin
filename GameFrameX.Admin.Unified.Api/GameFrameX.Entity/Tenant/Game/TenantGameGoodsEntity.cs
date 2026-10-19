using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏资源
/// </summary>
[SugarTable(null, "游戏资源")]
[SugarIndex("idx_tenant_game_goods_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameGoodsEntity : EntityBasicSelect, IAppScoped
{
    /// <summary>
    /// 应用Id。运营内容应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Admin 后台当前应用上下文写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

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