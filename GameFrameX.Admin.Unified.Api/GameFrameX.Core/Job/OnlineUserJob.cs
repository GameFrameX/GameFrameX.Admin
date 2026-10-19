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
        using var serviceScope = _scopeFactory.CreateScope();

        var tenantRepository = serviceScope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemTenantEntity>>();
        var tenantDbConnectionProvider = serviceScope.ServiceProvider.GetRequiredService<TenantDbConnectionProvider>();
        var tenants = await tenantRepository.AsQueryable()
                                     .Filter(null, true)
                                     .Where(u => u.IsEnabled == true && u.TenantType == TenantTypeEnum.Db && !string.IsNullOrWhiteSpace(u.Connection))
                                     .ToListAsync(stoppingToken);

        foreach (var tenant in tenants)
        {
            var tenantDb = tenantDbConnectionProvider.GetTenantDbConnectionScope(tenant.Id);
            if (tenantDb == null)
            {
                continue;
            }

            await tenantDb.Deleteable<TenantOnlineUserEntity>()
                          .Where(u => u.TenantId == tenant.Id)
                          .ExecuteCommandAsync(stoppingToken);
        }

        var originColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("【" + DateTime.Now + "】服务重启清空在线用户");
        Console.ForegroundColor = originColor;

        // 缓存租户列表；这里只依赖主库 system_tenant，避免构造带租户仓储的租户管理服务。
        serviceScope.ServiceProvider.GetRequiredService<SystemCacheService>().Set(CacheConst.KeyTenant, tenants);
    }
}