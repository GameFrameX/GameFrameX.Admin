using GameFrameX.Core.Job;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.SqlSugar;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace GameFrameX.Application.Platform.Job;

/// <summary>
/// 业务定时任务扫描派发作业（调度合一）。
/// <para>接替已退役的 <c>ScheduledTaskHostedService</c>：周期扫描 <see cref="GameTimeJobEntity"/> 全表，
/// 按行的 cron（NCrontab 含秒 6 段）到期执行。语义与旧引擎对齐：</para>
/// <list type="bullet">
/// <item>首次见到的行 <c>NextRun = GetNextOccurrence(当下)</c>——停机错过的时间窗不补跑。</item>
/// <item><c>IsOnce</c> 行执行成功后删行，只执行一次。</item>
/// <item>行的 Cron/IsOnce/MailId/Name 变更（指纹比对）后重置 NextRun——增删改在一个扫描周期内生效
/// （旧引擎需重启，此为任务要求的增强）。</item>
/// <item><c>Name</c> 解析不到 <see cref="IScheduleJob"/> 实现时告警并推进 NextRun（旧引擎会因此空转刷日志，此处修正）。</item>
/// <item><c>MailId</c> 仍经 <see cref="SystemCacheService"/> 的 <see cref="MailIdCacheKey"/> 键传给任务实现（与旧引擎同一通道）。</item>
/// </list>
/// </summary>
[JobDetail("job_gameTimeJobDispatch", Description = "业务定时任务扫描派发", GroupName = "default", Concurrent = false)]
[PeriodSeconds(ScanIntervalSeconds, TriggerId = "trigger_gameTimeJobDispatch", Description = "业务定时任务扫描派发", RunOnStart = true)]
public class GameTimeJobDispatchJob : IJob
{
    /// <summary>
    /// 扫描周期（秒）。到期执行的判定粒度受此周期影响（旧引擎为秒级精确等待，合一后最大延迟约一个扫描周期）。
    /// </summary>
    public const int ScanIntervalSeconds = 5;

    /// <summary>
    /// 派发侧与业务任务实现之间传递邮件 Id 的缓存键（旧引擎遗留通道，键值不可变）。
    /// </summary>
    public const string MailIdCacheKey = "MailId";

    private static readonly Dictionary<long, TrackedBusinessTask> TrackedTasks = new();
    private static readonly SemaphoreSlim ExecuteLock = new(1, 1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GameTimeJobDispatchJob> _logger;

    /// <summary>
    /// 业务任务类型解析器（按 <c>Name</c> 解析实现 <see cref="IScheduleJob"/> 的 CLR 类型）。
    /// 可替换以便单测注入探针类型。
    /// </summary>
    internal static Func<string, Type?> BusinessJobTypeResolver { get; set; } = DefaultBusinessJobTypeResolver;

    /// <summary>
    /// 时钟（到期判定与 NextRun 推进用）。可替换以便单测推进时间。
    /// </summary>
    internal static Func<DateTime> Clock { get; set; } = DefaultClock;

    public GameTimeJobDispatchJob(IServiceScopeFactory scopeFactory, ILogger<GameTimeJobDispatchJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        // 引擎侧 Concurrent=false 已保证不重叠；信号量兜底手动执行（任务调度页 RunJob）与扫描并发的边界。
        if (!await ExecuteLock.WaitAsync(0, stoppingToken))
        {
            return;
        }

        try
        {
            await ScanAndExecuteAsync(stoppingToken);
        }
        finally
        {
            ExecuteLock.Release();
        }
    }

    private async Task ScanAndExecuteAsync(CancellationToken cancellationToken)
    {
        List<GameTimeJobEntity> rows;
        using (var scope = _scopeFactory.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            // 与旧引擎同口径：走全局自动过滤器（启用行 / 未删除行）。
            rows = await database.Queryable<GameTimeJobEntity>().ToListAsync(cancellationToken);
        }

        var dueTasks = ReconcileTracking(rows, Clock());
        if (dueTasks.Count == 0)
        {
            return;
        }

        // 与旧引擎一致：到期任务并行执行。
        await Task.WhenAll(dueTasks.Select(task => ExecuteBusinessTaskAsync(task, cancellationToken)));
    }

    /// <summary>
    /// 对账内存跟踪与表数据：新行初始化、变更行重置、消失行移除；返回本次到期的任务。
    /// </summary>
    private List<TrackedBusinessTask> ReconcileTracking(List<GameTimeJobEntity> rows, DateTime now)
    {
        var dueTasks = new List<TrackedBusinessTask>();

        foreach (var row in rows)
        {
            var fingerprint = BuildFingerprint(row);
            if (!TrackedTasks.TryGetValue(row.Id, out var tracked))
            {
                tracked = new TrackedBusinessTask { RowId = row.Id, };
                TrackedTasks[row.Id] = tracked;
            }

            if (tracked.Fingerprint != fingerprint)
            {
                // 首次见到或业务字段变更：从当下重新起算（cron 变更后旧 NextRun 失效）。
                tracked.Fingerprint = fingerprint;
                tracked.NextRun = GetNextOccurrence(row.Cron, now);
                if (tracked.NextRun == null)
                {
                    _logger.LogWarning("定时任务 {Description}（Id={RowId}）的 Cron 表达式无效：{Cron}，跳过该行",
                                       row.Description, row.Id, row.Cron);
                }
            }

            if (tracked.NextRun != null && tracked.NextRun <= now)
            {
                tracked.CurrentRow = row;
                dueTasks.Add(tracked);
            }
        }

        var liveRowIds = new HashSet<long>(rows.Select(row => row.Id));
        var staleRowIds = TrackedTasks.Keys.Where(rowId => !liveRowIds.Contains(rowId)).ToList();
        foreach (var staleRowId in staleRowIds)
        {
            TrackedTasks.Remove(staleRowId);
        }

        return dueTasks;
    }

    private async Task ExecuteBusinessTaskAsync(TrackedBusinessTask tracked, CancellationToken cancellationToken)
    {
        var row = tracked.CurrentRow!;
        tracked.CurrentRow = null;

        try
        {
            _logger.LogInformation("开始执行定时任务: {Description}", row.Description);

            using (var scope = _scopeFactory.CreateScope())
            {
                var jobType = BusinessJobTypeResolver(row.Name);
                if (jobType == null || !typeof(IScheduleJob).IsAssignableFrom(jobType))
                {
                    // 无论解析成败都推进 NextRun：解析失败的行不得每轮到期（旧引擎空转缺陷的修正）。
                    tracked.NextRun = GetNextOccurrence(row.Cron, Clock());
                    _logger.LogWarning("定时任务 {Description} 的名称解析不到 IScheduleJob 实现: {Name}",
                                       row.Description, row.Name);
                    return;
                }

                var job = scope.ServiceProvider.GetService(jobType) as IScheduleJob;
                if (job == null)
                {
                    tracked.NextRun = GetNextOccurrence(row.Cron, Clock());
                    _logger.LogWarning("定时任务 {Description} 的实现未注册到 DI 容器: {Name}", row.Description, row.Name);
                    return;
                }

                var systemCacheService = scope.ServiceProvider.GetRequiredService<SystemCacheService>();
                systemCacheService.Set(MailIdCacheKey, row.MailId > 0 ? row.MailId : 0);

                var innerTask = await job.ExecuteAsync(cancellationToken);
                if (innerTask != null)
                {
                    await innerTask;
                }

                if (row.IsOnce && row.Id > 0)
                {
                    var repository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<GameTimeJobEntity>>();
                    await repository.DeleteByIdAsync(row.Id);
                    TrackedTasks.Remove(row.Id);
                    _logger.LogInformation("完成执行一次性定时任务并删除记录: {Description}", row.Description);
                    return;
                }
            }

            tracked.NextRun = GetNextOccurrence(row.Cron, Clock());
            _logger.LogInformation("完成执行定时任务: {Description}, 下一次执行时间: {NextRun}", row.Description, tracked.NextRun);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("定时任务 {Description} 已取消", row.Description);
        }
        catch (Exception exception)
        {
            // 旧引擎异常后不推进 NextRun 会陷入立即重试的死循环；此处推进到下一个周期（错过即错过）。
            tracked.NextRun = GetNextOccurrence(row.Cron, Clock());
            _logger.LogError(exception, "执行定时任务 {Description} 时发生异常，下一次执行时间: {NextRun}", row.Description, tracked.NextRun);
        }
    }

    private DateTime? GetNextOccurrence(string cronExpression, DateTime from)
    {
        try
        {
            var schedule = CrontabSchedule.Parse(cronExpression, new CrontabSchedule.ParseOptions { IncludingSeconds = true, });
            return schedule.GetNextOccurrence(from);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string BuildFingerprint(GameTimeJobEntity row)
    {
        return $"{row.Name}|{row.Cron}|{row.IsOnce}|{row.MailId}";
    }

    private static DateTime DefaultClock()
    {
        return DateTime.Now;
    }

    /// <summary>
    /// 恢复默认时钟与类型解析器（单测隔离用）。
    /// </summary>
    internal static void ResetRuntimeHooksForTests()
    {
        BusinessJobTypeResolver = DefaultBusinessJobTypeResolver;
        Clock = DefaultClock;
    }

    /// <summary>
    /// 清空跟踪状态（单测隔离用）。
    /// </summary>
    internal static void ResetTrackingForTests()
    {
        TrackedTasks.Clear();
    }

    private static Type? DefaultBusinessJobTypeResolver(string name)
    {
        // 旧引擎用 Type.GetType("GameFrameX.Application.Platform.Job.{name}") 无程序集限定解析，
        // 实测解析不到（探针测试为证）——业务定时任务在合一前从未真正执行过。
        // 改为按项目类型目录解析：先精确匹配带命名空间前缀的 FullName，再按类名匹配本命名空间下的实现。
        var fullName = $"GameFrameX.Application.Platform.Job.{name}";
        return ProjectTypeCatalog.EffectiveTypes.FirstOrDefault(type =>
                         (type.FullName == fullName || (type.Name == name && type.Namespace == "GameFrameX.Application.Platform.Job"))
                         && typeof(IScheduleJob).IsAssignableFrom(type));
    }

    /// <summary>
    /// 跟踪中的业务定时任务（进程内状态；行删除后移除）。
    /// </summary>
    private sealed class TrackedBusinessTask
    {
        public long RowId { get; init; }

        public string Fingerprint { get; set; } = string.Empty;

        public DateTime? NextRun { get; set; }

        public GameTimeJobEntity? CurrentRow { get; set; }
    }
}
