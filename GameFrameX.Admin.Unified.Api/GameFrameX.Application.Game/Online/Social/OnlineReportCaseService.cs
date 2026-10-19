using System.ComponentModel;
using GameFrameX.Application.Game.Online.Social.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty;
using GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty.Dto;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social;

/// <summary>
/// Online 举报案件处置服务。路由前缀 /api/onlineReportCase（change C350 / vault:C7）。
/// <para>案件数据实时透传 Online（Admin 不落镜像）；处置流三步：事实源写入（Mute/Ban 落 TenantGamePenaltyEntity）
/// → 联动命令（经 <see cref="OnlinePenaltyLinkCommandService"/>，失败不回滚事实源、可重试）→ 当事人通知 + 案件状态回写
/// （失败不阻断处置，原因分别落响应字段）。三步状态经 MonitorLogging 自动落租户操作审计。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 130)]
public class OnlineReportCaseService : IAdminApiController
{
    /// <summary>
    /// 案件查询 action 名（mock 先行，联调以 vault 详表为准）。
    /// </summary>
    private const string QueryReportCasesAction = "query_report_cases";

    /// <summary>
    /// 案件状态回写 action 名（mock 先行，联调以 vault 详表为准）。
    /// </summary>
    private const string UpdateReportCaseAction = "update_report_case";

    /// <summary>
    /// 当事人通知 action 名（mock 先行，联调以 vault 详表为准）。
    /// </summary>
    private const string SendAdminNotificationAction = "send_admin_notification";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;
    private readonly TenantGamePenaltyEntityService _penaltyEntityService;
    private readonly OnlinePenaltyLinkCommandService _penaltyLinkCommandService;

    /// <summary>
    /// Online 举报案件处置服务构造函数。
    /// </summary>
    public OnlineReportCaseService(
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory,
        TenantGamePenaltyEntityService penaltyEntityService,
        OnlinePenaltyLinkCommandService penaltyLinkCommandService)
    {
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
        _penaltyEntityService = penaltyEntityService;
        _penaltyLinkCommandService = penaltyLinkCommandService;
    }

    /// <summary>
    /// 举报案件分页查询：过滤条件透传 Online，游标分页往返。
    /// </summary>
    /// <param name="input">作用域（必填）+ 案件过滤条件 + 游标。</param>
    /// <returns>案件行 + 下一页游标。</returns>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    [DisplayName("举报案件分页查询")]
    public async Task<OnlineReportCasePageResponse> PageAsync(PageOnlineReportCaseRequest input)
    {
        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, "report-case-page");
        var data = await _onlineServerClient.PostAsync<OnlineReportCaseQueryResultResponse>(QueryReportCasesAction, new OnlineReportCaseQueryRequest
        {
            Status = input.Status,
            ReporterPlayerId = input.ReporterPlayerId,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            Cursor = input.Cursor,
            PageSize = input.PageSize,
        }, scope);

        return new OnlineReportCasePageResponse
        {
            Items = data.Items.Select(MapCaseToResponse).ToList(),
            NextCursor = data.NextCursor,
            HasMore = data.HasMore,
        };
    }

    /// <summary>
    /// 处置举报案件：按决策分派三步流（事实源 → 联动 → 通知/回写），任一步失败不静默——落审计并在响应中呈现。
    /// <para>
    /// 决策语义：Dismiss（驳回，不写事实源）/ Warn（警告通知，不写事实源）/ Mute / Ban（写处罚事实源并联动 Online 运行时）/
    /// ForwardPenalty（转交处罚流程人工跟进，案件转 Handling，不自动写事实源）。
    /// </para>
    /// </summary>
    /// <param name="input">案件作用域 + 决策 + 原因（+ Mute/Ban 时长与被举报人）。</param>
    /// <returns>三步结果聚合（含可重试的联动失败明细）。</returns>
    [HttpPost("handle")]
    [ApiDescriptionSettings(Name = "Handle")]
    [DisplayName("处置举报案件")]
    public async Task<OnlineReportHandleResponse> HandleAsync(OnlineReportHandleRequest input)
    {
        // 作用域前置校验：越界拒绝发生在任何写步骤之前（不产生半途事实）。
        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, $"report-handle:{input.CaseId}");
        var handledAt = TimerHelper.UnixTimeSeconds();

        // 第一步（仅 Mute/Ban）：处罚事实源写入。先落账后联动——事实源是 Admin 侧唯一处罚事实。
        long? penaltyId = null;
        if (input.Decision == OnlineReportDecisionKind.Mute || input.Decision == OnlineReportDecisionKind.Ban)
        {
            penaltyId = (await WritePenaltyFactAsync(input, handledAt)).Id;
        }

        // 第二步（仅 Mute/Ban）：联动命令下发（Online 运行时生效）。失败不回滚事实源，明细进响应可重试。
        OnlinePenaltyLinkResponse? linkCommand = null;
        if (penaltyId.HasValue)
        {
            linkCommand = await _penaltyLinkCommandService.ExecuteLinkAsync(penaltyId.Value, MapDecisionToLinkCommand(input.Decision));
        }

        // 第三步之一：当事人通知（Warn/Mute/Ban；失败不阻断处置，原因落响应）。
        var notified = false;
        string? notifiedFailureReason = null;
        if (NeedsPlayerNotification(input.Decision))
        {
            try
            {
                await _onlineServerClient.PostAsync<object>(SendAdminNotificationAction, new OnlineNotificationSendRequest
                {
                    PlayerId = input.ReportedPlayerId,
                    Title = BuildNotificationTitle(input.Decision),
                    Content = BuildNotificationContent(input),
                }, scope);
                notified = true;
            }
            catch (Exception exception) when (exception is BusinessException or InvalidOperationException or HttpRequestException)
            {
                notifiedFailureReason = exception.Message;
            }
        }

        // 第三步之二：案件状态回写 Online（失败不阻断处置，原因落响应；回写失败须人工重查）。
        var caseUpdated = false;
        string? caseUpdateFailureReason = null;
        try
        {
            await _onlineServerClient.PostAsync<object>(UpdateReportCaseAction, new OnlineReportCaseUpdateRequest
            {
                CaseId = input.CaseId,
                Status = MapDecisionToCaseStatus(input.Decision),
                HandleResult = BuildHandleResultSummary(input, penaltyId),
            }, scope);
            caseUpdated = true;
        }
        catch (Exception exception) when (exception is BusinessException or InvalidOperationException or HttpRequestException)
        {
            caseUpdateFailureReason = exception.Message;
        }

        return new OnlineReportHandleResponse
        {
            CaseId = input.CaseId,
            Decision = input.Decision,
            PenaltyId = penaltyId,
            LinkCommand = linkCommand,
            CaseUpdated = caseUpdated,
            CaseUpdateFailureReason = caseUpdateFailureReason,
            PlayerNotified = notified,
            NotifiedFailureReason = notifiedFailureReason,
            HandledAt = handledAt,
        };
    }

    /// <summary>
    /// 写处罚事实源（复用 TenantGamePenaltyEntityService.AddAsync 语义：服务端强制 Status=Active、OperatorId=当前操作人）。
    /// <para>SourceReportId 为 long? 而案件 Id 为 Online 侧字符串，类型不匹配不写入——案件回链经 Reason 摘要与审计日志保留。</para>
    /// </summary>
    private async Task<TenantGamePenaltyEntityResponse> WritePenaltyFactAsync(OnlineReportHandleRequest input, long handledAt)
    {
        if (!input.DurationMinutes.HasValue)
        {
            // Mute / Ban 必带时长（DTO Range 校验不覆盖「必填」语义，服务端兜底）。
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "DurationMinutes", });
        }

        return await _penaltyEntityService.AddAsync(new AddTenantGamePenaltyEntityRequest
        {
            AppId = input.AppId,
            AreaId = input.ServerId,
            TargetType = PenaltyTargetTypeEnum.Account,
            TargetId = input.ReportedPlayerId,
            ActionType = MapDecisionToPenaltyAction(input.Decision),
            Reason = $"[举报案件] {input.Reason}（案件 {input.CaseId}）",
            ExpireTime = handledAt + input.DurationMinutes.Value * 60L,
            SourceReportId = null,
        });
    }

    /// <summary>
    /// 处置决策 → 处罚动作映射（仅 Mute / Ban 进入事实源；其余决策不经本方法）。
    /// </summary>
    private static PenaltyActionTypeEnum MapDecisionToPenaltyAction(OnlineReportDecisionKind decision)
    {
        return decision == OnlineReportDecisionKind.Ban ? PenaltyActionTypeEnum.Ban : PenaltyActionTypeEnum.Mute;
    }

    /// <summary>
    /// 处置决策 → 联动命令映射（Kick 不在处置决策直达路径——踢下线只经处罚页手动触发）。
    /// </summary>
    private static OnlinePenaltyCommandKind MapDecisionToLinkCommand(OnlineReportDecisionKind decision)
    {
        return decision == OnlineReportDecisionKind.Ban ? OnlinePenaltyCommandKind.Ban : OnlinePenaltyCommandKind.Mute;
    }

    /// <summary>
    /// 处置决策 → 案件状态映射：Dismiss→Rejected；Warn/Mute/Ban→Resolved；ForwardPenalty→Handling（待人工跟进）。
    /// </summary>
    private static OnlineReportCaseStatusKind MapDecisionToCaseStatus(OnlineReportDecisionKind decision)
    {
        switch (decision)
        {
            case OnlineReportDecisionKind.Dismiss:
            {
                return OnlineReportCaseStatusKind.Rejected;
            }

            case OnlineReportDecisionKind.ForwardPenalty:
            {
                return OnlineReportCaseStatusKind.Handling;
            }

            default:
            {
                return OnlineReportCaseStatusKind.Resolved;
            }
        }
    }

    /// <summary>
    /// 是否需要向被举报人推送通知：Warn（警告）/ Mute / Ban（处罚结果）推送；Dismiss / ForwardPenalty 不打扰当事人。
    /// </summary>
    private static bool NeedsPlayerNotification(OnlineReportDecisionKind decision)
    {
        return decision == OnlineReportDecisionKind.Warn
               || decision == OnlineReportDecisionKind.Mute
               || decision == OnlineReportDecisionKind.Ban;
    }

    /// <summary>
    /// 当事人通知标题（按决策给固定文案，不透出审计内部字段）。
    /// </summary>
    private static string BuildNotificationTitle(OnlineReportDecisionKind decision)
    {
        switch (decision)
        {
            case OnlineReportDecisionKind.Warn:
            {
                return "违规警告通知";
            }

            case OnlineReportDecisionKind.Mute:
            {
                return "禁言处罚通知";
            }

            case OnlineReportDecisionKind.Ban:
            {
                return "封禁处罚通知";
            }

            default:
            {
                return "举报处置通知";
            }
        }
    }

    /// <summary>
    /// 当事人通知内容（决策 + 时长 + 原因摘要；处罚 Id 不对玩家暴露）。
    /// </summary>
    private static string BuildNotificationContent(OnlineReportHandleRequest input)
    {
        var duration = input.DurationMinutes.HasValue ? $"，时长 {input.DurationMinutes.Value} 分钟" : string.Empty;
        return $"您因被举报并经审核确认违规{duration}：{input.Reason}。如有异议请通过客服渠道申诉。";
    }

    /// <summary>
    /// 案件处置结果摘要（回写 Online 供举报人侧重展示；含处罚 Id 回链供内部追查）。
    /// </summary>
    private static string BuildHandleResultSummary(OnlineReportHandleRequest input, long? penaltyId)
    {
        var penaltySuffix = penaltyId.HasValue ? $"（处罚Id {penaltyId.Value}）" : string.Empty;
        return $"{input.Decision}：{input.Reason}{penaltySuffix}";
    }

    /// <summary>
    /// 线缆案件行 → API 响应行映射（同构直拷；证据字段透传）。
    /// </summary>
    private static OnlineReportCaseItemResponse MapCaseToResponse(OnlineReportCaseResponse item)
    {
        return new OnlineReportCaseItemResponse
        {
            CaseId = item.CaseId,
            AppId = item.AppId,
            ServerId = item.ServerId,
            ReporterPlayerId = item.ReporterPlayerId,
            ReportedPlayerId = item.ReportedPlayerId,
            Scene = item.Scene,
            MatchId = item.MatchId,
            ChatMessageId = item.ChatMessageId,
            Reason = item.Reason,
            Evidence = item.Evidence,
            Status = item.Status,
            HandleResult = item.HandleResult,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
        };
    }
}
