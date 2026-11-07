using GameFrameX.Core.Job;
using Microsoft.Extensions.Logging;

namespace GameFrameX.Application.Audit.Retention;

/// <summary>
/// 审计日志归档内置作业（触发器按裁定 改用 Cron）。
/// 默认每天 03:30 由调度引擎触发；开关与天数见 <see cref="AuditRetentionOptions"/>。
/// </summary>
[JobDetail("job_auditRetention", Description = "审计日志归档", GroupName = "default", Concurrent = false)]
[Cron("0 30 3 * * *", TriggerId = "trigger_auditRetention", Description = "审计日志归档（每天 03:30）")]
public class AuditRetentionJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="scopeFactory">服务作用域工厂</param>
    public AuditRetentionJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// 执行审计日志归档。
    /// </summary>
    /// <param name="context">作业执行上下文</param>
    /// <param name="stoppingToken">取消令牌</param>
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using (var serviceScope = _scopeFactory.CreateScope())
        {
            var retentionService = serviceScope.ServiceProvider.GetRequiredService<AuditRetentionService>();
            var result = await retentionService.ExecuteAsync(stoppingToken);
            context.Result = result;
            serviceScope.ServiceProvider.GetService<ILogger<AuditRetentionJob>>()?.LogInformation(
                "审计日志归档作业完成：启用={Enabled}，耗时={ElapsedMilliseconds}ms，明细={Tables}",
                result.Enabled, result.ElapsedMilliseconds,
                string.Join("；", result.Tables.Select(table => $"{table.LogKind} 归档 {table.ArchivedCount} 行 / 清理 {table.PurgedCount} 行")));
        }
    }
}
