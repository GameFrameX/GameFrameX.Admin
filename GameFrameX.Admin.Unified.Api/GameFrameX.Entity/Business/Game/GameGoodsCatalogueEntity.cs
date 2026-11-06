using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Business.Game;

/// <summary>
/// 游戏资源分类
/// </summary>
[SugarTable(null, "游戏资源分类")]
[BusinessTable]
[IncrementTable]
public class GameGoodsCatalogueEntity : EntityBasicSelect
{
}