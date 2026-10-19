using System.Reflection;
using System.Text.RegularExpressions;
using GameFrameX.Application.System.Job.Dto;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.System.Job;

/// <summary>
/// 系统作业任务服务
/// </summary>
[ApiDescriptionSettings(Order = 320)]
public class SystemJobService : IAdminApiController
{
    private readonly SqlSugarRepository<SystemJobDetailEntity> _systemJobDetailRepository;
    private readonly SqlSugarRepository<SystemJobTriggerEntity> _systemJobTriggerRepository;
    private readonly SqlSugarRepository<SystemJobClusterEntity> _systemJobClusterRepository;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly DynamicJobCompiler _dynamicJobCompiler;

    public SystemJobService(SqlSugarRepository<SystemJobDetailEntity> systemJobDetailRepository,
        SqlSugarRepository<SystemJobTriggerEntity> systemJobTriggerRepository,
        SqlSugarRepository<SystemJobClusterEntity> systemJobClusterRepository,
        ISchedulerFactory schedulerFactory,
        DynamicJobCompiler dynamicJobCompiler)
    {
        _systemJobDetailRepository = systemJobDetailRepository;
        _systemJobTriggerRepository = systemJobTriggerRepository;
        _systemJobClusterRepository = systemJobClusterRepository;
        _schedulerFactory = schedulerFactory;
        _dynamicJobCompiler = dynamicJobCompiler;
    }

    /// <summary>
    /// 获取作业分页列表
    /// </summary>
    [DisplayName("获取作业分页列表")]
    [ApiDescriptionSettings(Name = "PageJobDetail")]
    public async Task<SqlSugarPagedList<JobResponse>> PageJobDetailAsync(PageJobRequest input)
    {
        var jobId = input.JobId?.Trim() ?? string.Empty;
        var description = input.Description?.Trim() ?? string.Empty;
        var jobDetails = await _systemJobDetailRepository.AsQueryable()
                                                  .WhereIF(!string.IsNullOrWhiteSpace(jobId), u => u.JobId.Contains(jobId))
                                                  .WhereIF(!string.IsNullOrWhiteSpace(description), u => (u.Description ?? string.Empty).Contains(description))
                                                  .Select(d => new JobResponse
                                                  {
                                                      JobDetail = d,
                                                  }).ToPagedListAsync(input.Page, input.PageSize);
        await _systemJobDetailRepository.AsSugarClient().ThenMapperAsync(jobDetails.Items, async u => { u.JobTriggers = await _systemJobTriggerRepository.GetListAsync(t => t.JobId == u.JobDetail.JobId); });

        // 提取中括号里面的参数值
        var rgx = new Regex(@"(?i)(?<=\[)(.*)(?=\])");
        foreach (var job in jobDetails.Items)
        {
            foreach (var jobTrigger in job.JobTriggers)
            {
                jobTrigger.Args = rgx.Match(jobTrigger.Args ?? "").Value;
            }
        }

        return jobDetails;
    }

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "AddJobDetail")]
    [HttpPost]
    [DisplayName("添加作业")]
    public async Task AddJobDetailAsync(AddJobDetailRequest input)
    {
        var isExist = await _systemJobDetailRepository.IsAnyAsync(u => u.JobId == input.JobId && u.Id != input.Id);
        if (isExist)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }

        // 动态创建作业
        Type jobType;
        switch (input.CreateType)
        {
            case JobCreateTypeEnum.Script when string.IsNullOrEmpty(input.ScriptCode):
                throw BusinessError.Oh(JobErrorCode.ScriptCodeEmpty);
            case JobCreateTypeEnum.Script:
            {
                jobType = _dynamicJobCompiler.BuildJob(input.ScriptCode);

                if (jobType.GetCustomAttributes(typeof(JobDetailAttribute)).FirstOrDefault() is not JobDetailAttribute jobDetailAttribute)
                {
                    throw BusinessError.Oh(JobErrorCode.JobDetailAttributeRequired);
                }

                if (jobDetailAttribute.JobId != input.JobId)
                {
                    throw BusinessError.Oh(JobErrorCode.JobIdMismatchWithScript);
                }

                break;
            }
            case JobCreateTypeEnum.Http:
                jobType = typeof(HttpJob);
                break;

            default:
                throw new NotSupportedException();
        }

        _schedulerFactory.AddJob(
            JobBuilder.Create(jobType)
                      .LoadFrom(input.Adapt<SystemJobDetailEntity>()).SetJobType(jobType));

        // 延迟一下等待持久化写入，再执行其他字段的更新
        await Task.Delay(500);
        await _systemJobDetailRepository.AsUpdateable()
                                 .SetColumns(u => new SystemJobDetailEntity { CreateType = input.CreateType, ScriptCode = input.ScriptCode, })
                                 .Where(u => u.JobId == input.JobId).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新作业
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "UpdateJobDetail")]
    [HttpPost]
    [DisplayName("更新作业")]
    public async Task UpdateJobDetailAsync(UpdateJobDetailRequest input)
    {
        var isExist = await _systemJobDetailRepository.IsAnyAsync(u => u.JobId == input.JobId && u.Id != input.Id);
        if (isExist)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }

        var systemJobDetail = await _systemJobDetailRepository.GetFirstAsync(u => u.Id == input.Id);
        if (systemJobDetail.JobId != input.JobId)
        {
            throw BusinessError.Oh(JobErrorCode.JobIdChangeForbidden);
        }

        var scheduler = _schedulerFactory.GetJob(systemJobDetail.JobId);
        var oldScriptCode = systemJobDetail.ScriptCode; //旧脚本代码
        input.Adapt(systemJobDetail);

        if (input.CreateType == JobCreateTypeEnum.Script)
        {
            if (string.IsNullOrEmpty(input.ScriptCode))
            {
                throw BusinessError.Oh(JobErrorCode.ScriptCodeEmpty);
            }

            if (input.ScriptCode != oldScriptCode)
            {
                // 动态创建作业
                var jobType = _dynamicJobCompiler.BuildJob(input.ScriptCode);

                if (jobType.GetCustomAttributes(typeof(JobDetailAttribute)).FirstOrDefault() is not JobDetailAttribute jobDetailAttribute)
                {
                    throw BusinessError.Oh(JobErrorCode.JobDetailAttributeRequired);
                }

                if (jobDetailAttribute.JobId != input.JobId)
                {
                    throw BusinessError.Oh(JobErrorCode.JobIdMismatchWithScript);
                }

                if (scheduler != null)
                {
                    await scheduler.UpdateDetailAsync(JobBuilder.Create(jobType).LoadFrom(systemJobDetail).SetJobType(jobType));
                }
            }
        }
        else
        {
            if (scheduler != null)
            {
                await scheduler.UpdateDetailAsync(scheduler.GetJobBuilder().LoadFrom(systemJobDetail));
            }
        }

        // Tip: 假如这次更新有变更了 JobId，变更 JobId 后触发的持久化更新执行，会由于找不到 JobId 而更新不到数据
        // 延迟一下等待持久化写入，再执行其他字段的更新
        await Task.Delay(500);
        await _systemJobDetailRepository.UpdateAsync(systemJobDetail);
    }

    /// <summary>
    /// 删除作业
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "DeleteJobDetail")]
    [HttpPost]
    [DisplayName("删除作业")]
    public async Task DeleteJobDetailAsync(DeleteJobDetailRequest input)
    {
        _schedulerFactory.RemoveJob(input.JobId);

        // 如果 _schedulerFactory 中不存在 JodId，则无法触发持久化，下面的代码确保作业和触发器能被删除
        await _systemJobDetailRepository.DeleteAsync(u => u.JobId == input.JobId);
        await _systemJobTriggerRepository.DeleteAsync(u => u.JobId == input.JobId);
    }

    /// <summary>
    /// 获取触发器列表
    /// </summary>
    [DisplayName("获取触发器列表")]
    [ApiDescriptionSettings(Name = "GetJobTriggerList")]
    public async Task<List<SystemJobTriggerResponse>> GetJobTriggerListAsync([FromQuery] JobDetailRequest input)
    {
        return await _systemJobTriggerRepository.AsQueryable()
                                         .WhereIF(!string.IsNullOrWhiteSpace(input.JobId), u => u.JobId.Contains(input.JobId))
                                         .Select<SystemJobTriggerResponse>()
                                         .ToListAsync();
    }

    /// <summary>
    /// 添加触发器
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "AddJobTrigger")]
    [HttpPost]
    [DisplayName("添加触发器")]
    public async Task AddJobTriggerAsync(AddJobTriggerRequest input)
    {
        var isExist = await _systemJobTriggerRepository.IsAnyAsync(u => u.TriggerId == input.TriggerId && u.Id != input.Id);
        if (isExist)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }

        var jobTrigger = input.Adapt<SystemJobTriggerEntity>();
        jobTrigger.Args = "[" + jobTrigger.Args + "]";

        var scheduler = _schedulerFactory.GetJob(input.JobId);
        if (scheduler != null)
        {
            await scheduler.AddTriggerAsync(Triggers.Create(input.AssemblyName, input.TriggerType).LoadFrom(jobTrigger));
        }
    }

    /// <summary>
    /// 更新触发器
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "UpdateJobTrigger")]
    [HttpPost]
    [DisplayName("更新触发器")]
    public async Task UpdateJobTriggerAsync(UpdateJobTriggerRequest input)
    {
        var isExist = await _systemJobTriggerRepository.IsAnyAsync(u => u.TriggerId == input.TriggerId && u.Id != input.Id);
        if (isExist)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }

        var jobTrigger = input.Adapt<SystemJobTriggerEntity>();
        jobTrigger.Args = "[" + jobTrigger.Args + "]";

        var scheduler = _schedulerFactory.GetJob(input.JobId);
        if (scheduler != null)
        {
            await scheduler.UpdateTriggerAsync(Triggers.Create(input.AssemblyName, input.TriggerType).LoadFrom(jobTrigger));
        }
    }

    /// <summary>
    /// 删除触发器
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "DeleteJobTrigger")]
    [HttpPost]
    [DisplayName("删除触发器")]
    public async Task DeleteJobTriggerAsync(DeleteJobTriggerRequest input)
    {
        var scheduler = _schedulerFactory.GetJob(input.JobId);
        if (scheduler != null)
        {
            await scheduler.RemoveTriggerAsync(input.TriggerId);
        }

        // 如果 _schedulerFactory 中不存在 JodId，则无法触发持久化，下行代码确保触发器能被删除
        await _systemJobTriggerRepository.DeleteAsync(u => u.JobId == input.JobId && u.TriggerId == input.TriggerId);
    }

    /// <summary>
    /// 暂停所有作业
    /// </summary>
    /// <returns></returns>
    [DisplayName("暂停所有作业")]
    public void PauseAllJob()
    {
        _schedulerFactory.PauseAll();
    }

    /// <summary>
    /// 启动所有作业
    /// </summary>
    /// <returns></returns>
    [DisplayName("启动所有作业")]
    public void StartAllJob()
    {
        _schedulerFactory.StartAll();
    }

    /// <summary>
    /// 暂停作业
    /// </summary>
    [DisplayName("暂停作业")]
    public void PauseJob(JobDetailRequest input)
    {
        var scheduler = _schedulerFactory.GetJob(input.JobId);
        scheduler?.Pause();
    }

    /// <summary>
    /// 启动作业
    /// </summary>
    [DisplayName("启动作业")]
    public void StartJob(JobDetailRequest input)
    {
        var scheduler = _schedulerFactory.GetJob(input.JobId);
        scheduler?.Start();
    }

    /// <summary>
    /// 执行作业
    /// </summary>
    /// <param name="input"></param>
    [DisplayName("执行作业")]
    [ApiDescriptionSettings(Name = "RunJob")]
    public async Task RunJobAsync(JobDetailRequest input)
    {
        if (await _schedulerFactory.TryRunJobAsync(input.JobId) != ScheduleResult.Succeed)
        {
            throw BusinessError.Oh(JobErrorCode.JobExecutionFailed);
        }
    }

    /// <summary>
    /// 暂停触发器
    /// </summary>
    [DisplayName("暂停触发器")]
    public void PauseTrigger(JobTriggerRequest input)
    {
        var scheduler = _schedulerFactory.GetJob(input.JobId);
        scheduler?.PauseTrigger(input.TriggerId);
    }

    /// <summary>
    /// 启动触发器
    /// </summary>
    [DisplayName("启动触发器")]
    public void StartTrigger(JobTriggerRequest input)
    {
        var scheduler = _schedulerFactory.GetJob(input.JobId);
        scheduler?.StartTrigger(input.TriggerId);
    }

    /// <summary>
    /// 强制唤醒作业调度器
    /// </summary>
    [DisplayName("强制唤醒作业调度器")]
    public void CancelSleep()
    {
        _schedulerFactory.CancelSleep();
    }

    /// <summary>
    /// 强制触发所有作业持久化
    /// </summary>
    [DisplayName("强制触发所有作业持久化")]
    public void PersistAll()
    {
        _schedulerFactory.PersistAll();
    }

    /// <summary>
    /// 获取集群列表
    /// </summary>
    [DisplayName("获取集群列表")]
    [ApiDescriptionSettings(Name = "GetJobClusterList")]
    public async Task<List<SystemJobClusterResponse>> GetJobClusterListAsync()
    {
        return await _systemJobClusterRepository.AsQueryable()
                                         .Select<SystemJobClusterResponse>()
                                         .ToListAsync();
    }
}