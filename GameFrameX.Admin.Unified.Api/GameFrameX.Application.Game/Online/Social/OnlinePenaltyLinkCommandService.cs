using System.ComponentModel;
using GameFrameX.Application.Game.Online.Social.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social;

/// <summary>
/// Online 处罚联动命令服务（高危；权限码 onlinePenaltyLinkCommand:execute 仅同步 TenantAdmin）。
/// <para>
/// 路由前缀 /api/onlinePenaltyLinkCommand（change C350 / vault:C7、vault:C2 S1.4）。
/// 经 <see cref="IOnlineServerClient"/> 调 Online 显式命令接口（踢下线 / 吊销 Token / 禁言，三命令硬白名单，X3 红线）；
/// 联动命令调用点唯一——处置流（OnlineReportCaseService）与既有页面增量（TenantGamePenaltyEntityService.SyncOnlineRuntimeAsync）
/// 一律经 <see cref="ExecuteLinkAsync"/>，不得绕过本服务直调 Online 命令接口（守护测试 OnlineSocialCommandWhitelistTests）。
/// </para>
/// <para>
/// 语义契约：Admin 侧处罚事实仍以 <c>TenantGamePenaltyEntity</c> 为唯一事实源，本服务只把「已生效的事实」同步到 Online 运行时
/// （事实源先行）；联动失败不回滚事实源，可按 IdempotencyKey 重试（见 <see cref="BuildLinkCommandIdempotencyKey"/>）。
/// </para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 131)]
public class OnlinePenaltyLinkCommandService : IAdminApiController
{
    /// <summary>
    /// 踢下线命令的 Online 管理 action 名（vault:C2 S1.4 契约；mock 先行，联调以 vault 详表为准）。
    /// </summary>
    private const string KickPlayerAction = "kick_player";

    /// <summary>
    /// 封禁命令（吊销全部 Session Token 并阻断后续登录）的 Online 管理 action 名（vault:C2 S1.4）。
    /// </summary>
    private const string RevokeTokenAction = "revoke_token";

    /// <summary>
    /// 禁言命令的 Online 管理 action 名（vault:C7 统一决策服务）。
    /// </summary>
    private const string MutePlayerAction = "mute_player";

    private readonly SqlSugarRepository<TenantGamePenaltyEntity> _penaltyRepository;
    private readonly SqlSugarRepository<TenantGameAreaEntity> _gameAreaRepository;
    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 处罚联动命令服务构造函数。
    /// </summary>
    public OnlinePenaltyLinkCommandService(
        SqlSugarRepository<TenantGamePenaltyEntity> penaltyRepository,
        SqlSugarRepository<TenantGameAreaEntity> gameAreaRepository,
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _penaltyRepository = penaltyRepository;
        _gameAreaRepository = gameAreaRepository;
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 执行处罚联动命令（高危端点）：按处罚事实源行推导作用域并下发 Online 命令。
    /// </summary>
    /// <param name="input">处罚 Id + 联动命令（Kick / Ban / Mute 三值硬白名单）。</param>
    /// <returns>各联动目标区服的聚合执行结果（含旧 Token 吊销确认字段）。</returns>
    [HttpPost("execute")]
    [ApiDescriptionSettings(Name = "Execute")]
    [DisplayName("执行Online处罚联动命令")]
    public Task<OnlinePenaltyLinkResponse> ExecuteAsync(OnlinePenaltyLinkRequest input)
    {
        return ExecuteLinkAsync(input.PenaltyId, input.Command);
    }

    /// <summary>
    /// 联动命令唯一执行入口（供处置流与 gamePenaltyEntity 页同步动作复用）。
    /// <para>
    /// 「事实源先行、联动可重试」：调用前处罚事实必须已写入 <c>TenantGamePenaltyEntity</c> 且处于生效中——
    /// Online 命令只是把既有事实生效到运行时，本方法不产生任何新处罚；联动失败不影响事实源状态，
    /// 重新调用同（PenaltyId, Command, ServerId）组合时 Online 侧按 IdempotencyKey 返回首次结果（vault:C2 S1.5），
    /// 因此重试天然幂等，不会重复处罚。
    /// </para>
    /// </summary>
    /// <param name="penaltyId">处罚事实源行 Id（必须生效中）。</param>
    /// <param name="command">联动命令（Kick / Ban / Mute）。</param>
    /// <returns>各联动目标区服的聚合执行结果。</returns>
    [NonAction]
    public async Task<OnlinePenaltyLinkResponse> ExecuteLinkAsync(long penaltyId, OnlinePenaltyCommandKind command)
    {
        var penalty = await _penaltyRepository.GetFirstAsync(u => u.Id == penaltyId);
        if (penalty == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound, new { penaltyId, });
        }

        if (penalty.Status != PenaltyStatusEnum.Active)
        {
            throw BusinessError.Oh(OnlineModerationErrorCode.PenaltyNotActive, new { penaltyId, status = penalty.Status.ToString(), });
        }

        if (penalty.ActionType == PenaltyActionTypeEnum.RestrictTrade || penalty.ActionType == PenaltyActionTypeEnum.RestrictMatch)
        {
            // 限制交易 / 限制匹配是纯落账动作（不阻断登录，也无 Online 运行时对应物），对其发联动命令会产生「已同步」错觉。
            throw BusinessError.Oh(OnlineModerationErrorCode.PenaltyActionNotLinkable, new { penaltyId, actionType = penalty.ActionType.ToString(), });
        }

        var serverIds = await ResolveLinkTargetServerIdsAsync(penalty);
        var results = new List<OnlinePenaltyLinkResultResponse>();
        foreach (var serverId in serverIds)
        {
            results.Add(await ExecuteForServerAsync(penalty, command, serverId));
        }

        return new OnlinePenaltyLinkResponse
        {
            PenaltyId = penalty.Id,
            Command = command,
            Results = results,
            AllSucceeded = results.All(u => u.Success),
        };
    }

    /// <summary>
    /// 确定联动目标区服集：处罚行 AreaId 有值 = 单区服；null（全区服生效）= App 下全部区服逐一联动。
    /// </summary>
    private async Task<List<long>> ResolveLinkTargetServerIdsAsync(TenantGamePenaltyEntity penalty)
    {
        if (penalty.AreaId.HasValue)
        {
            return new List<long> { penalty.AreaId.Value, };
        }

        var serverIds = await _gameAreaRepository.AsQueryable()
                          .Where(u => u.GameAppId == penalty.AppId)
                          .Select(u => u.Id)
                          .ToListAsync();
        if (serverIds.Count == 0)
        {
            throw BusinessError.Oh(OnlineModerationErrorCode.PenaltyLinkTargetMissing, new { penaltyId = penalty.Id, appId = penalty.AppId, });
        }

        return serverIds;
    }

    /// <summary>
    /// 向单个区服下发联动命令并落一条结果（含旧 Token 吊销确认字段）。
    /// <para>单区服命令失败（Online 业务码 / 传输异常）不中断其余区服——聚合语义下逐区服呈现成败，可针对性重试。</para>
    /// </summary>
    private async Task<OnlinePenaltyLinkResultResponse> ExecuteForServerAsync(TenantGamePenaltyEntity penalty, OnlinePenaltyCommandKind command, long serverId)
    {
        var action = MapCommandToAction(command);
        try
        {
            var scope = await _scopeContextFactory.CreateAsync(penalty.AppId, serverId, $"penalty-link:{penalty.Id}:{command}");
            var data = await _onlineServerClient.PostAsync<OnlineLinkCommandAckResponse>(action, new OnlineLinkCommandRequest
            {
                PenaltyId = penalty.Id,
                PlayerId = penalty.TargetId,
                IdempotencyKey = BuildLinkCommandIdempotencyKey(penalty.Id, command, serverId),
            }, scope);

            return new OnlinePenaltyLinkResultResponse
            {
                ServerId = serverId,
                Success = true,
                OnlineCode = null,
                FailureMessage = null,
                TokenRevokedConfirmed = data?.TokenRevokedConfirmed ?? false,
            };
        }
        catch (BusinessException exception)
        {
            // Online 业务码非 0：镜像码已由 OnlineServerClient 段级映射；这里降级为单区服失败明细，聚合呈现。
            return new OnlinePenaltyLinkResultResponse
            {
                ServerId = serverId,
                Success = false,
                OnlineCode = exception.Code,
                FailureMessage = exception.Message,
                TokenRevokedConfirmed = false,
            };
        }
        catch (Exception exception) when (exception is InvalidOperationException or HttpRequestException)
        {
            // 传输层失败（信封解包失败 / HTTP 不可达）：可重试（IdempotencyKey 保证 Online 侧不重复生效）。
            return new OnlinePenaltyLinkResultResponse
            {
                ServerId = serverId,
                Success = false,
                OnlineCode = null,
                FailureMessage = exception.Message,
                TokenRevokedConfirmed = false,
            };
        }
    }

    /// <summary>
    /// 联动命令 → Online 管理 action 名映射（写路径三命令硬白名单的唯一映射点）。
    /// <para>Kick=踢下线；Ban=吊销全部 Token 并阻断后续登录；Mute=禁言。新增命令必须回 vault 评审并同步守护测试。</para>
    /// </summary>
    private static string MapCommandToAction(OnlinePenaltyCommandKind command)
    {
        switch (command)
        {
            case OnlinePenaltyCommandKind.Kick:
            {
                return KickPlayerAction;
            }

            case OnlinePenaltyCommandKind.Ban:
            {
                return RevokeTokenAction;
            }

            case OnlinePenaltyCommandKind.Mute:
            {
                return MutePlayerAction;
            }

            default:
            {
                // 枚举三值硬白名单之外的成员（守护测试已锁死），运行时兜底拒绝而非放行。
                throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { command = command.ToString(), });
            }
        }
    }

    /// <summary>
    /// 构建联动命令幂等键：<c>penalty-{PenaltyId}-{command}-{serverId}</c>。
    /// <para>
    /// 为什么先写事实源：Admin 侧处罚以 <c>TenantGamePenaltyEntity</c> 为唯一事实源，Online 命令只负责「生效到运行时」；
    /// 若命令先于事实落库，联路中断会出现「运行时已被限制但 Admin 无记录」的不可审计状态。
    /// 为什么重试幂等：同一（处罚 + 命令 + 区服）组合的重复调用携带相同 IdempotencyKey，Online 侧按 vault:C2 S1.5
    /// 契约返回首次执行结果，不会重复踢人 / 重复吊销；因此联动失败可以直接安全重试（含 gamePenaltyEntity 页的手动同步）。
    /// </para>
    /// </summary>
    private static string BuildLinkCommandIdempotencyKey(long penaltyId, OnlinePenaltyCommandKind command, long serverId)
    {
        return $"penalty-{penaltyId}-{command.ToString().ToLowerInvariant()}-{serverId}";
    }
}
