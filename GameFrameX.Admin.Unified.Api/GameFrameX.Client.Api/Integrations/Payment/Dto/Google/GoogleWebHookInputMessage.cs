using System.Text.Json.Serialization;

namespace GameFrameX.Client.Api.Integrations.Payment.Dto.Google;

/// <summary>
/// Google Pay WebHook 消息
/// </summary>
public sealed class GoogleWebHookInputMessage
{
    /// <summary>
    /// 数据
    /// </summary>
    [JsonPropertyName("data")]
    public string Data { get; set; } = null!;

    /// <summary>
    /// 消息ID
    /// </summary>
    public string messageId { get; set; } = null!;

    /// <summary>
    /// 消息ID
    /// </summary>
    public string message_id { get; set; } = null!;

    /// <summary>
    /// 发布时间
    /// </summary>
    public string publishTime { get; set; } = null!;

    /// <summary>
    /// 发布时间
    /// </summary>
    public string publish_time { get; set; } = null!;
}