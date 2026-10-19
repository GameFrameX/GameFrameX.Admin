using System.Text.Json.Serialization;

namespace GameFrameX.Client.Api.Integrations.Advertisement.Dto;

/// <summary>
/// Google AdMob广告回调输入参数
/// </summary>
public sealed class AdMobCallbackRequest
{
    /// <summary>
    /// 广告单元ID
    /// </summary>
    [JsonPropertyName("ad_unit")]
    [FromQuery(Name = "ad_unit")]
    public string AdUnit { get; set; } = string.Empty;

    /// <summary>
    /// 广告网络ID
    /// </summary>
    [JsonPropertyName("ad_network")]
    [FromQuery(Name = "ad_network")]
    public string AdNetwork { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID
    /// </summary>
    [JsonPropertyName("user_id")]
    [FromQuery(Name = "user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 奖励类型
    /// </summary>
    [JsonPropertyName("reward_item")]
    [FromQuery(Name = "reward_item")]
    public string RewardItem { get; set; } = string.Empty;

    /// <summary>
    /// 奖励数量
    /// </summary>
    [JsonPropertyName("reward_amount")]
    [FromQuery(Name = "reward_amount")]
    public string RewardAmount { get; set; } = string.Empty;

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    [FromQuery(Name = "timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>
    /// 交易ID
    /// </summary>
    [JsonPropertyName("transaction_id")]
    [FromQuery(Name = "transaction_id")]
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>
    /// 签名
    /// </summary>
    [JsonPropertyName("signature")]
    [FromQuery(Name = "signature")]
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// 键ID
    /// </summary>
    [JsonPropertyName("key_id")]
    [FromQuery(Name = "key_id")]
    public long KeyId { get; set; }

    /// <summary>
    /// 自定义数据
    /// </summary>
    [JsonPropertyName("custom_data")]
    [FromQuery(Name = "custom_data")]
    public string CustomData { get; set; } = string.Empty;
}