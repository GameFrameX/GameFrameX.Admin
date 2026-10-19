using GameFrameX.Core.Cache;
using GameFrameX.Core.Extension.Cache;
using GameFrameX.Entity.Enum;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameFrameX.Application.System.Job;

/// <summary>
/// 作业集群控制
/// </summary>
public class JobClusterServer : IJobClusterServer
{
    private const int HeartbeatDelayMilliseconds = 3000;
    private const int MaxRetryDelayMilliseconds = 30000;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobClusterServer> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ICache _cache;

    public JobClusterServer(IServiceProvider serviceProvider,
        ILogger<JobClusterServer> logger,
        IHostApplicationLifetime applicationLifetime,
        ICache cache)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _cache = cache;
    }

    /// <summary>
    /// 当前作业调度器启动通知
    /// </summary>
    /// <param name="context">作业集群服务上下文</param>
    public void Start(JobClusterContext context)
    {
        QueueClusterStatusUpdate(context.ClusterId, ClusterStatus.Waiting, true, nameof(Start));
    }

    /// <summary>
    /// 等待被唤醒
    /// </summary>
    /// <param name="context">作业集群服务上下文</param>
    /// <returns><see cref="Task"/></returns>
    public async Task WaitingForAsync(JobClusterContext context)
    {
        var clusterId = context.ClusterId;
        var stoppingToken = _applicationLifetime.ApplicationStopping;
        var failureCount = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            // 控制集群心跳频率（放在头部为了防止 IsAnyAsync continue 没sleep占用大量IO和CPU）
            await Task.Delay(HeartbeatDelayMilliseconds + Random.Shared.Next(500, 1000), stoppingToken); // 错开集群同时启动

            try
            {
                //使用分布式锁
                using (_cache.AcquireLock("lock:JobClusterServer:WaitingForAsync", 1000))
                {
                    using var scope = _serviceProvider.CreateScope();
                    var _systemJobClusterRepository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemJobClusterEntity>>();
                    // 在这里查询数据库，根据以下两种情况处理
                    // 1) 如果作业集群表已有 status 为 ClusterStatus.Working 则继续循环
                    // 2) 如果作业集群表中还没有其他服务或只有自己，则插入一条集群服务或调用 await WorkNowAsync(clusterId); 之后 return;
                    // 3) 如果作业集群表中没有 status 为 ClusterStatus.Working 的，调用 await WorkNowAsync(clusterId); 之后 return;
                    if (await _systemJobClusterRepository.IsAnyAsync(u => u.Status == ClusterStatus.Working))
                    {
                        failureCount = 0;
                        continue;
                    }

                    await WorkNowAsync(clusterId);
                    return;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                failureCount++;
                var retryDelay = GetRetryDelayMilliseconds(failureCount);
                _logger.LogWarning(ex, "作业集群 {ClusterId} 等待选主失败，{RetryDelay}ms 后重试", clusterId, retryDelay);

                await Task.Delay(retryDelay, stoppingToken);
            }
        }
    }

    private static int GetRetryDelayMilliseconds(int failureCount)
    {
        var cappedFailureCount = Math.Min(failureCount, 5);
        var delay = HeartbeatDelayMilliseconds * (1 << (cappedFailureCount - 1));
        return Math.Min(delay, MaxRetryDelayMilliseconds);
    }

    private void QueueClusterStatusUpdate(string clusterId, ClusterStatus status, bool upsert, string operation)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var _systemJobClusterRepository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemJobClusterEntity>>();
                if (upsert && await _systemJobClusterRepository.IsAnyAsync(u => u.ClusterId == clusterId))
                {
                    await _systemJobClusterRepository.AsUpdateable()
                                              .SetColumns(u => u.Status == status)
                                              .Where(u => u.ClusterId == clusterId)
                                              .ExecuteCommandAsync();
                    return;
                }

                if (upsert)
                {
                    await _systemJobClusterRepository.AsInsertable(new SystemJobClusterEntity { ClusterId = clusterId, Status = status, }).ExecuteCommandAsync();
                    return;
                }

                await _systemJobClusterRepository.UpdateAsync(u => new SystemJobClusterEntity { Status = status, }, u => u.ClusterId == clusterId);
            }
            catch (ObjectDisposedException) when (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "作业集群 {ClusterId} 状态更新失败，操作：{Operation}，目标状态：{Status}", clusterId, operation, status);
            }
        });
    }

    /// <summary>
    /// 当前作业调度器停止通知
    /// </summary>
    /// <param name="context">作业集群服务上下文</param>
    public void Stop(JobClusterContext context)
    {
        QueueClusterStatusUpdate(context.ClusterId, ClusterStatus.Crashed, false, nameof(Stop));
    }

    /// <summary>
    /// 当前作业调度器宕机
    /// </summary>
    /// <param name="context">作业集群服务上下文</param>
    public void Crash(JobClusterContext context)
    {
        QueueClusterStatusUpdate(context.ClusterId, ClusterStatus.Crashed, false, nameof(Crash));
    }

    /// <summary>
    /// 指示集群可以工作
    /// </summary>
    /// <param name="clusterId">集群 Id</param>
    /// <returns></returns>
    private async Task WorkNowAsync(string clusterId)
    {
        using var scope = _serviceProvider.CreateScope();
        var _systemJobClusterRepository = scope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemJobClusterEntity>>();
        // 在作业集群表中，更新 clusterId 的 status 为 ClusterStatus.Working
        await _systemJobClusterRepository.UpdateAsync(u => new SystemJobClusterEntity { Status = ClusterStatus.Working, }, u => u.ClusterId == clusterId);
    }
}