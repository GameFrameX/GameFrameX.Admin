namespace GameFrameX.Client.Api.GameUserPay.Dto;

/// <summary>
/// 支付订单查询请求（C11 支付域 Query；OrderId / ProductId 二选一：按单号查单条 / 按用户+商品查最新一条）
/// </summary>
public class GameUserPayQueryRequest
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 对外订单号（按单号形态）
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    /// 渠道商品 ID（按商品形态：该 UserId + ProductId 最新一条）
    /// </summary>
    public string? ProductId { get; set; }
}