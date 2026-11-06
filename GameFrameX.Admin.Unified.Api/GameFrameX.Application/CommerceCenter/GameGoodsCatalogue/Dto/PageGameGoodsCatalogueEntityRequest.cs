namespace GameFrameX.Application.CommerceCenter.GameGoodsCatalogue.Dto;

/// <summary>
/// 游戏道具分类分页查询输入参数
/// </summary>
public class PageGameGoodsCatalogueEntityRequest : BasePageRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;
}