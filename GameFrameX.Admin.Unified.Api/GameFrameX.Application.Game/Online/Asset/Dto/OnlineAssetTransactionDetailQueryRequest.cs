namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// Online 交易详情查询请求体（Admin → Online 线缆契约；mock 先行，联调以 vault 详表回写）。
/// <para>Admin 侧详情端点是 GET（C257 读 GET），Online 侧查询经 POST 通道携带 body（<c>GetAsync</c> 无业务载荷位）。</para>
/// </summary>
public class OnlineAssetTransactionDetailQueryRequest
{
    /// <summary>
    /// 交易 Id。
    /// </summary>
    public string TransactionId { get; set; } = null!;
}
