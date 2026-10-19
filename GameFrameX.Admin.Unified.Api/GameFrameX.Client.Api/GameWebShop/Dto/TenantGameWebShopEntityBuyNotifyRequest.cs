using System.ComponentModel.DataAnnotations;

using System.Text.Json.Serialization;

namespace GameFrameX.Client.Api.GameWebShop.Dto;

/// <summary>
/// 购买一个商品
/// </summary>
public sealed class TenantGameWebShopEntityBuyNotifyRequest
{
    [JsonPropertyName("pid")]
    [FromQuery(Name = "pid")]
    [Required(ErrorMessage = "")] public int Pid { get; set; }

    [JsonPropertyName("trade_no")]
    [FromQuery(Name = "trade_no")]
    [Required(ErrorMessage = "")] public string TradeNo { get; set; } = null!;

    [JsonPropertyName("out_trade_no")]
    [FromQuery(Name = "out_trade_no")]
    [Required(ErrorMessage = "")] public string OutTradeNo { get; set; } = null!;

    [JsonPropertyName("type")]
    [FromQuery(Name = "type")]
    [Required(ErrorMessage = "")] public string Type { get; set; } = null!;

    [JsonPropertyName("name")]
    [FromQuery(Name = "name")]
    [Required(ErrorMessage = "")] public string Name { get; set; } = null!;

    [JsonPropertyName("money")]
    [FromQuery(Name = "money")]
    [Required(ErrorMessage = "")] public decimal Money { get; set; }

    [JsonPropertyName("trade_status")]
    [FromQuery(Name = "trade_status")]
    [Required(ErrorMessage = "")] public string TradeStatus { get; set; } = null!;

    [JsonPropertyName("param")]
    [FromQuery(Name = "param")]
    [Required(ErrorMessage = "")] public string Param { get; set; } = null!; // 原始 param 字符串

    [JsonPropertyName("sign")]
    [FromQuery(Name = "sign")]
    [Required(ErrorMessage = "")] public string Sign { get; set; } = null!;

    [JsonPropertyName("sign_type")]
    [FromQuery(Name = "sign_type")]
    [Required(ErrorMessage = "")] public string SignType { get; set; } = null!;
}