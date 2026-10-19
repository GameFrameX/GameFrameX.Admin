namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// 交易详情内的 Grant 明细行（Item/道具 或 Currency/货币、数量、前后值）。
/// </summary>
public class OnlineAssetTransactionGrantItemResponse
{
    /// <summary>
    /// 货币或道具类型。
    /// </summary>
    public string ItemOrCurrencyType { get; set; } = string.Empty;

    /// <summary>
    /// 数量（发放为正、撤销为负）。
    /// </summary>
    public long Quantity { get; set; }

    /// <summary>
    /// 变更前值。
    /// </summary>
    public long BeforeValue { get; set; }

    /// <summary>
    /// 变更后值。
    /// </summary>
    public long AfterValue { get; set; }
}
