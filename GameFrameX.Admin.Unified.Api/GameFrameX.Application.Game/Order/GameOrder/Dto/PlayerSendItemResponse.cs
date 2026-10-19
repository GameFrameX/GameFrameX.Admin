namespace GameFrameX.Application.Game.Order.GameOrder.Dto;

/// <summary>
/// 发送道具响应
/// </summary>
public sealed class PlayerSendItemResponse
{
    /// <summary>
    /// 道具列表
    /// </summary>
    public Dictionary<int, long> Items { get; set; } = new();
}