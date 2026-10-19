using System.Text.Json.Serialization;

namespace GameFrameX.Client.Api.Integrations.Advertisement.Dto;

/// <summary>
/// Unity Ads 服务端回调输入参数。
/// <para>仅作 Swagger / 兼容占位；权威参数解析在 <c>GameAdvertisementRewardService</c> 内按 <c>UnityAdsAdvertisementCallbackOption</c> 的 <c>*Param</c> 字段从 query 动态取（schema-driven）。</para>
/// </summary>
public sealed class AdvertisementUnityAdsCallbackRequest
{
    /// <summary>
    /// 用户/会话 Id（Unity Ads <c>sid</c>，默认参数名）。
    /// </summary>
    [JsonPropertyName("sid")]
    [FromQuery(Name = "sid")]
    public string Sid { get; set; } = null!;

    /// <summary>
    /// 订单 Id（Unity Ads <c>oid</c>，默认参数名，用于幂等去重）。
    /// </summary>
    [JsonPropertyName("oid")]
    [FromQuery(Name = "oid")]
    public string Oid { get; set; } = null!;

    /// <summary>
    /// HMAC 签名（Unity Ads <c>hmac</c>，默认参数名）。
    /// </summary>
    [JsonPropertyName("hmac")]
    [FromQuery(Name = "hmac")]
    public string Hmac { get; set; } = null!;
}