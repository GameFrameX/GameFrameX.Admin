namespace GameFrameX.Application.Game.Goods.GameGoodsCatalogue.Dto;

/// <summary>
/// 游戏道具分类实体基类 - 包含 AddInput 和 UpdateInput 的公共字段
/// </summary>
public abstract class TenantGameGoodsCatalogueEntityBaseRequest : BaseUpdateRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; } = null!;
}