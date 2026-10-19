// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Client.Api.GrafanaLoki;

public static class GrafanaLokiAnalyticsManager
{
    private static readonly HttpClient LokiHttpClient = new();
    private static readonly Dictionary<string, object> LokiPublicProperties = new();
    private static readonly Dictionary<string, string> LokiLabels = new();
    private static System.Timers.Timer _timer = null!;
    private static int _sendInterval = 5000; // 默认5秒间隔
    private static int _batchSize = 100; // 默认30条数据
    private static string _lokiUrl = "https://lokipush.grafana.zilancreative.com/loki/api/v1/push"; // 默认Loki URL
    private static readonly object LockObject = new();

    private static ILogGrafanaLokiStorage _logGrafanaLokiStorage = null!;

    static GrafanaLokiAnalyticsManager()
    {
        InitializeTimer();
    }

    private static void InitializeTimer()
    {
        _timer?.Dispose();
        _timer = new System.Timers.Timer(_sendInterval);
        _timer.Elapsed += async (sender, e) => { await Task.Run(async () => { await SendEventsToLokiAsync(); }); };
        _timer.AutoReset = true;
        _timer.Enabled = true;
        LokiHttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        lock (LockObject)
        {
            _logGrafanaLokiStorage = new SqliteLogGrafanaLokiStorage();
        }

        LokiPublicProperties.Add("app_name", AppDomain.CurrentDomain.FriendlyName);
        LokiLabels.Add("app_name", AppDomain.CurrentDomain.FriendlyName);
    }

    public static void SetPublicProperties(string key, object value)
    {
        LokiPublicProperties[key] = value;
    }

    /// <summary>
    /// 配置发送参数
    /// </summary>
    /// <param name="sendIntervalSeconds">发送间隔（秒）</param>
    /// <param name="batchSize">每批发送数量</param>
    /// <param name="lokiUrl">Loki服务器URL</param>
    public static void Configure(int sendIntervalSeconds = 5, int batchSize = 100, string? lokiUrl = null)
    {
        _sendInterval = sendIntervalSeconds * 1000;
        _batchSize = batchSize;
        if (!string.IsNullOrEmpty(lokiUrl))
        {
            _lokiUrl = lokiUrl;
        }

        InitializeTimer();
    }

    /// <summary>
    /// 设置Loki服务器URL
    /// </summary>
    /// <param name="lokiUrl">Loki服务器URL</param>
    public static void SetLokiUrl(string lokiUrl)
    {
        if (!string.IsNullOrEmpty(lokiUrl))
        {
            _lokiUrl = lokiUrl;
        }
    }

    /// <summary>
    /// 获取当前配置的Loki URL
    /// </summary>
    /// <returns>Loki URL</returns>
    public static string GetLokiUrl()
    {
        return _lokiUrl;
    }

    private static async Task SendEventsToLokiAsync()
    {
        var eventsToSend = new List<GrafanaLokiAnalyticsEntry>();

        lock (LockObject)
        {
            var batchToSend = _logGrafanaLokiStorage.LoadPendingLogs(_batchSize);
            // 持续发送直到队列为空或少于批量大小
            eventsToSend.AddRange(batchToSend);
        }

        if (eventsToSend.Count == 0)
        {
            return;
        }

        await SendBatchToLoki(eventsToSend);
    }

    /// <summary>
    /// 构建Loki格式的JSON负载
    /// </summary>
    /// <param name="logs">日志条目列表</param>
    /// <param name="labels">标签字典</param>
    /// <returns>JSON字符串</returns>
    private static string BuildPayload(List<GrafanaLokiAnalyticsEntry> logs, Dictionary<string, string> labels)
    {
        if (logs == null || logs.Count == 0)
        {
            return string.Empty;
        }

        var grafanaLokiAnalyticsData = new GrafanaLokiAnalyticsData();
        var grafanaLokiAnalyticsDataStreamsItem = new GrafanaLokiAnalyticsDataStreamsItem();
        grafanaLokiAnalyticsData.Streams.Add(grafanaLokiAnalyticsDataStreamsItem);

        // 添加标签
        foreach (var label in labels)
        {
            grafanaLokiAnalyticsDataStreamsItem.Stream[label.Key] = label.Value;
        }

        // 添加日志条目
        foreach (var log in logs)
        {
            var values = new List<string> { log.TimestampNs.ToString(), log.GetEventDataJson(), };
            grafanaLokiAnalyticsDataStreamsItem.Values.Add(values);
        }

        return JsonHelper.Serialize(grafanaLokiAnalyticsData);
    }

    private static async Task SendBatchToLoki(List<GrafanaLokiAnalyticsEntry> events)
    {
        var payload = BuildPayload(events, LokiLabels);

        var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var response = await LokiHttpClient.PostAsync(_lokiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[GameAna] 成功发送 {events.Count} 条事件到 Loki ({_lokiUrl})");
                _logGrafanaLokiStorage.ClearProcessedLogs(events.Select(evt => evt.Id).ToList());
            }
            else
            {
                Console.WriteLine($"[GameAna] 发送到 Loki 失败，状态码: {response.StatusCode}, URL: {_lokiUrl}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GameAna] 发送到 Loki 异常: {ex.Message}, URL: {_lokiUrl}");
        }
    }

    /// <summary>
    /// 添加游戏事件
    /// </summary>
    /// <param name="eventName">事件名</param>
    /// <param name="parameters">事件参数</param>
    public static void Event(string eventName, Dictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrEmpty(eventName))
        {
            return;
        }

        var gameEvent = new GrafanaLokiAnalyticsEntry(eventName, parameters ?? new Dictionary<string, object>(), LokiPublicProperties);

        lock (LockObject)
        {
            _logGrafanaLokiStorage.SaveLog(gameEvent);
        }
    }

    /// <summary>
    /// 停止定时器
    /// </summary>
    public static void Stop()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
}