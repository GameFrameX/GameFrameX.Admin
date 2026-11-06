using GameFrameX.Application.ServerCenter.Online;
using GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;

namespace GameFrameX.Application.ServerCenter.GameAreaMerge;

/// <summary>为合服配置独立超时，沿用 Online 的作用域与双层信封协议。</summary>
public sealed class GameAreaMergeClient
{
    private const int DefaultExecutionTimeoutSeconds = 1800;
    private readonly OnlineServerClient _client;
    private readonly OnlineScopeContextFactory _scopeFactory;
    public int ExecutionTimeoutSeconds { get; }

    public GameAreaMergeClient(HttpClient httpClient, IOnlineServerEndpointResolver endpointResolver,
        SqlSugarRepository<GameAreaEntity> repository, OnlineScopeContextFactory scopeFactory)
    {
        ExecutionTimeoutSeconds = httpClient.Timeout == Timeout.InfiniteTimeSpan
            ? DefaultExecutionTimeoutSeconds
            : (int)Math.Max(1, httpClient.Timeout.TotalSeconds);
        _client = new OnlineServerClient(httpClient, endpointResolver, repository);
        _scopeFactory = scopeFactory;
    }

    /// <summary>不接受 HTTP 请求取消令牌；生命周期由专属 HttpClient 超时控制。</summary>
    public async Task<GameAreaMergeReportResponse> DispatchAsync(string action, long targetServerId, string sourceServerIds)
    {
        var scope = await _scopeFactory.CreateAsync(targetServerId, $"server-merge:{sourceServerIds}");
        return await _client.PostAsync<GameAreaMergeReportResponse>(action, new GameAreaMergeDispatchRequest
        {
            TargetServerId = targetServerId,
            SourceServerIds = sourceServerIds,
        }, scope);
    }
}
