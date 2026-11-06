using GameFrameX.Core.Service.Business;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Log;

namespace GameFrameX.Core.Job;

/// <summary>
/// 按保留期清理主库差异日志与管理员历史日志（C404 裁定 §9.4）。
/// <para>访问日志与操作日志的留存已交由审计留存作业（job_auditRetention：搬归档表）负责，
/// 本作业不再删除 SystemLogVisit / SystemLogOperation，防止在归档前删掉热数据。</para>
/// <para>触发器保持无参 [Daily]：调度合一后该特性不带调度参数，作业保持休眠（§9.8）；
/// 是否启用差异日志清理登记为后续候选。</para>
/// </summary>
[JobDetail("job_log", Description = "清理差异日志与历史日志", GroupName = "default", Concurrent = false)]
[Daily(TriggerId = "trigger_log", Description = "清理差异日志与历史日志")]
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
            var mainDb = provider.GetRequiredService<ISqlSugarClient>().GetMainDatabaseScope();
            // §9.4：SystemLogVisit / SystemLogOperation 的删除已移交审计留存（job_auditRetention 归档）；
            // 此处仅保留差异日志与管理员历史日志（C400 后只读）的清理。
            await mainDb.Deleteable<SystemLogDiffEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);

            await mainDb.Deleteable<LogVisitEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
            await mainDb.Deleteable<LogOperationEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
            await mainDb.Deleteable<LogDiffEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
        }
    }
}
