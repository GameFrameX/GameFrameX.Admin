using System.Globalization;
using System.Threading;
using GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Game;

namespace GameFrameX.Application.ServerCenter.GameAreaMerge;

/// <summary>合服计划、预演、双人复核及同步执行编排。</summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 120)]
public class GameAreaMergeService : IAdminApiController
{
    // 本宿主串行预留区服，执行仍通过持久化状态比较防重复触发。
    private static readonly SemaphoreSlim PlanGate = new(1, 1);
    private readonly SqlSugarRepository<GameAreaMergePlanEntity> _repository;
    private readonly SqlSugarRepository<TenantGameAreaEntity> _areaRepository;
    private readonly GameAreaMergeClient _client;
    private readonly CurrentUserContext _currentUser;

    public GameAreaMergeService(SqlSugarRepository<GameAreaMergePlanEntity> repository,
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
    public Task<SqlSugarPagedList<GameAreaMergePlanResponse>> PageAsync(BasePageRequest input)
    {
        return _repository.AsQueryable().OrderByDescending(plan => plan.Id)
            .Select<GameAreaMergePlanResponse>().ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>读取计划与两个阶段的完整报告快照。</summary>
    [HttpGet(CrudActionRoutes.Detail)]
    [DisplayName("合服计划详情")]
    public async Task<GameAreaMergePlanResponse> DetailAsync([FromQuery] BaseIdRequest input)
    {
        return (await FindAsync(input.Id)).Adapt<GameAreaMergePlanResponse>();
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
            var activePlans = await _repository.AsQueryable().Where(plan => plan.Status != GameAreaMergePlanStatus.Completed).ToListAsync();
            if (activePlans.Any(plan => Sources(plan).Append(plan.TargetServerId).Intersect(participants).Any()))
            {
                throw BusinessError.Oh(AreaMergeErrorCode.SourceReserved);
            }
            var entity = new GameAreaMergePlanEntity
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
            return entity.Adapt<GameAreaMergePlanResponse>();
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
            return entity.Adapt<GameAreaMergePlanResponse>();
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
            return entity.Adapt<GameAreaMergePlanResponse>();
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
            return entity.Adapt<GameAreaMergePlanResponse>();
        }
        EnsureState(entity, GameAreaMergePlanStatus.Approved);
        if (!input.BackupConfirmed || string.IsNullOrWhiteSpace(input.BackupIdentifier))
        {
            throw BusinessError.Oh(AreaMergeErrorCode.BackupRequired);
        }
        entity.BackupIdentifier = input.BackupIdentifier.Trim();
        entity.BackupConfirmed = true;
        entity.ExecutionStartedTime = Now();
        var claimed = await _repository.Context.Updateable<GameAreaMergePlanEntity>()
            .SetColumns(plan => new GameAreaMergePlanEntity
            {
                Status = GameAreaMergePlanStatus.Executing,
                BackupIdentifier = entity.BackupIdentifier,
                BackupConfirmed = true,
                ExecutionStartedTime = entity.ExecutionStartedTime,
            }).Where(plan => plan.Id == entity.Id && plan.Status == GameAreaMergePlanStatus.Approved).ExecuteCommandAsync();
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
        return entity.Adapt<GameAreaMergePlanResponse>();
    }

    private async Task<GameAreaMergePlanEntity> FindAsync(long identifier)
    {
        return await _repository.GetFirstAsync(plan => plan.Id == identifier)
            ?? throw BusinessError.Oh(AreaMergeErrorCode.PlanNotFound);
    }

    private Task<int> PersistAsync(GameAreaMergePlanEntity entity)
    {
        return _repository.Context.Updateable(entity).ExecuteCommandAsync();
    }

    private async Task FailAsync(GameAreaMergePlanEntity entity)
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

    private static long[] Sources(GameAreaMergePlanEntity entity) => entity.SourceServerIds.Split(',').Select(value => long.Parse(value, CultureInfo.InvariantCulture)).ToArray();

    private static void EnsureState(GameAreaMergePlanEntity entity, params GameAreaMergePlanStatus[] allowed)
    {
        if (!allowed.Contains(entity.Status))
        {
            throw BusinessError.Oh(AreaMergeErrorCode.InvalidState);
        }
    }

    private static void ValidateReport(GameAreaMergePlanEntity entity, GameAreaMergeReportResponse report, bool requirePlan)
    {
        if (string.IsNullOrWhiteSpace(report.PlanId) || report.TargetServerId != entity.TargetServerId
            || report.SourceServerIds == null || !report.SourceServerIds.OrderBy(identifier => identifier).SequenceEqual(Sources(entity))
            || (requirePlan && report.PlanId != entity.PlanId))
        {
            throw BusinessError.Oh(AreaMergeErrorCode.InvalidReport);
        }
    }
}
