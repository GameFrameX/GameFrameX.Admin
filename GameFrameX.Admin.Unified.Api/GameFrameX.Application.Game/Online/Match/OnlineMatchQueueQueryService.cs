using System.ComponentModel;
using GameFrameX.Application.Game.Online.Match.Dto;
using GameFrameX.Application.Game.ServerManager.Online;

namespace GameFrameX.Application.Game.Online.Match;

/// <summary>
/// Online 匹配队列总览查询服务。路由前缀 /api/onlineMatchQueueQuery（change C341 / vault:C5 S4.9）。
/// <para>纯只读观测：实时透传 Online 查询，Admin 侧零持久化、零缓存（不复制队列状态为权威数据）。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 120)]
public class OnlineMatchQueueQueryService : IAdminApiController
{
    /// <summary>
    /// 队列总览查询 action 名（mock 先行，联调以 vault C5 详表为准）。
    /// </summary>
    private const string QueryMatchQueueOverviewAction = "query_match_queue_overview";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 匹配队列总览查询服务构造函数。
    /// </summary>
    public OnlineMatchQueueQueryService(
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 匹配队列总览查询：按 Mode/Region 维度返回队列深度、平均等待、吞吐快照，并标注 Online 侧 ServerTime。
    /// </summary>
    /// <param name="input">作用域（必填）+ 匹配模式/地区过滤。</param>
    /// <returns>队列快照行 + Online 侧快照时间。</returns>
    [HttpPost("overview")]
    [ApiDescriptionSettings(Name = "Overview")]
    [DisplayName("匹配队列总览查询")]
    public async Task<OnlineMatchOverviewResponse> OverviewAsync(OnlineMatchOverviewRequest input)
    {
        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, "match-queue-overview");
        var data = await _onlineServerClient.PostAsync<OnlineMatchOverviewQueryResultResponse>(QueryMatchQueueOverviewAction, new OnlineMatchOverviewQueryRequest
        {
            Mode = input.Mode,
            Region = input.Region,
        }, scope);

        return new OnlineMatchOverviewResponse
        {
            Queues = data.Queues,
            ServerTime = data.ServerTime,
            SnapshotNote = "运行态快照（Online 侧生成），非权威状态，请勿缓存",
        };
    }
}
