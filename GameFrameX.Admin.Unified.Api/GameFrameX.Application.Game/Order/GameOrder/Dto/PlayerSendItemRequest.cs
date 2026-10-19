namespace GameFrameX.Application.Game.Order.GameOrder.Dto;

/// <summary>
/// 发送道具请求
/// </summary>
public sealed class PlayerSendItemRequest
{
    /// <summary>
    /// 账号ID
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// 玩家ID
    /// </summary>
    public long PlayerId { get; set; }

    /// <summary>
    /// 是否是充值道具
    /// </summary>
    public bool IsRecharge { get; set; }

    /// <summary>
    /// 商品ID,只有在IsRecharge为true时生效
    /// </summary>
    public long ProductId { get; set; }

    /// <summary>
    /// 道具列表
    /// </summary>
    public List<PropertyItemResponse> Items { get; set; } = new();

    /// <summary>
    /// 金额,只有在IsRecharge为true时生效
    /// </summary>
    public uint Amount { get; set; }

    /// <summary>
    /// 渠道
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// 支付渠道
    /// </summary>
    public string PayChannel { get; set; } = string.Empty;

    /// <summary>
    /// 平台
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// 是否是沙箱环境
    /// </summary>
    public bool IsSandbox { get; set; }
}