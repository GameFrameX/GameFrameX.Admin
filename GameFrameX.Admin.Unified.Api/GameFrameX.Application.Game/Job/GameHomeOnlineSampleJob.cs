using GameFrameX.Application.Game.Home.GameHome;
using GameFrameX.Core.Job;
using Microsoft.Extensions.Logging;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Application.Game.Job;

/// <summary>
/// 首页在线人数时序采样作业（C321 / GFX-730）
/// <para>每 15 分钟按「启用区服 Control GetOnlinePlayer 求和」聚合全服在线数，
/// upsert 到主库采样表（15 分钟桶起点 + 当前值覆盖）；首页 onlineTrend 接口按整点桶回读。</para>
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
            var database = serviceScope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            var httpClientFactory = serviceScope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var bucketTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 900 * 900;
            var onlineCount = await GameHomeOnlineCounter.GetTotalOnlineAsync(database, httpClientFactory);
            await UpsertSampleAsync(database, bucketTime, onlineCount, stoppingToken);
        }
    }

    /// <summary>
    /// 采样落表（桶起点唯一）。
    /// 用普通 Insertable/Updateable 而非 DiffLog 变体（lesson：2026-07-13-sqlsugar-insertwithdifflog-tenant-prefix-loss）。
    /// </summary>
    private static async Task UpsertSampleAsync(ISqlSugarClient tenantDb, long bucketTime, int onlineCount, CancellationToken stoppingToken)
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
            SampleTime = bucketTime,
            OnlineCount = onlineCount,
        }).ExecuteCommandAsync(stoppingToken);
    }
}
