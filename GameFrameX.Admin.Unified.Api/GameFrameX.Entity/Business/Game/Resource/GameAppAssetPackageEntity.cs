using GameFrameX.Entity.Base;
using GameFrameX.Foundation.Orm.Entity.Filter;

namespace GameFrameX.Entity.Business.Game.Resource;

/// <summary>
/// 客户端资源包名称字典。
/// <para>
/// 资源包定义属于具体游戏应用；版本发布、客户端资源更新和应用分包均以应用为发布单元，
/// </para>
/// </summary>
[SugarTable(null, "客户端资源包")]
[BusinessTable]
[IncrementTable]
public sealed class GameAppAssetPackageEntity : EntityBasic, ISelectFilter
{

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [SugarColumn(ColumnDescription = "详细描述", Length = EntityStringLength.Length1024)]
    public string Description { get; set; } = null!;
}