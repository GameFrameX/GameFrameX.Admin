namespace GameFrameX.Application.CommerceCenter.GameGoods.Dto;

/// <summary>
/// 游戏资源分页查询输入参数
/// </summary>
public class PageTenantGameGoodsEntityRequest : BasePageRequest
{
    /// <summary>
    /// 配置ID
    /// </summary>
    public int? ConfigId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;
}