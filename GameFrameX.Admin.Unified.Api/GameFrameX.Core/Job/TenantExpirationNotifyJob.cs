using GameFrameX.Core.Service.OnlineUser;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Job;

/// <summary>
/// 租户到期通知作业
/// 每天扫描即将到期的租户，提前 7/3/1 天发送邮件和站内信通知
/// </summary>
[JobDetail("job_tenant_expiration_notify", Description = "租户到期通知", GroupName = "default", Concurrent = false)]
[Daily(TriggerId = "trigger_tenant_expiration_notify", Description = "租户到期通知")]
public class TenantExpirationNotifyJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    // 通知提前天数
    private static readonly int[] NotifyDays = { 7, 3, 1, };

    public TenantExpirationNotifyJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var tenantRepository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemTenantEntity>>();
        var tenantDbConnectionProvider = scope.ServiceProvider.GetRequiredService<TenantDbConnectionProvider>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TenantExpirationNotifyJob>>();

        var tenants = await tenantRepository.AsQueryable()
                                     .Where(u => u.ExpireTime != null && u.IsEnabled == true)
                                     .ToListAsync(stoppingToken);

        foreach (var tenant in tenants)
        {
            var expireTime = TimerHelper.TimestampSecondsToDateTime(tenant.ExpireTime, true);
            var daysLeft = (int)((tenant.ExpireTime.GetValueOrDefault() - TimerHelper.UnixTimeSeconds()) / (24 * 60 * 60));

            if (!NotifyDays.Contains(daysLeft))
            {
                continue;
            }

            var tenantDb = tenantDbConnectionProvider.GetTenantDbConnectionScope(tenant.Id);
            if (tenantDb == null)
            {
                continue;
            }

            var adminUserId = await tenantDb.Queryable<TenantUserEntity>()
                                            .Where(u => u.TenantId == tenant.Id && u.AccountType == AccountTypeEnum.SystemAdmin && u.IsDeleted == false)
                                            .Select(u => u.Id)
                                            .FirstAsync(stoppingToken);

            if (adminUserId <= 0)
            {
                continue;
            }

            var message = $"您的服务将于 {daysLeft} 天后到期（{expireTime:yyyy-MM-dd}），请及时续费以免影响使用。";

            try
            {
                using (TenantOperationContext.UseTenant(tenant.Id))
                {
                    var onlineUserService = scope.ServiceProvider.GetRequiredService<TenantOnlineUserService>();
                    var notice = new TenantNoticeEntity
                    {
                        Title = "服务到期提醒",
                        Content = message,
                        Type = NoticeTypeEnum.NOTICE,
                    };
                    await onlineUserService.PublicNoticeAsync(notice, new List<long> { adminUserId, });
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "发送租户 {TenantId} 站内信通知失败", tenant.Id);
            }
        }
    }
}