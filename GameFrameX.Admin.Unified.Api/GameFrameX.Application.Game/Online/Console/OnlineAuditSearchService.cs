using System.ComponentModel;
using GameFrameX.Application.Game.Online.Console.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.Online;

namespace GameFrameX.Application.Game.Online.Console;

/// <summary>
/// Online 控制台统一审计检索服务。路由前缀 /api/onlineAuditSearch（change C344 / vault:C9 S8.2）。
/// <para>
/// 三来源实时归并（Admin 侧零事件镜像、零落地表）：Admin 操作日志（<see cref="TenantLogOperationEntity"/>，
/// MonitorLogging 全 action 自动落库）+ Online 审计事件（IOnlineServerClient 实时透传）+ 各域命令事实源表
/// （处罚落账 <see cref="TenantGamePenaltyEntity"/> / 配置灰度 <see cref="TenantGameOnlineConfigRolloutEntity"/>）。
/// 三来源 → 五维统一字段的映射唯一来源是 <see cref="OnlineAuditSourceMap"/>，本 Service 不散落第二套映射。
/// </para>
/// <para>
/// OnlineAuditEvent 来源参与前置：作用域三元组是每次 Online 调用硬前置（C339），AppId + ServerId 均提供时才
/// 查询该来源，否则静默跳过（本地两来源仍支持无作用域全量检索）；该来源查询失败按异常原样透传，不软化降级——
/// 审计检索宁可失败不可静默缺源。
/// </para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 138)]
public class OnlineAuditSearchService : IAdminApiController
{
    /// <summary>
    /// Online 审计事件查询 action 名（mock 先行，联调以 vault C9 详表为准）。
    /// </summary>
    private const string QueryOnlineAuditEventsAction = "query_online_audit_events";

    private readonly SqlSugarRepository<TenantLogOperationEntity> _operationLogRepository;
    private readonly SqlSugarRepository<TenantGamePenaltyEntity> _penaltyRepository;
    private readonly SqlSugarRepository<TenantGameOnlineConfigRolloutEntity> _rolloutRepository;
    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 控制台统一审计检索服务构造函数。
    /// </summary>
    public OnlineAuditSearchService(
        SqlSugarRepository<TenantLogOperationEntity> operationLogRepository,
        SqlSugarRepository<TenantGamePenaltyEntity> penaltyRepository,
        SqlSugarRepository<TenantGameOnlineConfigRolloutEntity> rolloutRepository,
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _operationLogRepository = operationLogRepository;
        _penaltyRepository = penaltyRepository;
        _rolloutRepository = rolloutRepository;
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 统一审计检索分页：三来源各取时间倒序 top（PageSize+1）→ 统一映射 → 五维内存过滤 → 跨源 OccurredAt 倒序归并取一页。
    /// <para>
    /// 分页语义（时间倒序 keyset）：NextCursor = 本页末行 OccurredAt Unix 秒，下一页各来源只取 OccurredAt 严格小于
    /// cursor 的行。<b>同秒截断边界</b>：Unix 秒粒度下同一秒内多行被页边界截断时，下一页会跳过同秒的后续行——
    /// 秒级时间戳无法区分同秒行序，属已知精度限制，非数据丢失（重放检索用时间窗过滤代替深翻页可达同区域）。
    /// </para>
    /// <para>
    /// 深翻页近似边界：每来源只翻 top（PageSize+1）即参与归并——若某来源的命中行稀疏分散在时间深处（五维过滤
    /// 命中率低），该来源可能在未翻尽时提前不再贡献行，HasMore 随之为 false。缓解：作用域/时间/对象维度尽量
    /// SQL 预筛下推（见各来源查询），稀疏过滤请优先收窄时间窗与作用域，而非无限深翻页。
    /// </para>
    /// </summary>
    /// <param name="input">五维过滤（操作者/对象/作用域/命令类型/时间窗）+ keyset 游标 + 页大小。</param>
    /// <returns>统一审计行（OccurredAt 倒序）+ 下一页游标；HasMore=false 即翻尽。</returns>
    [HttpPost("page")]
    [ApiDescriptionSettings(Name = "Page")]
    [DisplayName("统一审计检索分页")]
    public async Task<PageOnlineAuditSearchResponse> PageAsync(PageOnlineAuditSearchRequest input)
    {
        var cursorSeconds = ResolveCursor(input.Cursor);

        var candidates = new List<OnlineAuditSearchEntryResponse>();
        candidates.AddRange(await QueryAdminOperationSourceAsync(input, cursorSeconds));
        candidates.AddRange(await QueryOnlineAuditEventSourceAsync(input, cursorSeconds));
        candidates.AddRange(await QueryDomainCommandSourceAsync(input, cursorSeconds));

        var ordered = candidates
            // 统一 keyset 兜底：A/C 来源的 SQL 预筛与 B 来源的线缆 EndTime 下推都是优化，
            // 「OccurredAt 严格小于 cursor」的正确性在归并处统一保证（线缆侧未按契约过滤时不重页）。
            .Where(entry => (!cursorSeconds.HasValue || entry.OccurredAt < cursorSeconds.Value) && MatchesFilters(entry, input))
            .OrderByDescending(entry => entry.OccurredAt)
            .ToList();
        var hasMore = ordered.Count > input.PageSize;
        var page = hasMore ? ordered.Take(input.PageSize).ToList() : ordered;
        return new PageOnlineAuditSearchResponse
        {
            Items = page,
            NextCursor = hasMore && page.Count > 0 ? page[page.Count - 1].OccurredAt.ToString() : null,
            HasMore = hasMore,
        };
    }

    /// <summary>
    /// 解析 keyset 游标为 Unix 秒；非空但非数字的游标按参数校验拒绝（脏游标静默当首页会翻页错乱）。
    /// </summary>
    private static long? ResolveCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        if (!long.TryParse(cursor, out var parsed))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "Cursor", });
        }

        return parsed;
    }

    /// <summary>
    /// A 来源：Admin 操作日志。时间窗 / 操作者账号 / 目标值出现（RequestParam LIKE 预筛）SQL 下推，
    /// 目标标识精确值与作用域维度经 <see cref="OnlineAuditSourceMap"/> 提取后在内存过滤兜底。
    /// </summary>
    private async Task<List<OnlineAuditSearchEntryResponse>> QueryAdminOperationSourceAsync(PageOnlineAuditSearchRequest input, long? cursorSeconds)
    {
        var rows = await _operationLogRepository.AsQueryable()
                         .WhereIF(input.StartTime.HasValue, u => (u.LogDateTime ?? 0) >= input.StartTime.Value)
                         .WhereIF(input.EndTime.HasValue, u => (u.LogDateTime ?? 0) <= input.EndTime.Value)
                         .WhereIF(cursorSeconds.HasValue, u => (u.LogDateTime ?? 0) < cursorSeconds.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Operator), u => (u.Account ?? string.Empty).Contains(input.Operator) || (u.RealName ?? string.Empty).Contains(input.Operator))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.TargetPlayerId), u => (u.RequestParam ?? string.Empty).Contains(input.TargetPlayerId))
                         .OrderByDescending(u => u.LogDateTime ?? 0)
                         .Take(input.PageSize + 1)
                         .ToListAsync();
        return rows.Select(OnlineAuditSourceMap.FromAdminOperation).ToList();
    }

    /// <summary>
    /// B 来源：Online 审计事件实时透传。仅 AppId + ServerId 均提供（Online 调用硬前置）时参与，否则返回空集。
    /// <para>
    /// 统一 keyset 下推方式：不启用 Online 原生游标，把 cursor 折进时间窗终点（整数秒下 EndTime = cursor - 1
    /// 与严格小于 cursor 精确等价）；作用域回填：事件行的 AppId/ServerId 即本次查询锚定的作用域。
    /// </para>
    /// </summary>
    private async Task<List<OnlineAuditSearchEntryResponse>> QueryOnlineAuditEventSourceAsync(PageOnlineAuditSearchRequest input, long? cursorSeconds)
    {
        if (!input.AppId.HasValue || !input.ServerId.HasValue)
        {
            // 作用域未锚定：本地两来源仍全量检索，本来源静默跳过（非错误）。
            return new List<OnlineAuditSearchEntryResponse>();
        }

        var scope = await _scopeContextFactory.CreateAsync(input.AppId.Value, input.ServerId.Value, "audit-search");
        var data = await _onlineServerClient.PostAsync<OnlineAuditEventQueryResultResponse>(QueryOnlineAuditEventsAction, new OnlineAuditEventQueryRequest
        {
            OperatorId = input.Operator,
            TargetPlayerId = input.TargetPlayerId,
            CommandKind = input.CommandKind,
            StartTime = input.StartTime,
            EndTime = ResolveOnlineWindowEnd(input.EndTime, cursorSeconds),
            Cursor = null,
            PageSize = input.PageSize + 1,
        }, scope);

        var mapped = data.Items.Select(OnlineAuditSourceMap.FromOnlineAuditEvent).ToList();
        foreach (var entry in mapped)
        {
            // 回填查询锚定作用域：Online 事件线缆不重复携带作用域，归属即本次查询锚点。
            entry.AppId = input.AppId;
            entry.ServerId = input.ServerId;
        }

        return mapped;
    }

    /// <summary>
    /// Online 事件时间窗终点：cursor 折进窗口（cursor - 1 使闭区间 <= 与严格 < cursor 在整数秒下精确等价），
    /// 与显式 EndTime 同时存在时取更早者。
    /// </summary>
    private static long? ResolveOnlineWindowEnd(long? endTime, long? cursorSeconds)
    {
        if (!cursorSeconds.HasValue)
        {
            return endTime;
        }

        var cursorExclusive = cursorSeconds.Value - 1;
        if (!endTime.HasValue)
        {
            return cursorExclusive;
        }

        return Math.Min(endTime.Value, cursorExclusive);
    }

    /// <summary>
    /// C 来源：各域命令事实源表（处罚落账 + 配置灰度发布记录），只读查询。作用域 / 时间 / 目标（处罚表可精确）SQL 下推。
    /// </summary>
    private async Task<List<OnlineAuditSearchEntryResponse>> QueryDomainCommandSourceAsync(PageOnlineAuditSearchRequest input, long? cursorSeconds)
    {
        var penalties = await _penaltyRepository.AsQueryable()
                          .WhereIF(input.AppId.HasValue, u => u.AppId == input.AppId.Value)
                          .WhereIF(input.ServerId.HasValue, u => u.AreaId == input.ServerId.Value)
                          .WhereIF(!string.IsNullOrWhiteSpace(input.TargetPlayerId), u => u.TargetId == input.TargetPlayerId)
                          .WhereIF(input.StartTime.HasValue, u => u.CreatedTime >= input.StartTime.Value)
                          .WhereIF(input.EndTime.HasValue, u => u.CreatedTime <= input.EndTime.Value)
                          .WhereIF(cursorSeconds.HasValue, u => u.CreatedTime < cursorSeconds.Value)
                          .OrderByDescending(u => u.CreatedTime)
                          .Take(input.PageSize + 1)
                          .ToListAsync();

        var rollouts = await _rolloutRepository.AsQueryable()
                         .WhereIF(input.AppId.HasValue, u => u.AppId == input.AppId.Value)
                         .WhereIF(input.ServerId.HasValue, u => u.ServerId == input.ServerId.Value)
                         .WhereIF(input.StartTime.HasValue, u => u.PublishedTime >= input.StartTime.Value)
                         .WhereIF(input.EndTime.HasValue, u => u.PublishedTime <= input.EndTime.Value)
                         .WhereIF(cursorSeconds.HasValue, u => u.PublishedTime < cursorSeconds.Value)
                         .OrderByDescending(u => u.PublishedTime)
                         .Take(input.PageSize + 1)
                         .ToListAsync();

        var entries = penalties.Select(OnlineAuditSourceMap.FromPenaltyRecord).ToList();
        entries.AddRange(rollouts.Select(OnlineAuditSourceMap.FromConfigRolloutRecord));
        return entries;
    }

    /// <summary>
    /// 五维统一过滤（对象维度兜底：SQL 预筛只能缩小超集，精确命中以映射后的统一字段为准）。
    /// <para>操作者 / 命令类型为 substring（忽略大小写）匹配；目标对象为精确匹配（Id 语义）；作用域为相等匹配。</para>
    /// </summary>
    private static bool MatchesFilters(OnlineAuditSearchEntryResponse entry, PageOnlineAuditSearchRequest input)
    {
        if (!string.IsNullOrWhiteSpace(input.Operator) && !entry.OperatorIdentifier.Contains(input.Operator, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(input.TargetPlayerId) && !string.Equals(entry.TargetPlayerIdentifier, input.TargetPlayerId, StringComparison.Ordinal))
        {
            return false;
        }

        if (input.AppId.HasValue && entry.AppId != input.AppId.Value)
        {
            return false;
        }

        if (input.ServerId.HasValue && entry.ServerId != input.ServerId.Value)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(input.CommandKind) && !entry.CommandType.Contains(input.CommandKind, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
