using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using GameFrameX.Core.Service.PlatformLogging;

namespace GameFrameX.Core.Logging;

/// <summary>
/// ES日志写入器。C171 起由 <see cref="PlatformLoggingConfigProvider"/> 快照的 <c>ElasticSearch.IsEnabled</c> 门控：
/// 保存后同 Host 内关闭 ES 日志即时生效。
/// </summary>
public class ElasticSearchLoggingWriter : IDatabaseLoggingWriter, IDisposable
{
    private readonly ElasticsearchClient _esClient;
    private readonly IServiceScope _serviceScope;
    private readonly SystemConfigService _systemConfigService; // Scoped：经独立 Scope 解析，避免 Singleton 捕获 Scoped（与 DatabaseLoggingWriter 同款模式）
    private readonly PlatformLoggingConfigProvider _loggingConfigProvider;

    public ElasticSearchLoggingWriter(ElasticsearchClient esClient,
        IServiceScopeFactory scopeFactory,
        PlatformLoggingConfigProvider loggingConfigProvider)
    {
        _esClient = esClient;
        _serviceScope = scopeFactory.CreateScope();
        _systemConfigService = _serviceScope.ServiceProvider.GetRequiredService<SystemConfigService>();
        _loggingConfigProvider = loggingConfigProvider;
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }

    public async Task WriteAsync(IReadOnlyList<LogMessage> logMsgs, bool flush)
    {
        if (!_loggingConfigProvider.GetSnapshot().ElasticSearch.IsEnabled)
        {
            return;
        }

        foreach (var logMsg in logMsgs)
        {
            var systemOpLogEnabled = await _systemConfigService.GetConfigValueAsync<bool>(CommonConst.SysOpLog);
            if (!systemOpLogEnabled)
            {
                return;
            }

            var jsonStr = logMsg.Context?.Get("loggingMonitor")?.ToString();
            if (string.IsNullOrEmpty(jsonStr))
            {
                return;
            }

            // ponytail: monitor 结构随监控管线演进，按需索引读取（actionName 过滤），
            // 不整体建模；字段语义与 DatabaseLoggingWriter 读取口径保持一致。
            var loggingMonitor = JsonNode.Parse(jsonStr);
            if (loggingMonitor == null)
            {
                return;
            }

            var actionName = loggingMonitor["actionName"]?.GetValue<string>();
            if (actionName == "userInfo" || actionName == "logout")
            {
                return;
            }

            await _esClient.IndexAsync(jsonStr, idx => idx.OpType(OpType.Create));
        }
    }
}