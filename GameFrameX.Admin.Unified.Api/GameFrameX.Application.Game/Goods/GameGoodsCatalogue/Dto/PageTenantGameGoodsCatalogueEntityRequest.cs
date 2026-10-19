namespace GameFrameX.Application.Game.Goods.GameGoodsCatalogue.Dto;

/// <summary>
/// 游戏道具分类分页查询输入参数
/// </summary>
public class PageTenantGameGoodsCatalogueEntityRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id（可选过滤，按当前应用筛选游戏资源分类）
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;
}