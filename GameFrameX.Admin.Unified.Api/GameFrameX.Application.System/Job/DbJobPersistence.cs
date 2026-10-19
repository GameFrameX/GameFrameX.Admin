using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.System.Job;

/// <summary>
/// 作业持久化（数据库）
/// </summary>
public class DbJobPersistence : IJobPersistence
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DbJobPersistence(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// 作业调度服务启动时
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<SchedulerBuilder>> PreloadAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var jobDetailRepository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemJobDetailEntity>>();
        var jobTriggerRepository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemJobTriggerEntity>>();
        var dynamicJobCompiler = scope.ServiceProvider.GetRequiredService<DynamicJobCompiler>();

        // 获取所有定义的作业
        var allJobs = ProjectTypeCatalog.EffectiveTypes.ScanToBuilders().ToList();
        // 若数据库不存在任何作业，则直接返回
        if (!await jobDetailRepository.IsAnyAsync(u => true, stoppingToken))
        {
            return allJobs;
        }

        // 遍历所有定义的作业
        foreach (var schedulerBuilder in allJobs)
        {
            // 获取作业信息构建器
            var jobBuilder = schedulerBuilder.GetJobBuilder();

            // 加载数据库数据
            var dbDetail = jobDetailRepository.GetFirst(u => u.JobId == jobBuilder.JobId);
            if (dbDetail == null)
            {
                continue;
            }

            // 同步数据库数据
            // 显式按字段覆盖，避免 历史框架 内部 LoadFrom(Mapster) 在 long? → DateTime? 上抛 InvalidCast；
            // UpdatedTime 跳过——历史框架 调度运行期会自行维护该字段。
            jobBuilder
                .SetGroupName(dbDetail.GroupName)
                .SetJobType(dbDetail.AssemblyName, dbDetail.JobType)
                .SetDescription(dbDetail.Description)
                .SetConcurrent(dbDetail.Concurrent)
                .SetIncludeAnnotations(dbDetail.IncludeAnnotations)
                .SetProperties(dbDetail.Properties ?? "{}");
            DisableAnnotationTriggerRescan(jobBuilder);

            // 获取作业的所有数据库的触发器
            var dbTriggers = jobTriggerRepository.GetList(u => u.JobId == jobBuilder.JobId).ToArray();
            // 遍历所有作业触发器
            foreach (var triggerBuilder in schedulerBuilder.GetEnumerable())
            {
                // 加载数据库数据
                var dbTrigger = dbTriggers.FirstOrDefault(u => u.JobId == jobBuilder.JobId && u.TriggerId == triggerBuilder.TriggerId);
                if (dbTrigger == null)
                {
                    continue;
                }

                LoadFromDb(dbTrigger, triggerBuilder).Updated(); // 标记更新
            }

            // 遍历所有非编译时定义的触发器加入到作业中
            foreach (var dbTrigger in dbTriggers)
            {
                if (schedulerBuilder.GetTriggerBuilder(dbTrigger.TriggerId)?.JobId == jobBuilder.JobId)
                {
                    continue;
                }

                var triggerBuilder = LoadFromDb(dbTrigger, TriggerBuilder.Create(dbTrigger.TriggerId));
                schedulerBuilder.AddTriggerBuilder(triggerBuilder); // 先添加
                triggerBuilder.Updated(); // 再标记更新
            }

            // 标记更新
            schedulerBuilder.Updated();
        }

        // 获取数据库所有通过脚本创建的作业
        var allDbScriptJobs = jobDetailRepository.GetList(u => u.CreateType != JobCreateTypeEnum.BuiltIn);
        foreach (var dbDetail in allDbScriptJobs)
        {
            // 动态创建作业
            Type jobType;
            switch (dbDetail.CreateType)
            {
                case JobCreateTypeEnum.Script:
                    jobType = dynamicJobCompiler.BuildJob(dbDetail.ScriptCode ?? throw new InvalidOperationException("脚本类型作业缺少 ScriptCode"));
                    break;

                case JobCreateTypeEnum.Http:
                    jobType = typeof(HttpJob);
                    break;

                default:
                    throw new NotSupportedException();
            }

            // 动态构建的 jobType 的程序集名称为随机名称，需重新设置
            dbDetail.AssemblyName = jobType.Assembly.FullName!.Split(',')[0];
            var jobBuilder = LoadFromDb(
                dbDetail,
                JobBuilder.Create(jobType)
                          .SetGroupName(dbDetail.GroupName ?? "default")
                          .SetJobType(dbDetail.AssemblyName, jobType.FullName));

            DisableAnnotationTriggerRescan(jobBuilder);

            // 获取作业的所有数据库的触发器加入到作业中
            var dbTriggers = jobTriggerRepository.GetList(u => u.JobId == jobBuilder.JobId).ToArray();
            var triggerBuilders = dbTriggers.Select(u => LoadFromDb(u, TriggerBuilder.Create(u.TriggerId)).Updated());
            var schedulerBuilder = SchedulerBuilder.Create(jobBuilder, triggerBuilders.ToArray());

            // 标记更新
            schedulerBuilder.Updated();

            allJobs.Add(schedulerBuilder);
        }

        return allJobs;
    }

    internal static JobBuilder DisableAnnotationTriggerRescan(JobBuilder jobBuilder)
    {
        // 已扫描或已显式加载触发器的 builder 不能再让 历史框架 Build 扫描 IJob 的 [Trigger] 特性，否则同名触发器会重复追加。
        return jobBuilder.SetIncludeAnnotations(false);
    }

    /// <summary>
    /// 作业计划初始化通知
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public Task<SchedulerBuilder> OnLoadingAsync(SchedulerBuilder builder, CancellationToken stoppingToken)
    {
        return Task.FromResult(builder);
    }

    /// <summary>
    /// 作业计划Scheduler的JobDetail变化时
    /// </summary>
    /// <param name="context"></param>
    public Task OnChangedAsync(PersistenceContext context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var jobDetailRepository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemJobDetailEntity>>();

        // 项目强制规则（C144）：*Time 字段一律 UTC 秒级 long?；
        // 与 历史框架.Schedule.JobDetail 边界处只允许通过 ScheduleTimeBridge 显式编码。
        var jobDetail = new SystemJobDetailEntity
        {
            JobId = context.JobDetail.JobId ?? string.Empty,
            GroupName = context.JobDetail.GroupName,
            JobType = context.JobDetail.JobType,
            AssemblyName = context.JobDetail.AssemblyName,
            Description = context.JobDetail.Description,
            Concurrent = context.JobDetail.Concurrent,
            IncludeAnnotations = context.JobDetail.IncludeAnnotations,
            Properties = context.JobDetail.Properties ?? "{}",
            UpdatedTime = ScheduleTimeBridge.ToUnixSeconds(context.JobDetail.UpdatedTime),
        };
        switch (context.Behavior)
        {
            case PersistenceBehavior.Appended:
                jobDetailRepository.AsInsertable(jobDetail).ExecuteCommand();
                break;

            case PersistenceBehavior.Updated:
                jobDetailRepository.AsUpdateable(jobDetail).WhereColumns(u => new { u.JobId, }).IgnoreColumns(u => new { u.Id, u.CreateType, u.ScriptCode, }).ExecuteCommand();
                break;

            case PersistenceBehavior.Removed:
                jobDetailRepository.AsDeleteable().Where(u => u.JobId == jobDetail.JobId).ExecuteCommand();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 作业计划Scheduler的触发器Trigger变化时
    /// </summary>
    /// <param name="context"></param>
    public Task OnTriggerChangedAsync(PersistenceTriggerContext context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var jobTriggerRepository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemJobTriggerEntity>>();

        // 项目强制规则（C144）：*Time 字段一律 UTC 秒级 long?；
        // 与 历史框架.Schedule.Trigger 边界处只允许通过 ScheduleTimeBridge 显式编码。
        var jobTrigger = new SystemJobTriggerEntity
        {
            TriggerId = context.Trigger.TriggerId ?? string.Empty,
            JobId = context.Trigger.JobId ?? string.Empty,
            TriggerType = context.Trigger.TriggerType,
            AssemblyName = context.Trigger.AssemblyName,
            Args = context.Trigger.Args,
            Description = context.Trigger.Description,
            Status = context.Trigger.Status,
            StartTime = ScheduleTimeBridge.ToUnixSeconds(context.Trigger.StartTime),
            EndTime = ScheduleTimeBridge.ToUnixSeconds(context.Trigger.EndTime),
            LastRunTime = ScheduleTimeBridge.ToUnixSeconds(context.Trigger.LastRunTime),
            NextRunTime = ScheduleTimeBridge.ToUnixSeconds(context.Trigger.NextRunTime),
            NumberOfRuns = context.Trigger.NumberOfRuns,
            MaxNumberOfRuns = context.Trigger.MaxNumberOfRuns,
            NumberOfErrors = context.Trigger.NumberOfErrors,
            MaxNumberOfErrors = context.Trigger.MaxNumberOfErrors,
            NumRetries = context.Trigger.NumRetries,
            RetryTimeout = context.Trigger.RetryTimeout,
            StartNow = context.Trigger.StartNow,
            RunOnStart = context.Trigger.RunOnStart,
            ResetOnlyOnce = context.Trigger.ResetOnlyOnce,
            UpdatedTime = ScheduleTimeBridge.ToUnixSeconds(context.Trigger.UpdatedTime),
        };
        switch (context.Behavior)
        {
            case PersistenceBehavior.Appended:
                jobTriggerRepository.AsInsertable(jobTrigger).ExecuteCommand();
                break;

            case PersistenceBehavior.Updated:
                jobTriggerRepository.AsUpdateable(jobTrigger).WhereColumns(u => new { u.TriggerId, u.JobId, }).IgnoreColumns(u => new { u.Id, }).ExecuteCommand();
                break;

            case PersistenceBehavior.Removed:
                jobTriggerRepository.AsDeleteable().Where(u => u.TriggerId == jobTrigger.TriggerId && u.JobId == jobTrigger.JobId).ExecuteCommand();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return Task.CompletedTask;
    }


    public Task OnExecutionRecordAsync(PersistenceExecutionRecordContext context)
    {
        return Task.CompletedTask;
    }

    // 显式把本地实体的 long? *Time 字段桥接回 历史框架.Schedule 的 DateTime?。
    // UpdatedTime 在 历史框架 中只有 internal set，且属于运行时由调度器自身维护的状态，
    // 故此处不覆盖；调度器在下次持久化时会按需刷新。
    private static TriggerBuilder LoadFromDb(SystemJobTriggerEntity db, TriggerBuilder builder)
    {
        builder
            .SetTriggerType(db.AssemblyName ?? "GameFrameX.Core", db.TriggerType ?? string.Empty)
            .SetArgs(db.Args)
            .SetDescription(db.Description)
            .SetStatus(db.Status)
            .SetStartTime(ScheduleTimeBridge.FromUnixSeconds(db.StartTime))
            .SetEndTime(ScheduleTimeBridge.FromUnixSeconds(db.EndTime))
            .SetLastRunTime(ScheduleTimeBridge.FromUnixSeconds(db.LastRunTime))
            .SetNextRunTime(ScheduleTimeBridge.FromUnixSeconds(db.NextRunTime))
            .SetNumberOfRuns(db.NumberOfRuns)
            .SetMaxNumberOfRuns(db.MaxNumberOfRuns)
            .SetNumberOfErrors(db.NumberOfErrors)
            .SetMaxNumberOfErrors(db.MaxNumberOfErrors)
            .SetNumRetries(db.NumRetries)
            .SetRetryTimeout(db.RetryTimeout)
            .SetStartNow(db.StartNow)
            .SetRunOnStart(db.RunOnStart)
            .SetResetOnlyOnce(db.ResetOnlyOnce);
        return builder;
    }

    private static JobBuilder LoadFromDb(SystemJobDetailEntity db, JobBuilder builder)
    {
        builder
            .SetGroupName(db.GroupName)
            .SetJobType(db.AssemblyName, db.JobType)
            .SetDescription(db.Description)
            .SetConcurrent(db.Concurrent)
            .SetIncludeAnnotations(db.IncludeAnnotations)
            .SetProperties(db.Properties ?? "{}");
        return builder;
    }
}