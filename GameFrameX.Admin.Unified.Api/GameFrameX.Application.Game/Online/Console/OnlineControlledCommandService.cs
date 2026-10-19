using System.ComponentModel;
using GameFrameX.Application.Game.Online.Console.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Enum;

namespace GameFrameX.Application.Game.Online.Console;

/// <summary>
/// Online 控制台受控操作命令服务（高危；权限码 onlineControlledCommand:execute 仅同步 TenantAdmin）。
/// <para>
/// 路由前缀 /api/onlineControlledCommand（change C344 / vault:C9 S8.1）。
/// 只承载四条新命令（<see cref="OnlineControlledCommandKind"/> 严格四值），经 <see cref="IOnlineServerClient"/>
/// 直调 Online 显式命令接口；命令 action 名唯一实现点集中本文件（守护测试 OnlineOverviewIsolationTests）。
/// </para>
/// <para>
/// 受控操作复用矩阵（禁平行实现）：踢下线/封禁/禁言经 C342 后端半边落地的
/// <c>OnlinePenaltyLinkCommandService</c>（/api/onlinePenaltyLinkCommand/execute，承载 C350 处罚联动域），
/// 发放/撤销资产经 C340 <c>OnlineAssetGrantCommandService</c>（/api/onlineAssetGrantCommand/grant|revoke）
/// ——控制台操作面板直调上述既有端点，本服务不转发、不重新实现；
/// Online/Console 源码出现既有命令 token 即为违规（守护测试 + 既有白名单测试全域拦截）。
/// </para>
/// <para>
/// 高危复核口径：本端点整体高危——<c>Reason</c> 与 <c>ReviewerOperatorId</c> 对全部四命令必填（双人复核字段），
/// 审计经 MonitorLogging 全 action 自动落库 + Online 侧审计事件双通道留痕。
/// </para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 139)]
public class OnlineControlledCommandService : IAdminApiController
{
    /// <summary>
    /// 命令路由解析结果（action 名 + 目标标识 + 开关方向；单向命令方向为 null）。
    /// </summary>
    private sealed class CommandRoute
    {
        public string Action { get; set; } = string.Empty;

        public string TargetIdentifier { get; set; } = string.Empty;

        public ControlledCommandDirection? Direction { get; set; }
    }

    /// <summary>
    /// 取消匹配 Ticket 命令的 Online 管理 action 名（mock 先行，联调以 vault C9 详表为准）。
    /// </summary>
    private const string CancelMatchTicketAction = "cancel_match_ticket";

    /// <summary>
    /// 结束异常 Match 命令的 Online 管理 action 名（mock 先行，联调以 vault C9 详表为准）。
    /// </summary>
    private const string EndAbnormalMatchAction = "end_abnormal_match";

    /// <summary>
    /// 暂停/恢复玩家命令的 Online 管理 action 名（mock 先行，联调以 vault C9 详表为准）。
    /// </summary>
    private const string SuspendResumePlayerAction = "suspend_resume_player";

    /// <summary>
    /// 匹配隔离命令的 Online 管理 action 名（mock 先行，联调以 vault C9 详表为准）。
    /// </summary>
    private const string MatchIsolationAction = "match_isolation";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 控制台受控操作命令服务构造函数。
    /// </summary>
    public OnlineControlledCommandService(
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 执行受控操作命令（高危端点）：四值命令路由 + 目标/方向校验 + 双人复核字段必填。
    /// <para>
    /// 幂等语义（vault:C9 VC-8.4 / vault:C2 S1.5）：每次调用按
    /// <see cref="BuildIdempotencyKey"/> 规则派生 Online IdempotencyKey 下发——同一（命令, 方向, 目标, 区服）
    /// 组合的重复调用携带相同幂等键，Online 侧按契约返回首次执行结果，不产生第二次副作用；
    /// 因此网络重试 / 运营重复点击天然安全，失败可直接重试。
    /// </para>
    /// </summary>
    /// <param name="input">作用域（必填）+ 命令（严格四值）+ 目标标识 + 方向（开关型命令）+ 原因 + 复核人（均必填项见 DTO 注释）。</param>
    /// <returns>Online 首次执行结果回执（命令/目标/幂等键/状态/时间/摘要）。</returns>
    [HttpPost("execute")]
    [ApiDescriptionSettings(Name = "Execute")]
    [DisplayName("执行Online受控操作命令")]
    public async Task<OnlineControlledCommandAckResponse> ExecuteAsync(OnlineControlledCommandRequest input)
    {
        ValidateReasonAndReviewer(input.Reason, input.ReviewerOperatorId);
        var route = ResolveCommandTarget(input);

        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, $"controlled-command:{input.Command}:{route.TargetIdentifier}");
        return await _onlineServerClient.PostAsync<OnlineControlledCommandAckResponse>(route.Action, new OnlineControlledCommandDispatchRequest
        {
            Command = route.Action,
            TicketId = input.Command == OnlineControlledCommandKind.CancelTicket ? input.TicketId : null,
            MatchId = input.Command == OnlineControlledCommandKind.EndAbnormalMatch ? input.MatchId : null,
            PlayerId = input.Command == OnlineControlledCommandKind.SuspendResumePlayer || input.Command == OnlineControlledCommandKind.MatchIsolation ? input.PlayerId : null,
            Direction = route.Direction?.ToString(),
            Reason = input.Reason,
            ReviewerOperatorId = input.ReviewerOperatorId,
            IdempotencyKey = BuildIdempotencyKey(route.Action, route.Direction, route.TargetIdentifier, input.ServerId),
        }, scope);
    }

    /// <summary>
    /// 校验操作原因与复核人必填（高危双人复核口径；DTO [Required] 之外的兜底：空白字符串语义）。
    /// </summary>
    private static void ValidateReasonAndReviewer(string reason, string reviewerOperatorId)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "Reason", });
        }

        if (string.IsNullOrWhiteSpace(reviewerOperatorId))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "ReviewerOperatorId", });
        }
    }

    /// <summary>
    /// 四值命令路由：解析 action 名、目标标识与开关方向，并校验目标/方向字段的必填与禁填组合。
    /// <para>
    /// 目标标识组合矩阵：CancelTicket → TicketId；EndAbnormalMatch → MatchId；
    /// SuspendResumePlayer / MatchIsolation → PlayerId + Direction（开关型命令必填方向）。
    /// 单向命令携带方向、命令缺失自己的目标字段，均按参数校验拒绝——组合错配会在 Online 侧产生语义歧义。
    /// </para>
    /// </summary>
    private static CommandRoute ResolveCommandTarget(OnlineControlledCommandRequest input)
    {
        switch (input.Command)
        {
            case OnlineControlledCommandKind.CancelTicket:
            {
                if (string.IsNullOrWhiteSpace(input.TicketId))
                {
                    throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "TicketId", command = input.Command.ToString(), });
                }

                RejectUnexpectedDirection(input.Direction, input.Command);
                RejectUnexpectedTargets(input, expectTicketId: false, expectMatchId: true, expectPlayerId: true);
                return new CommandRoute { Action = CancelMatchTicketAction, TargetIdentifier = input.TicketId, Direction = null, };
            }

            case OnlineControlledCommandKind.EndAbnormalMatch:
            {
                if (string.IsNullOrWhiteSpace(input.MatchId))
                {
                    throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "MatchId", command = input.Command.ToString(), });
                }

                RejectUnexpectedDirection(input.Direction, input.Command);
                RejectUnexpectedTargets(input, expectTicketId: true, expectMatchId: false, expectPlayerId: true);
                return new CommandRoute { Action = EndAbnormalMatchAction, TargetIdentifier = input.MatchId, Direction = null, };
            }

            case OnlineControlledCommandKind.SuspendResumePlayer:
            case OnlineControlledCommandKind.MatchIsolation:
            {
                if (string.IsNullOrWhiteSpace(input.PlayerId))
                {
                    throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "PlayerId", command = input.Command.ToString(), });
                }

                if (!input.Direction.HasValue)
                {
                    // 开关型命令必须携带方向：同一目标「暂停」与「恢复」是两次独立命令意图。
                    throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "Direction", command = input.Command.ToString(), });
                }

                RejectUnexpectedTargets(input, expectTicketId: true, expectMatchId: true, expectPlayerId: false);
                var action = input.Command == OnlineControlledCommandKind.SuspendResumePlayer ? SuspendResumePlayerAction : MatchIsolationAction;
                return new CommandRoute { Action = action, TargetIdentifier = input.PlayerId, Direction = input.Direction.Value, };
            }

            default:
            {
                // 枚举四值硬白名单之外的成员（守护测试已锁死），运行时兜底拒绝而非放行。
                throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { command = input.Command.ToString(), });
            }
        }
    }

    /// <summary>
    /// 单向命令（CancelTicket / EndAbnormalMatch）禁止携带方向——方向只对开关型命令有意义。
    /// </summary>
    private static void RejectUnexpectedDirection(ControlledCommandDirection? direction, OnlineControlledCommandKind command)
    {
        if (direction.HasValue)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "Direction", command = command.ToString(), reason = "单向命令不支持方向", });
        }
    }

    /// <summary>
    /// 命令不消费的目标字段必须为空——组合错配（如 CancelTicket 带 MatchId）按参数校验拒绝。
    /// </summary>
    private static void RejectUnexpectedTargets(OnlineControlledCommandRequest input, bool expectTicketId, bool expectMatchId, bool expectPlayerId)
    {
        if (expectTicketId && !string.IsNullOrWhiteSpace(input.TicketId))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "TicketId", command = input.Command.ToString(), });
        }

        if (expectMatchId && !string.IsNullOrWhiteSpace(input.MatchId))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "MatchId", command = input.Command.ToString(), });
        }

        if (expectPlayerId && !string.IsNullOrWhiteSpace(input.PlayerId))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "PlayerId", command = input.Command.ToString(), });
        }
    }

    /// <summary>
    /// 构建受控命令幂等键：<c>onlineControlled-{command}-{direction}-{targetId}-{serverId}</c>（单向命令省略 direction 段）。
    /// <para>
    /// {command} 段用 Online action 名（snake_case，与线缆 <c>Command</c> 一致），不用枚举名形态——
    /// 幂等键与下发命令在 Online 侧可按同一标识对账。
    /// 为什么方向参与幂等键：同一目标的「暂停」与「恢复」是两次独立命令意图，各自独立幂等——
    /// 若共享幂等键，恢复操作会被 Online 判定为重复调用而返回暂停的首次结果，开关语义反转。
    /// 为什么区服参与幂等键：同一命令可能逐区服下发（与 C350 处罚联动幂等键口径一致）。
    /// 重复执行语义（VC-8.4）：同键重复调用 Online 侧返回首次执行结果，不产生第二次副作用。
    /// </para>
    /// </summary>
    private static string BuildIdempotencyKey(string action, ControlledCommandDirection? direction, string targetIdentifier, long serverId)
    {
        var directionSegment = direction.HasValue ? "-" + direction.Value.ToString().ToLowerInvariant() : string.Empty;
        return $"onlineControlled-{action}{directionSegment}-{targetIdentifier}-{serverId}";
    }
}
