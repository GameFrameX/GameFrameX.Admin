using System.ComponentModel;
using GameFrameX.Application.Game.Online.Match.Dto;
using GameFrameX.Application.Game.ServerManager.Online;

namespace GameFrameX.Application.Game.Online.Match;

/// <summary>
/// Online 匹配 Ticket 明细查询服务。路由前缀 /api/onlineMatchTicketQuery（change C341 / vault:C5 S4.9）。
/// <para>纯只读观测：失败原因明细与异常 Ticket（长时间未分配/重复取消/孤儿）检索实时透传 Online，
/// 游标翻页往返；Admin 侧零持久化、零缓存。治理动作（如取消 Ticket）属 vault:C9 受控操作（C344），不在本服务。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 121)]
public class OnlineMatchTicketQueryService : IAdminApiController
{
    /// <summary>
    /// 失败原因明细查询 action 名（mock 先行，联调以 vault C5 详表为准）。
    /// </summary>
    private const string QueryMatchTicketFailuresAction = "query_match_ticket_failures";

    /// <summary>
    /// 异常 Ticket 查询 action 名（mock 先行，联调以 vault C5 详表为准）。
    /// </summary>
    private const string QueryMatchAbnormalTicketsAction = "query_match_abnormal_tickets";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 匹配 Ticket 明细查询服务构造函数。
    /// </summary>
    public OnlineMatchTicketQueryService(
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 匹配失败原因分页查询：原因码/时间窗过滤 + 按原因码聚合摘要，游标翻页往返透传 Online。
    /// </summary>
    /// <param name="input">作用域（必填）+ 原因码/时间窗过滤 + 游标。</param>
    /// <returns>失败明细（含展示键）+ 聚合摘要 + Online 侧快照时间。</returns>
    [HttpPost("failurePage")]
    [ApiDescriptionSettings(Name = "FailurePage")]
    [DisplayName("匹配失败原因分页查询")]
    public async Task<OnlineMatchFailurePageResponse> FailurePageAsync(PageOnlineMatchFailureRequest input)
    {
        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, "match-failure-page");
        var data = await _onlineServerClient.PostAsync<OnlineMatchFailureQueryResultResponse>(QueryMatchTicketFailuresAction, new OnlineMatchFailureQueryRequest
        {
            ReasonCode = input.ReasonCode,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            Cursor = input.Cursor,
            PageSize = input.PageSize,
        }, scope);

        return new OnlineMatchFailurePageResponse
        {
            Items = data.Items.Select(entry => new OnlineMatchFailureResponse
            {
                TicketId = entry.TicketId,
                PartyId = entry.PartyId,
                ReasonCode = entry.ReasonCode,
                ReasonDisplayKey = OnlineMatchTicketDisplayMap.MapFailureReasonToDisplayKey(entry.ReasonCode),
                ReasonText = entry.ReasonText,
                OccurredAt = entry.OccurredAt,
            }).ToList(),
            ReasonSummary = data.ReasonSummary,
            NextCursor = data.NextCursor,
            HasMore = data.HasMore,
            ServerTime = data.ServerTime,
        };
    }

    /// <summary>
    /// 异常 Ticket 分页查询：按异常类别（长时间未分配/重复取消/孤儿）检索，游标翻页往返透传 Online。
    /// </summary>
    /// <param name="input">作用域（必填）+ 异常类别 + 游标。</param>
    /// <returns>异常 Ticket 行（含状态展示键）+ Online 侧快照时间。</returns>
    [HttpPost("abnormalPage")]
    [ApiDescriptionSettings(Name = "AbnormalPage")]
    [DisplayName("异常Ticket分页查询")]
    public async Task<OnlineMatchAbnormalTicketPageResponse> AbnormalPageAsync(PageOnlineMatchAbnormalTicketRequest input)
    {
        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, "match-abnormal-page");
        var data = await _onlineServerClient.PostAsync<OnlineMatchAbnormalTicketQueryResultResponse>(QueryMatchAbnormalTicketsAction, new OnlineMatchAbnormalTicketQueryRequest
        {
            AbnormalKind = input.AbnormalKind,
            Cursor = input.Cursor,
            PageSize = input.PageSize,
        }, scope);

        return new OnlineMatchAbnormalTicketPageResponse
        {
            Items = data.Items.Select(entry => new OnlineMatchAbnormalTicketResponse
            {
                TicketId = entry.TicketId,
                PartyId = entry.PartyId,
                Mode = entry.Mode,
                Region = entry.Region,
                Status = entry.Status,
                StatusDisplayKey = OnlineMatchTicketDisplayMap.MapTicketStatusToDisplayKey(entry.Status),
                CreatedAt = entry.CreatedAt,
                ExpiresAt = entry.ExpiresAt,
                WaitingSeconds = entry.WaitingSeconds,
                CancelCount = entry.CancelCount,
            }).ToList(),
            NextCursor = data.NextCursor,
            HasMore = data.HasMore,
            ServerTime = data.ServerTime,
        };
    }
}
