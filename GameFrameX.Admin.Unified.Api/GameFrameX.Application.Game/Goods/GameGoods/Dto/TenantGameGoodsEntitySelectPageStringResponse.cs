namespace GameFrameX.Application.Game.Goods.GameGoods.Dto;

/// <summary>
/// 游戏资源选择器输出参数
/// </summary>
public class TenantGameGoodsEntitySelectPageStringResponse : BaseSelectPageStringResponse
{
    /// <summary>
    /// 应用Id
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 星级
    /// </summary>
    public bool? Star { get; set; }

    /// <summary>
    /// 物品目录Id
    /// </summary>
    public long? ClassifyId { get; set; }

    /// <summary>
    /// 物品目录名称
    /// </summary>
    public string ClassifyIdName { get; set; } = null!;
}