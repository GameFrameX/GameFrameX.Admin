using GameFrameX.Application.Game.Tenant.AntiCheat.TenantGameAntiCheatEvent.Dto;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Core.Const;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Idempotency;

#pragma warning disable CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Tenant.AntiCheat.TenantGameAntiCheatEvent;

/// <summary>
/// 反作弊事件账本 Admin 控制面。安全治理事件事实源的录入与人工复核入口。
/// <para>非通用 CRUD：上报（SubmitAsync）幂等且一律落 Pending；变更（ReviewAsync）走人工复核状态机，不暴露通用 UpdateAsync。</para>
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public sealed class TenantGameAntiCheatEventEntityService
    : BaseService<TenantGameAntiCheatEventEntity>
{
    /// <summary>
    /// 合法的人工复核目标状态（Pending → Approved/Rejected/Escalated 单向迁移）。
    /// </summary>
    private static readonly HashSet<ManualReviewStateEnum> ValidReviewTargets =
        new()
        {
            ManualReviewStateEnum.Approved,
            ManualReviewStateEnum.Rejected,
            ManualReviewStateEnum.Escalated,
        };

    public TenantGameAntiCheatEventEntityService(SqlSugarRepository<TenantGameAntiCheatEventEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 分页查询反作弊事件（按 AppId / Provider / RiskLevel / ManualReviewState / TargetType / TargetId 过滤）。
    /// </summary>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantGameAntiCheatEventResponse>> PageAsync(
        PageTenantGameAntiCheatEventRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "反作弊事件");
        var targetId = string.IsNullOrWhiteSpace(input.TargetId) ? null : input.TargetId.Trim();
        var query = Repository.AsQueryable()
                              .Where(u => u.IsDeleted == false)
                              .Where(u => u.AppId == appId)
                              .WhereIF(input.Provider.HasValue, u => u.Provider == input.Provider.Value)
                              .WhereIF(input.RiskLevel.HasValue, u => u.RiskLevel == input.RiskLevel.Value)
                              .WhereIF(input.ManualReviewState.HasValue,
                                       u => u.ManualReviewState == input.ManualReviewState.Value)
                              .WhereIF(input.TargetType.HasValue, u => u.TargetType == input.TargetType.Value)
                              .WhereIF(targetId != null, u => u.TargetId == targetId)
                              .Select<TenantGameAntiCheatEventResponse>()
                              .OrderBy(u => u.CreatedTime, OrderByType.Desc);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 详情。
    /// </summary>
    [HttpGet(CrudActionRoutes.Detail)]
    [ApiDescriptionSettings(Name = "Detail")]
    public async Task<TenantGameAntiCheatEventResponse> DetailAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await InnerGetAsync(input);
        return entity == null ? default : entity.Adapt<TenantGameAntiCheatEventResponse>();
    }

    /// <summary>
    /// 反作弊事件上报接入（幂等）。
    /// <para><strong>幂等策略</strong>：统一走 Foundation 协调器 + 事件表即 Store（<see cref="AntiCheatEventIdempotencyStore"/>）——
    /// 占位即入账（本站点执行 == 持久化，无外部副作用），并发同键由 <c>ux_anticheat_idempotency</c> 唯一索引仲裁，
    /// 输家按 Replay 口径返回既有事件（不重复入账、不抛错）；ManualReviewState 固定为 Pending。</para>
    /// <para><strong>建议不自动生效</strong>：SuggestedAction 仅落字段，本方法不创建任何处罚记录；
    /// 处罚创建/撤销属 C11-C（C115）职责，复核通过后的映射由 C11-D-followup 承接。</para>
    /// </summary>
    [HttpPost("submit")]
    [ApiDescriptionSettings(Name = "Submit")]
    public async Task<TenantGameAntiCheatEventResponse> SubmitAsync(AddTenantGameAntiCheatEventRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "反作弊事件");
        var idempotencyKey = input.IdempotencyKey?.Trim() ?? string.Empty;
        if (idempotencyKey.Length == 0)
        {
            throw BusinessError.Oh("幂等键不能为空");
        }

        var entity = input.Adapt<TenantGameAntiCheatEventEntity>();
        entity.ManualReviewState = ManualReviewStateEnum.Pending;
#pragma warning disable CS8625 // 新建反作弊事件初始化字段为 null（DB schema 允许 null；= null! 仅满足 CS8618）
        entity.ReviewerId = null;
        entity.ReviewedTime = null;
        entity.ReviewDecision = null;
        entity.ReviewNote = null;
#pragma warning restore CS8625

        // 幂等判定统一走 Foundation 接口：协调器 + 事件表即 Store（站点规则见 Store 类备注）
        var store = new AntiCheatEventIdempotencyStore(Repository, entity);
        var coordinator = new IdempotencyCoordinator(store, SystemClock.Instance, AntiCheatEventIdempotencyStore.CreateCoordinatorOptions());
        var identifier = new IdempotencyRecordIdentifier(AntiCheatEventIdempotencyStore.IdempotencyScope, idempotencyKey);
        var decision = await coordinator.BeginAsync(new IdempotencyBeginRequest(AntiCheatEventIdempotencyStore.IdempotencyScope, idempotencyKey, AntiCheatEventIdempotencyStore.RequestDigest));
        if (decision.DecisionKind == IdempotencyDecisionKind.Replay)
        {
            // 同键重放：返回既有事件（不重复入账、不抛错）
            var existing = await Repository.GetFirstAsync(u => u.IdempotencyKey == idempotencyKey);
            return existing!.Adapt<TenantGameAntiCheatEventResponse>();
        }

        // Execute 分支：占位即入账（Conflict / Busy 不可达：常量摘要无内容冲突，占位原子无 Processing 中间态）
        await coordinator.CompleteAsync(new IdempotencyCompletionRequest(identifier, ReadOnlyMemory<byte>.Empty, 0));
        return entity.Adapt<TenantGameAntiCheatEventResponse>();
    }

    /// <summary>
    /// 人工复核动作。
    /// <para><strong>合法状态迁移</strong>：仅 Pending → Approved/Rejected/Escalated；非 Pending 拒绝重审，目标状态非法拒绝。</para>
    /// <para><strong>不写处罚表</strong>：本方法只更新事件自身的复核状态与审计字段；是否据此创建处罚属 C11-C（C115）与 C11-D-followup。</para>
    /// </summary>
    [HttpPost("review")]
    [ApiDescriptionSettings(Name = "Review")]
    public async Task ReviewAsync(UpdateTenantGameAntiCheatEventReviewRequest input)
    {
        if (!ValidReviewTargets.Contains(input.ManualReviewState))
        {
            throw BusinessError.Oh("目标复核状态非法，仅允许 Approved/Rejected/Escalated");
        }

        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("反作弊事件不存在");
        }

        if (entity.ManualReviewState != ManualReviewStateEnum.Pending)
        {
            throw BusinessError.Oh("该事件已复核，禁止重复复核");
        }

        entity.RowVersion = input.RowVersion;
        entity.ManualReviewState = input.ManualReviewState;
        entity.ReviewDecision = input.ManualReviewState.ToString();
#pragma warning disable CS8601 // input.ReviewNote 为 string?，实体 ReviewNote 字段在 DB schema 允许 null；前端可空
        entity.ReviewNote = input.ReviewNote;
#pragma warning restore CS8601
        entity.ReviewerId = TryGetCurrentUserId();
        entity.ReviewedTime = TimerHelper.UnixTimeSeconds();

        await Repository.Context.Updateable(entity)
                        .UpdateColumns(u => new
                        {
                            u.ManualReviewState,
                            u.ReviewDecision,
                            u.ReviewNote,
                            u.ReviewerId,
                            u.ReviewedTime,
                        })
                        .EnableDiffLogEvent()
                        .ExecuteCommandWithOptLockAsync(true);
    }

    private static long? TryGetCurrentUserId()
    {
        var userId = TenantOperationContext.CurrentUser?.FindFirst(ClaimConst.UserId)?.Value;
        if (long.TryParse(userId, out var parsedUserId))
        {
            return parsedUserId;
        }

        return null;
    }
}
#pragma warning restore CS8629