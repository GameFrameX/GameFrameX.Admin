using System.ComponentModel;
using GameFrameX.Application.Game.Online.Console.Dto;
using GameFrameX.Application.Game.ServerManager.Online;

namespace GameFrameX.Application.Game.Online.Console;

/// <summary>
/// Online 在线总览查询服务。路由前缀 /api/onlineOverviewQuery（change C344 / vault:C9 S8.1）。
/// <para>
/// 快照语义：实时透传 Online 玩家侧查询 API，Admin 侧零持久化、零缓存——运行态快照非权威状态。
/// X6 口径红线：数据源唯一 = Online 玩家侧查询 API，<b>不含管理员连接</b>——不得计入管理员浏览器
/// 连接链路（Hub 长连接与在线用户服务的管理员口径），也不得复用首页驾驶舱的在线计数口径
/// （C321 产物，与玩家在线口径不同源）；守护测试 OnlineOverviewIsolationTests 锁定本 Service
/// 及其依赖闭包对相关四个类型的零引用（类型名清单见守护测试内）。
/// </para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 136)]
public class OnlineOverviewQueryService : IAdminApiController
{
    /// <summary>
    /// 在线总览快照查询 action 名（mock 先行，联调以 vault C9 详表为准）。
    /// </summary>
    private const string QueryOnlineOverviewAction = "query_online_overview";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 在线总览查询服务构造函数。
    /// </summary>
    public OnlineOverviewQueryService(
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 在线总览快照查询：在线玩家数 / Session 数 / 匹配队列概况 / 进行中对局数 / 重连率 + Online 侧快照时间。
    /// <para>缺快照字段即缺省呈现（Online 侧返回什么呈现什么），Admin 不做本地聚合或推断补齐。</para>
    /// </summary>
    /// <param name="input">作用域（必填）。</param>
    /// <returns>玩家侧运行态快照 + 快照口径说明。</returns>
    [HttpPost("snapshot")]
    [ApiDescriptionSettings(Name = "Snapshot")]
    [DisplayName("在线总览快照查询")]
    public async Task<OnlineOverviewResponse> SnapshotAsync(OnlineOverviewRequest input)
    {
        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, "online-overview-snapshot");
        var data = await _onlineServerClient.PostAsync<OnlineOverviewQueryResultResponse>(QueryOnlineOverviewAction, new OnlineOverviewQueryRequest(), scope);

        return new OnlineOverviewResponse
        {
            OnlinePlayerCount = data.OnlinePlayerCount,
            SessionCount = data.SessionCount,
            MatchCount = data.MatchCount,
            ReconnectRate = data.ReconnectRate,
            QueueSummaries = data.QueueSummaries,
            ServerTime = data.ServerTime,
            SnapshotNote = "运行态快照（Online 侧生成，玩家侧口径），非权威状态，请勿缓存",
        };
    }
}
