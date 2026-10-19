namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// 人工发放明细行（货币或道具 + 数量）。
/// </summary>
public class OnlineAssetGrantItemRequest
{
    /// <summary>
    /// 货币或道具类型（如 gold / diamond / item:1001）。
    /// </summary>
    [Required(ErrorMessage = "货币或道具类型不能为空")]
    [MaxLength(EntityStringLength.Length256)]
    public string ItemOrCurrencyType { get; set; } = null!;

    /// <summary>
    /// 数量（必须大于 0）。
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "发放数量必须大于0")]
    public long Quantity { get; set; }
}
