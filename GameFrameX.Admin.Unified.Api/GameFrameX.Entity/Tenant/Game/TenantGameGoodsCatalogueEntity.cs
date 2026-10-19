using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏资源分类
/// </summary>
[SugarTable(null, "游戏资源分类")]
[SugarIndex("idx_tenant_game_goods_catalogue_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameGoodsCatalogueEntity : EntityBasicSelect, IAppScoped
{
    /// <summary>
    /// 应用Id。运营内容应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Admin 后台当前应用上下文写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }
}