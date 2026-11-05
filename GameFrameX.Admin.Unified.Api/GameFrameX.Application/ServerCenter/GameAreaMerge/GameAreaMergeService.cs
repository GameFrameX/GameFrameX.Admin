using GameFrameX.Entity.Tenant.Game;
using System.Globalization;
using System.Threading;
using GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.ServerCenter.GameAreaMerge;

/// <summary>合服计划、预演、双人复核及同步执行编排。</summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 120)]
public class GameAreaMergeService : IAdminApiController
{
    // 本宿主串行预留区服，执行仍通过持久化状态比较防重复触发。
    private static readonly SemaphoreSlim PlanGate = new(1, 1);
    // ponytail: 300 秒宽限是固定值，当前上限为 300 秒；若执行链路需要更长缓冲，升级时应改为配置项并同步状态机与验收契约。
    private const int ExecutionReclaimGraceSeconds = 300;
    private readonly SqlSugarRepository<TenantGameAreaMergePlanEntity> _repository;
    private readonly SqlSugarRepository<TenantGameAreaEntity> _areaRepository;
    private readonly GameAreaMergeClient _client;
    private readonly CurrentUserContext _currentUser;

    public GameAreaMergeService(SqlSugarRepository<TenantGameAreaMergePlanEntity> repository,
        SqlSugarRepository<TenantGameAreaEntity> areaRepository, GameAreaMergeClient client, CurrentUserContext currentUser)
    {
        _repository = repository;
        _areaRepository = areaRepository;
        _client = client;
        _currentUser = currentUser;
    }

    /// <summary>查询合服计划，最新计划优先。</summary>
    [HttpPost(CrudActionRoutes.Page)]
    [DisplayName("查询合服计划")]
    public async Task<SqlSugarPagedList<GameAreaMergePlanResponse>> PageAsync(BasePageRequest input)
    {
        var page = await _repository.AsQueryable().OrderByDescending(plan => plan.Id)
            .ToPagedListAsync(input.Page, input.PageSize);
        return new SqlSugarPagedList<GameAreaMergePlanResponse>
        {
            Page = page.Page,
            PageSize = page.PageSize,
            Total = page.Total,
            TotalPages = page.TotalPages,
            Items = page.Items.Select(ToResponse).ToList(),
            HasPrevPage = page.HasPrevPage,
            HasNextPage = page.HasNextPage,
        };
    }

    /// <summary>读取计划与两个阶段的完整报告快照。</summary>
    [HttpGet(CrudActionRoutes.Detail)]
    [DisplayName("合服计划详情")]
    public async Task<GameAreaMergePlanResponse> DetailAsync([FromQuery] BaseIdRequest input)
    {
        return ToResponse(await FindAsync(input.Id));
    }

    /// <summary>取消尚未进入执行阶段的合服计划，释放参与区服。</summary>
    [HttpPost("cancel")]
    [DisplayName("取消合服计划")]
    public async Task<GameAreaMergePlanResponse> CancelAsync(CancelGameAreaMergePlanRequest input)
    {
        RequireOperator();
        RequireReason(input.Reason);
        var cancelledTime = Now();
        var cancelled = await _repository.Context.Updateable<TenantGameAreaMergePlanEntity>()
            .SetColumns(plan => new TenantGameAreaMergePlanEntity
            {
                Status = GameAreaMergePlanStatus.Cancelled,
                CancelledByOperatorId = _currentUser.UserId,
                CancelledTime = cancelledTime,
                CancellationReason = input.Reason.Trim(),
            })
            .Where(plan => plan.Id == input.Id
                && plan.ExecutionStartedTime == null
                && (plan.Status == GameAreaMergePlanStatus.Draft
                    || plan.Status == GameAreaMergePlanStatus.DryRunCompleted
                    || plan.Status == GameAreaMergePlanStatus.Approved
                    || plan.Status == GameAreaMergePlanStatus.Failed))
            .ExecuteCommandAsync();
        if (cancelled != 1)
        {
            throw BusinessError.Oh(AreaMergeErrorCode.CancellationNotAllowed);
        }

        return ToResponse(await FindAsync(input.Id));
    }

    /// <summary>验证区服及占用关系，在目标服生成合服计划。</summary>
    [HttpPost(CrudActionRoutes.Add)]
    [DisplayName("新建合服计划")]
    public async Task<GameAreaMergePlanResponse> AddAsync(CreateGameAreaMergePlanRequest input)
    {
        if (input.SourceServerIds == null || input.SourceServerIds.Count == 0 || input.SourceServerIds.Count > 100
            || input.TargetServerId <= 0 || input.SourceServerIds.Any(source => source <= 0 || source == input.TargetServerId)
            || input.SourceServerIds.Distinct().Count() != input.SourceServerIds.Count)
        {
            throw BusinessError.Oh(AreaMergeErrorCode.InvalidServers);
        }
        RequireOperator();
        RequireReason(input.Reason);
        await PlanGate.WaitAsync();
        try
        {
            var participants = input.SourceServerIds.Append(input.TargetServerId).ToArray();
            var areas = await _areaRepository.AsQueryable().Filter(null, true)
                .Where(area => participants.Contains(area.Id) && area.IsDeleted == false).ToListAsync();
            if (areas.Count != participants.Length || areas.Any(area => area.ServerStatus == GameServerStatus.Merged || area.IsEnabled != true))
            {
                throw BusinessError.Oh(AreaMergeErrorCode.InvalidServers);
            }
            var activePlans = await _repository.AsQueryable()
                .Where(plan => plan.Status != GameAreaMergePlanStatus.Completed && plan.Status != GameAreaMergePlanStatus.Cancelled)
                .ToListAsync();
            if (activePlans.Any(plan => Sources(plan).Append(plan.TargetServerId).Intersect(participants).Any()))
            {
                throw BusinessError.Oh(AreaMergeErrorCode.SourceReserved);
            }
            var entity = new TenantGameAreaMergePlanEntity
            {
                TargetServerId = input.TargetServerId,
                SourceServerIds = string.Join(",", input.SourceServerIds.OrderBy(identifier => identifier)),
                ApplicantOperatorId = _currentUser.UserId,
                Reason = input.Reason.Trim(),
                Status = GameAreaMergePlanStatus.Draft,
                CreatedTime = Now(),
                IsDeleted = false,
            };
            var report = await _client.DispatchAsync("plan_server_merge", entity.TargetServerId, entity.SourceServerIds);
            ValidateReport(entity, report, false);
            entity.PlanId = report.PlanId;
            await _repository.InsertWithDiffLogAsync(entity);
            return ToResponse(entity);
        }
        finally
        {
            PlanGate.Release();
        }
    }

    /// <summary>仅草稿或失败计划可重新预演；保存报告供复核。</summary>
    [HttpPost("dryRun")]
    [DisplayName("预演合服计划")]
    public async Task<GameAreaMergePlanResponse> DryRunAsync(BaseIdRequest input)
    {
        await PlanGate.WaitAsync();
        try
        {
            var entity = await FindAsync(input.Id);
            EnsureState(entity, GameAreaMergePlanStatus.Draft, GameAreaMergePlanStatus.Failed);
            try
            {
                var report = await _client.DispatchAsync("dry_run_server_merge", entity.TargetServerId, entity.SourceServerIds);
                ValidateReport(entity, report, true);
                if (!report.DryRun)
                {
                    throw BusinessError.Oh(AreaMergeErrorCode.InvalidReport);
                }
                // ponytail: ExecutionStartedTime 只记录是否曾进入执行，重新预演不得清除，否则失败计划会错误恢复为可取消。
                entity.DryRunReportSnapshot = JsonSerializer.Serialize(report);
                entity.Status = GameAreaMergePlanStatus.DryRunCompleted;
                entity.DryRunTime = Now();
                entity.ReviewerOperatorId = null;
                entity.ApprovedTime = null;
                entity.BackupConfirmed = false;
                entity.BackupIdentifier = null;
                entity.FailureMessage = null;
                await PersistAsync(entity);
            }
            catch (Exception)
            {
                await FailAsync(entity);
                throw;
            }
            return ToResponse(entity);
        }
        finally
        {
            PlanGate.Release();
        }
    }

    /// <summary>当前登录复核人确认零全局域写入；申请人不得自批。</summary>
    [HttpPost("approve")]
    [DisplayName("批准合服计划")]
    public async Task<GameAreaMergePlanResponse> ApproveAsync(ApproveGameAreaMergePlanRequest input)
    {
        await PlanGate.WaitAsync();
        try
        {
            var entity = await FindAsync(input.Id);
            EnsureState(entity, GameAreaMergePlanStatus.DryRunCompleted);
            RequireReason(input.Reason);
            if (input.ReviewerOperatorId <= 0 || input.ReviewerOperatorId == entity.ApplicantOperatorId
                || input.ReviewerOperatorId != _currentUser.UserId)
            {
                throw BusinessError.Oh(AreaMergeErrorCode.ReviewRequired);
            }
            var report = JsonSerializer.Deserialize<GameAreaMergeReportResponse>(entity.DryRunReportSnapshot ?? "null");
            if (report == null || !report.DryRun)
            {
                throw BusinessError.Oh(AreaMergeErrorCode.InvalidReport);
            }
            ValidateReport(entity, report, true);
            if (report.GlobalDomainRowsTouched != 0)
            {
                throw BusinessError.Oh(AreaMergeErrorCode.GlobalDomainTouched);
            }
            entity.ReviewerOperatorId = input.ReviewerOperatorId;
            entity.Reason = input.Reason.Trim();
            entity.ApprovedTime = Now();
            entity.Status = GameAreaMergePlanStatus.Approved;
            await PersistAsync(entity);
            return ToResponse(entity);
        }
        finally
        {
            PlanGate.Release();
        }
    }

    /// <summary>备份确认后同步执行；成功或幂等回放时原子完成计划与源服退场。</summary>
    [HttpPost("execute")]
    [DisplayName("执行合服计划")]
    public async Task<GameAreaMergePlanResponse> ExecuteAsync(ExecuteGameAreaMergePlanRequest input)
    {
        var entity = await FindAsync(input.Id);
        if (entity.Status == GameAreaMergePlanStatus.Completed)
        {
            return ToResponse(entity);
        }
        if (entity.Status != GameAreaMergePlanStatus.Approved && !IsExecutionReclaimable(entity))
        {
            throw BusinessError.Oh(AreaMergeErrorCode.InvalidState);
        }
        if (!input.BackupConfirmed || string.IsNullOrWhiteSpace(input.BackupIdentifier))
        {
            throw BusinessError.Oh(AreaMergeErrorCode.BackupRequired);
        }
        entity.BackupIdentifier = input.BackupIdentifier.Trim();
        entity.BackupConfirmed = true;
        var executionStartedTime = Now();
        var reclaimCutoff = executionStartedTime - _client.ExecutionTimeoutSeconds - ExecutionReclaimGraceSeconds;
        entity.ExecutionStartedTime = executionStartedTime;
        var claimed = await _repository.Context.Updateable<TenantGameAreaMergePlanEntity>()
            .SetColumns(plan => new TenantGameAreaMergePlanEntity
            {
                Status = GameAreaMergePlanStatus.Executing,
                BackupIdentifier = entity.BackupIdentifier,
                BackupConfirmed = true,
                ExecutionStartedTime = executionStartedTime,
            }).Where(plan => plan.Id == entity.Id
                && (plan.Status == GameAreaMergePlanStatus.Approved
                    || (plan.Status == GameAreaMergePlanStatus.Executing
                        && plan.ExecutionStartedTime != null
                        && plan.ExecutionStartedTime <= reclaimCutoff))).ExecuteCommandAsync();
        if (claimed != 1)
        {
            throw BusinessError.Oh(AreaMergeErrorCode.InvalidState);
        }
        entity.Status = GameAreaMergePlanStatus.Executing;
        try
        {
            // OnlineServerClient 不消费 RequestAborted；专属 HttpClient 配置长超时。
            var report = await _client.DispatchAsync("execute_server_merge", entity.TargetServerId, entity.SourceServerIds);
            ValidateReport(entity, report, true);
            if (report.DryRun || report.GlobalDomainRowsTouched != 0)
            {
                throw BusinessError.Oh(AreaMergeErrorCode.InvalidReport);
            }
            // AlreadyExecuted 与首次执行同样收口，本地上次写回失败也可安全重放。
            entity.ExecutionReportSnapshot = JsonSerializer.Serialize(report);
            entity.Status = GameAreaMergePlanStatus.Completed;
            entity.CompletedTime = Now();
            entity.FailureMessage = null;
            var sources = Sources(entity);
            await _repository.Context.Ado.BeginTranAsync();
            try
            {
                await _repository.Context.Updateable<TenantGameAreaEntity>().SetColumns(area => new TenantGameAreaEntity
                {
                    ServerStatus = GameServerStatus.Merged,
                    MergedIntoServerId = entity.TargetServerId,
                    IsEnabled = false,
                }).Where(area => sources.Contains(area.Id) && area.IsDeleted == false).ExecuteCommandAsync();
                await PersistAsync(entity);
                await _repository.Context.Ado.CommitTranAsync();
            }
            catch
            {
                await _repository.Context.Ado.RollbackTranAsync();
                throw;
            }
        }
        catch (Exception)
        {
            await FailAsync(entity);
            throw;
        }
        return ToResponse(entity);
    }

    private async Task<TenantGameAreaMergePlanEntity> FindAsync(long identifier)
    {
        return await _repository.GetFirstAsync(plan => plan.Id == identifier)
            ?? throw BusinessError.Oh(AreaMergeErrorCode.PlanNotFound);
    }

    private Task<int> PersistAsync(TenantGameAreaMergePlanEntity entity)
    {
        return _repository.Context.Updateable(entity).ExecuteCommandAsync();
    }

    private async Task FailAsync(TenantGameAreaMergePlanEntity entity)
    {
        entity.Status = GameAreaMergePlanStatus.Failed;
        entity.FailedTime = Now();
        // 不将含 URL/凭证的底层异常保存为可见报告。
        entity.FailureMessage = ((int)AreaMergeErrorCode.DispatchFailed).ToString(CultureInfo.InvariantCulture);
        await PersistAsync(entity);
    }

    private void RequireOperator()
    {
        if (_currentUser.UserId <= 0)
        {
            throw BusinessError.Oh(AreaMergeErrorCode.ReviewRequired);
        }
    }

    private static void RequireReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw BusinessError.Oh(AreaMergeErrorCode.ReviewRequired);
        }
    }

    private static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private bool IsExecutionReclaimable(TenantGameAreaMergePlanEntity entity)
    {
        if (entity.Status != GameAreaMergePlanStatus.Executing || !entity.ExecutionStartedTime.HasValue)
        {
            return false;
        }

        var reclaimCutoff = Now() - _client.ExecutionTimeoutSeconds - ExecutionReclaimGraceSeconds;
        return entity.ExecutionStartedTime.Value <= reclaimCutoff;
    }

    private GameAreaMergePlanResponse ToResponse(TenantGameAreaMergePlanEntity entity)
    {
        var response = entity.Adapt<GameAreaMergePlanResponse>();
        response.Cancellable = entity.ExecutionStartedTime == null
            && (entity.Status == GameAreaMergePlanStatus.Draft
                || entity.Status == GameAreaMergePlanStatus.DryRunCompleted
                || entity.Status == GameAreaMergePlanStatus.Approved
                || entity.Status == GameAreaMergePlanStatus.Failed);
        response.ExecutionReclaimable = IsExecutionReclaimable(entity);
        return response;
    }

    private static long[] Sources(TenantGameAreaMergePlanEntity entity) => entity.SourceServerIds.Split(',').Select(value => long.Parse(value, CultureInfo.InvariantCulture)).ToArray();

    private static void EnsureState(TenantGameAreaMergePlanEntity entity, params GameAreaMergePlanStatus[] allowed)
    {
        if (!allowed.Contains(entity.Status))
        {
            throw BusinessError.Oh(AreaMergeErrorCode.InvalidState);
        }
    }

    private static void ValidateReport(TenantGameAreaMergePlanEntity entity, GameAreaMergeReportResponse report, bool requirePlan)
    {
        if (string.IsNullOrWhiteSpace(report.PlanId) || report.TargetServerId != entity.TargetServerId
            || report.SourceServerIds == null || !report.SourceServerIds.OrderBy(identifier => identifier).SequenceEqual(Sources(entity))
            || (requirePlan && report.PlanId != entity.PlanId))
        {
            throw BusinessError.Oh(AreaMergeErrorCode.InvalidReport);
        }
    }
}
