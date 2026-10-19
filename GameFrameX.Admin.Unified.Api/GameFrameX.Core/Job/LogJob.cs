using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Log;

namespace GameFrameX.Core.Job;

/// <summary>
/// 清理日志作业任务。C156 起：主库清理 SystemLog* 平台日志，每个库隔离租户的租户库清理 TenantLog* 租户日志。
/// 不再依赖独立日志库；日志随其归属库一起按保留期清理。
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
            var tenantProvider = provider.GetRequiredService<TenantDbConnectionProvider>();
            var tenant = provider.GetRequiredService<ISqlSugarClient>().AsTenant();

            // 1) 清理主库平台日志（system_log_*）
            var mainDb = tenant.GetConnectionScope(SqlSugarConst.MainConfigId);
            await mainDb.Deleteable<SystemLogVisitEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
            await mainDb.Deleteable<SystemLogOperationEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
            await mainDb.Deleteable<SystemLogDiffEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);

            // 2) 遍历每个库隔离租户，清理其租户库中的租户日志（tenant_log_*）
            var tenants = mainDb.Queryable<SystemTenantEntity>()
                                .Filter(null, true)
                                .Where(u => u.TenantType == TenantTypeEnum.Db)
                                .ToList();
            foreach (var tenantEntity in tenants)
            {
                stoppingToken.ThrowIfCancellationRequested();
                var tenantDb = tenantProvider.GetTenantDbConnectionScope(tenantEntity.Id);
                if (tenantDb == null)
                {
                    continue;
                }

                await tenantDb.Deleteable<TenantLogVisitEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
                await tenantDb.Deleteable<TenantLogOperationEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
                await tenantDb.Deleteable<TenantLogDiffEntity>().Where(u => u.CreatedTime < cutoffTimestamp).ExecuteCommandAsync(stoppingToken);
            }
        }
    }
}