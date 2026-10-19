// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Client.Api.GrafanaLoki;

/// <summary>
/// Grafana Loki数据流项，表示单个数据流及其相关日志值
/// </summary>
public sealed class GrafanaLokiAnalyticsDataStreamsItem
{
    /// <summary>
    /// 数据流信息，包含日志的元数据
    /// </summary>
    [JsonPropertyName("stream")]
    public Dictionary<string, string> Stream { get; set; } = new();

    /// <summary>
    /// 日志数据值列表，每个内部列表包含时间戳和日志内容
    /// </summary>
    [JsonPropertyName("values")]
    public List<List<string>> Values { get; set; } = new();
}