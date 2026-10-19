using System.ComponentModel;
using GameFrameX.Application.Game.Online.Console.Dto;
using GameFrameX.Application.Game.ServerManager.Online;

namespace GameFrameX.Application.Game.Online.Console;

/// <summary>
/// Online 玩家时间线查询服务。路由前缀 /api/onlinePlayerTimelineQuery（change C344 / vault:C9 S8.1）。
/// <para>
/// 只消费 Online 事件信封（EventId / CorrelationId 关联）：全部事件行原值透传，
/// 缺事件即空槽呈现——Admin 不构造事件、不推断分组归属、不本地补齐时间线空位（口径红线，守护测试守护范围之外的行为契约）。
/// </para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 137)]
public class OnlinePlayerTimelineQueryService : IAdminApiController
{
    /// <summary>
    /// 玩家时间线查询 action 名（mock 先行，联调以 vault C9 详表为准）。
    /// </summary>
    private const string QueryPlayerTimelineAction = "query_player_timeline";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 玩家时间线查询服务构造函数。
    /// </summary>
    public OnlinePlayerTimelineQueryService(
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 玩家时间线查询：按 PlayerId 拉取跨域事件链（Session/匹配/对局/资产/社交/处罚），
    /// 支持事件分组过滤 + 时间窗 + 游标翻页；事件行与游标原值往返透传 Online。
    /// </summary>
    /// <param name="input">作用域（必填）+ 玩家 Id（必填）+ 分组过滤 + 时间窗 + 游标。</param>
    /// <returns>事件行（信封原值）+ 下一页游标；Online 无事件时返回空 Events（空槽语义，非错误）。</returns>
    [HttpPost("query")]
    [ApiDescriptionSettings(Name = "Query")]
    [DisplayName("玩家时间线查询")]
    public async Task<OnlinePlayerTimelineResponse> QueryAsync(OnlinePlayerTimelineRequest input)
    {
        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, $"player-timeline:{input.PlayerId}");
        var data = await _onlineServerClient.PostAsync<OnlinePlayerTimelineQueryResultResponse>(QueryPlayerTimelineAction, new OnlinePlayerTimelineQueryRequest
        {
            PlayerId = input.PlayerId,
            EventGroup = input.EventGroup?.ToString(),
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            Cursor = input.Cursor,
            PageSize = input.PageSize,
        }, scope);

        return new OnlinePlayerTimelineResponse
        {
            PlayerId = input.PlayerId,
            Events = data.Events,
            NextCursor = data.NextCursor,
            HasMore = data.HasMore,
        };
    }
}
