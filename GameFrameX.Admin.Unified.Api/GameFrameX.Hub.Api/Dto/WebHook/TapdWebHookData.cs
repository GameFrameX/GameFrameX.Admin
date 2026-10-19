using System.Text.Json.Serialization;

namespace GameFrameX.Hub.Api.Dto.WebHook;

/// <summary>
/// Tapd WebHook 请求过来的数据模型
/// </summary>
public sealed class TapdWebHookData
{
    public sealed class WebHookDataEvent
    {
        public class WebHookDataEventObjectInfo
        {
            /// <summary>
            /// 创建人
            /// </summary>
            [JsonPropertyName("current_owner")]
            public string CurrentOwner { get; set; } = null!;

            /// <summary>
            /// 需求标题
            /// </summary>
            [JsonPropertyName("name")]
            public string Name { get; set; } = null!;

            /// <summary>
            /// BUG标题
            /// </summary>
            [JsonPropertyName("title")]
            public string Title { get; set; } = null!;
        }

        /// <summary>
        /// 事件数据
        /// </summary>
        [JsonPropertyName("object_info")]
        public WebHookDataEventObjectInfo ObjectInfo { get; set; } = null!;

        /// <summary>
        /// 工作空间ID或项目ID
        /// </summary>
        [JsonPropertyName("workspace_id")]
        public string WorkspaceId { get; set; } = null!;

        /// <summary>
        /// 用户名
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = null!;

        /// <summary>
        /// 类型
        /// </summary>
        [JsonPropertyName("object_type")]
        public string ObjectType { get; set; } = null!;

        /// <summary>
        /// ID
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        /// <summary>
        /// 事件时间戳
        /// </summary>
        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; }

        /// <summary>
        /// 事件类型Key
        /// </summary>
        [JsonPropertyName("event_key")]
        public string EventKey { get; set; } = null!;

        /// <summary>
        /// 来源
        /// </summary>
        [JsonPropertyName("from")]
        public string From { get; set; } = null!;

        /// <summary>
        /// 事件ID
        /// </summary>
        [JsonPropertyName("event_id")]
        public string EventId { get; set; } = null!;
    }

    /// <summary>
    /// 项目ID
    /// </summary>
    [JsonPropertyName("workspace_id")]
    public string WorkspaceId { get; set; } = null!;

    /// <summary>
    /// 事件数据
    /// </summary>
    [JsonPropertyName("event")]
    public WebHookDataEvent Event { get; set; } = null!;
}