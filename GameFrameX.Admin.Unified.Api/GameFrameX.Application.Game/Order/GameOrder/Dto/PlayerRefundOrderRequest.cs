namespace GameFrameX.Application.Game.Order.GameOrder.Dto;

/// <summary>
/// 退款扣除请求
/// </summary>
public sealed class PlayerRefundOrderRequest
{
    /// <summary>
    /// 账号ID
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// 角色ID
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 是否是充值道具
    /// </summary>
    public bool IsRecharge { get; set; }

    /// <summary>
    /// 商品ID
    /// </summary>
    public long ProductId { get; set; }

    /// <summary>
    /// 支付平台订单ID
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 需要扣除的道具列表
    /// </summary>
    public List<PropertyItemResponse> Items { get; set; } = new();

    /// <summary>
    /// 金额
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

    /// <summary>
    /// 退款原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}