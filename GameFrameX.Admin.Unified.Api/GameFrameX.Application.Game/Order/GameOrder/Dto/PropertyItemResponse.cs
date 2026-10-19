namespace GameFrameX.Application.Game.Order.GameOrder.Dto;

/// <summary>
/// 属性对象
/// </summary>
public sealed class PropertyItemResponse
{
    /// <summary>
    /// id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public long Count { get; set; }

    /// <summary>
    /// 道具id
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// 道具星级
    /// </summary>
    public int Star { get; set; }
}