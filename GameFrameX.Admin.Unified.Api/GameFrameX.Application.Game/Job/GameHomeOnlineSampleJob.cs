using GameFrameX.Application.Game.Home.GameHome;
using GameFrameX.Core.Job;
using Microsoft.Extensions.Logging;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Application.Game.Job;

/// <summary>
/// 首页在线人数时序采样作业（C321 / GFX-730）
/// <para>每 15 分钟遍历启用租户，按「启用区服 Control GetOnlinePlayer 求和」聚合全服在线数，
/// upsert 到租户库采样表（15 分钟桶起点 + 当前值覆盖）；首页 onlineTrend 接口按整点桶回读。</para>
/// </summary>
[JobDetail("job_homeOnlineSample", Description = "首页在线人数时序采样", GroupName = "default", Concurrent = false)]
[PeriodSeconds(900, TriggerId = "trigger_homeOnlineSample", Description = "首页在线人数时序采样", RunOnStart = true)]
public class GameHomeOnlineSampleJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GameHomeOnlineSampleJob> _logger;

    public GameHomeOnlineSampleJob(IServiceScopeFactory scopeFactory, ILogger<GameHomeOnlineSampleJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using (var serviceScope = _scopeFactory.CreateScope())
        {
            var tenantRepository = serviceScope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemTenantEntity>>();
            var tenantDbConnectionProvider = serviceScope.ServiceProvider.GetRequiredService<TenantDbConnectionProvider>();
            var httpClientFactory = serviceScope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var tenants = await tenantRepository.AsQueryable()
                                     .Filter(null, true)
                                     .Where(u => u.IsEnabled == true && u.TenantType == TenantTypeEnum.Db && !string.IsNullOrWhiteSpace(u.Connection))
                                     .ToListAsync(stoppingToken);

            // 15 分钟桶起点（Unix 秒，与时区无关）；同一桶内重复触发时覆盖为最新值
            var bucketTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 900 * 900;

            foreach (var tenant in tenants)
            {
                // 单租户失败（如远程存量租户库尚未建采样表，CodeFirst 直迁同 C314 判例）只记日志，不阻断其它租户采样
                try
                {
                    var tenantDb = tenantDbConnectionProvider.GetTenantDbConnectionScope(tenant.Id);
                    if (tenantDb == null)
                    {
                        continue;
                    }

                    using (TenantOperationContext.UseTenant(tenant.Id))
                    {
                        var onlineCount = await GameHomeOnlineCounter.GetTotalOnlineAsync(tenantDb, httpClientFactory);
                        await UpsertSampleAsync(tenantDb, tenant.Id, bucketTime, onlineCount, stoppingToken);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "租户 {TenantId} 在线人数采样失败，已跳过（不影响其它租户）。", tenant.Id);
                }
            }
        }
    }

    /// <summary>
    /// 采样落表（桶起点 + 租户唯一）。作业无登录用户，审计 AOP 的 TenantId 分支不生效，须显式赋值。
    /// 用普通 Insertable/Updateable 而非 DiffLog 变体（lesson：2026-07-13-sqlsugar-insertwithdifflog-tenant-prefix-loss）。
    /// </summary>
    private static async Task UpsertSampleAsync(ISqlSugarClient tenantDb, long tenantId, long bucketTime, int onlineCount, CancellationToken stoppingToken)
    {
        var existing = await tenantDb.Queryable<Entity.Tenant.Game.TenantGameHomeOnlineSampleEntity>()
                                     .Where(u => u.SampleTime == bucketTime)
                                     .FirstAsync(stoppingToken);
        if (existing != null)
        {
            existing.OnlineCount = onlineCount;
            await tenantDb.Updateable(existing).ExecuteCommandAsync(stoppingToken);
            return;
        }

        await tenantDb.Insertable(new Entity.Tenant.Game.TenantGameHomeOnlineSampleEntity
        {
            TenantId = tenantId,
            SampleTime = bucketTime,
            OnlineCount = onlineCount,
        }).ExecuteCommandAsync(stoppingToken);
    }
}
