namespace GameFrameX.Client.Api.Services.PayVerification;

/// <summary>
/// 验真结果（C11 支付域；具名类替代元组）
/// </summary>
public sealed class GamePayVerifyResponse
{
    /// <summary>
    /// 结果三态
    /// </summary>
    public GamePayVerifyState State { get; set; }

    /// <summary>
    /// 渠道交易号（Apple=TransactionId / Google=orderId 或 token 摘要；成功态回填订单）
    /// </summary>
    public string ProviderTransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 渠道商品 ID
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// 验真结果留档 JSON
    /// </summary>
    public string ReceiptJson { get; set; } = string.Empty;

    /// <summary>
    /// 附加说明（拒绝原因 / pending 原因）
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 成功结果
    /// </summary>
    /// <param name="providerTransactionId">渠道交易号</param>
    /// <param name="productId">渠道商品 ID</param>
    /// <param name="receiptJson">验真结果留档 JSON</param>
    /// <returns>成功态结果</returns>
    public static GamePayVerifyResponse Succeeded(string providerTransactionId, string productId, string receiptJson)
    {
        return new GamePayVerifyResponse
        {
            State = GamePayVerifyState.Succeeded,
            ProviderTransactionId = providerTransactionId,
            ProductId = productId,
            ReceiptJson = receiptJson,
        };
    }

    /// <summary>
    /// pending / 可疑结果
    /// </summary>
    /// <param name="message">原因说明</param>
    /// <returns>pending 态结果</returns>
    public static GamePayVerifyResponse Pending(string message)
    {
        return new GamePayVerifyResponse { State = GamePayVerifyState.Pending, Message = message, };
    }

    /// <summary>
    /// 拒绝结果
    /// </summary>
    /// <param name="message">拒绝原因</param>
    /// <returns>拒绝态结果</returns>
    public static GamePayVerifyResponse Rejected(string message)
    {
        return new GamePayVerifyResponse { State = GamePayVerifyState.Rejected, Message = message, };
    }
}