namespace GameFrameX.Application.Game.Goods.GameGoods.Dto;

/// <summary>
/// 游戏资源分页查询输入参数
/// </summary>
public class PageTenantGameGoodsEntityRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id（可选过滤，按当前应用筛选游戏资源）
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 配置ID
    /// </summary>
    public int? ConfigId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;
}