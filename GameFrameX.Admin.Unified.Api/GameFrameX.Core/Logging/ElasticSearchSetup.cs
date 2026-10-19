using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using GameFrameX.Core.Service.PlatformLogging;

namespace GameFrameX.Core.Logging;

/// <summary>
/// ES 服务注册。运行时配置（启用/URI/索引）来自 <see cref="PlatformLoggingConfigProvider"/> 快照，
/// 不再依赖静态 Logging.json。客户端以单例工厂延迟构造：首次解析（ES Writer 构造）时读取快照。
/// </summary>
public static class ElasticSearchSetup
{
    private static readonly Uri FallbackUri = new("http://127.0.0.1:9200");

    /// <summary>
    /// 注册 ES 客户端。快照启用且配置了 URI 时按真实地址构造；否则使用回环占位（不参与写入，Writer 由快照开关门控）。
    /// </summary>
    public static void AddElasticSearch(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var elasticSettings = sp.GetRequiredService<PlatformLoggingConfigProvider>().GetSnapshot().ElasticSearch;
            var uris = ResolveServerUris(elasticSettings);
            var connectionPool = new StaticNodePool(uris);
            var defaultIndex = string.IsNullOrWhiteSpace(elasticSettings.DefaultIndex) ? "adminnet" : elasticSettings.DefaultIndex;
            var settings = new ElasticsearchClientSettings(connectionPool)
                .DefaultIndex(defaultIndex);
            return new ElasticsearchClient(settings);
        });
    }

    private static List<Uri> ResolveServerUris(PlatformLoggingElasticSearchSettings elasticSettings)
    {
        if (!elasticSettings.IsEnabled || elasticSettings.ServerUris == null || elasticSettings.ServerUris.Count == 0)
        {
            return new List<Uri> { FallbackUri, };
        }

        var uris = new List<Uri>(elasticSettings.ServerUris.Count);
        foreach (var raw in elasticSettings.ServerUris)
        {
            if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
            {
                uris.Add(uri);
            }
        }

        return uris.Count == 0 ? new List<Uri> { FallbackUri, } : uris;
    }
}