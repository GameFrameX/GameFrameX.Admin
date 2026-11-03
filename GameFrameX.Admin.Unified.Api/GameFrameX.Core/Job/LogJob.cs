using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Log;

namespace GameFrameX.Core.Job;

/// <summary>
/// 按保留期清理主库中的日志。
/// </summary>
[JobDetail("job_log", Description = "清理操作日志", GroupName = "default", Concurrent = false)]
[Daily(TriggerId = "trigger_log", Description = "清理操作日志")]
public class LogJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public LogJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using (var serviceScope = _scopeFactory.CreateScope())
        {
            var daysAgo = 30; // 删除30天以前
            var cutoffTimestamp = TimerHelper.UnixTimeSeconds() - daysAgo * 24 * 60 * 60;

            var provider = serviceScope.ServiceProvider;
            var mainDb = provider.GetRequiredService<ISqlSugarClient>().AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);
            await mainDb.Deleteable<SystemLogVisitEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
            await mainDb.Deleteable<SystemLogOperationEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
            await mainDb.Deleteable<SystemLogDiffEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);

            await mainDb.Deleteable<TenantLogVisitEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
            await mainDb.Deleteable<TenantLogOperationEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
            await mainDb.Deleteable<TenantLogDiffEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
        }
    }
}
