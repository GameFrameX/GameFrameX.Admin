using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏资源分类
/// </summary>
[SugarTable(null, "游戏资源分类")]
[TenantTable]
[IncrementTable]
public class TenantGameGoodsCatalogueEntity : EntityBasicSelect
{
}