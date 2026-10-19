// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;
using SQLite;

namespace GameFrameX.Client.Api.GrafanaLoki;

/// <summary>
/// 日志条目
/// </summary>
[Serializable]
public sealed class GrafanaLokiAnalyticsEntry
{
    /// <summary>
    /// 唯一ID
    /// </summary>
    [PrimaryKey]
    public string Id { get; set; } = null!;

    /// <summary>
    /// 创建时间
    /// </summary>
    [Indexed]
    public long CreateTime { get; set; }

    /// <summary>
    /// 事件名称
    /// </summary>
    public string EventName { get; set; } = null!;

    /// <summary>
    /// 事件数据
    /// </summary>
    public string EventDataJson { get; set; } = null!;

    /// <summary>
    /// 时间戳（纳秒）
    /// </summary>
    public long TimestampNs { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="eventData">事件数据</param>
    /// <param name="publicProperties">公共属性</param>
    public GrafanaLokiAnalyticsEntry(string eventName, Dictionary<string, object> eventData, Dictionary<string, object>? publicProperties = null)
    {
        EventName = eventName;
        // 添加事件数据
        if (publicProperties == null)
        {
            publicProperties = new Dictionary<string, object>();
        }

        if (eventData == null)
        {
            eventData = new Dictionary<string, object>();
        }

        var result = publicProperties;

        foreach (var kvp in eventData)
        {
            result[kvp.Key] = kvp.Value;
        }

        // 设置事件时间
        result["event_time"] = DateTime.UtcNow;
        EventDataJson = JsonHelper.Serialize(result);

        CreateTime = TimerHelper.UnixTimeSeconds();

        // 设置时间戳（转换为纳秒）
        TimestampNs = TimerHelper.UnixTimeMilliseconds() * 1000000;

        // 生成唯一ID
        Id = Guid.NewGuid().ToString("N");
    }

    public GrafanaLokiAnalyticsEntry()
    {
    }

    /// <summary>
    /// 获取JSON格式的事件数据
    /// </summary>
    /// <returns>JSON字符串</returns>
    public string GetEventDataJson()
    {
        return JsonHelper.Serialize(new EventDataWrapper(EventName, EventDataJson));
    }

    /// <summary>
    /// 事件数据包装器（用于JSON序列化）
    /// </summary>
    [Serializable]
    public sealed class EventDataWrapper
    {
        [JsonPropertyName("event_name")]
        public string EventName { get; set; }
        [JsonPropertyName("event_data")]
        public string EventData { get; set; }

        public EventDataWrapper(string eventName, string eventData)
        {
            EventName = eventName;
            EventData = eventData;
        }
    }
}