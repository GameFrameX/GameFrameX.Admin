using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.System;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Job;

/// <summary>
/// 清理在线用户作业任务
/// </summary>
[JobDetail("job_onlineUser", Description = "清理在线用户", GroupName = "default", Concurrent = false)]
[PeriodSeconds(1, TriggerId = "trigger_onlineUser", Description = "清理在线用户", MaxNumberOfRuns = 1, RunOnStart = true)]
public class OnlineUserJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OnlineUserJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using (var serviceScope = _scopeFactory.CreateScope())
        {
            var database = serviceScope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            await database.Deleteable<TenantOnlineUserEntity>().ExecuteCommandAsync(stoppingToken);
        }

        Console.WriteLine("服务重启清空在线用户");
    }
}
