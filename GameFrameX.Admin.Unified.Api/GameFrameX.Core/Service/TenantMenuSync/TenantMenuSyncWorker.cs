using System.Collections.Concurrent;
using System.Threading.Channels;
using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Foundation.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace GameFrameX.Core.Service.TenantMenuSync;

/// <summary>
/// 租户菜单同步后台工作器（C353 T6）。职责：启动目录比对扫描 + 周期兜底扫描（触发③④）入队，
/// 以及 N 路消费者出队执行 Applier。触发①（套餐保存）与触发⑤（SuperAdmin 手动端点）由各自调用方直接入队。
/// <para>并发：WorkerConcurrency（默认 4）路消费者；失败快速出队 + 延迟重入队（<see cref="TenantMenuSyncQueue.ScheduleDelayed"/>），
/// 绝不在并发槽内 sleep；连续失败退避 = ScanIntervalSeconds × min(连续失败次数, 3)，成功即清零（进程内存态，重启重置，可接受）。</para>
/// <para>同步范围（与 <see cref="SystemTenantService.GetTenantDbListAsync"/> 同口径，消费时按租户 Id 单点复核）：
/// TenantType=Db ∧ IsEnabled ∧ ConfigId != main。C354 T6 起默认租户（独立 ConfigId）纳入；
/// Enterprise 套餐菜单启动幂等补全先于本 Worker 首扫，默认租户收敛结果 = C189 全量、无振荡。</para>
/// <para>启动扫描等待菜单目录就绪（MenuSeedCatalog 未加载时 AllDefinitions 抛 NotLoaded，
/// 空目录绝不参与 diff —— 防未加载全量误删）。</para>
/// </summary>
public sealed class TenantMenuSyncWorker : BackgroundService
{
    private const string TriggerStartup = "startup";
    private const string TriggerScan = "scan";
    private const string TriggerRetry = "retry-backoff";

    private const int CatalogReadyPollDelaySeconds = 2;
    private const int CatalogReadyMaxWaitSeconds = 300;

    private readonly TenantMenuSyncQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<TenantMenuSyncOptions> _options;
    private readonly ILogger<TenantMenuSyncWorker> _logger;
    private readonly ConcurrentDictionary<long, int> _consecutiveFailures = new();

    public TenantMenuSyncWorker(
        TenantMenuSyncQueue queue,
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<TenantMenuSyncOptions> options,
        ILogger<TenantMenuSyncWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // GFX-872 / C380：SqlSugarScope（5.1.4.216）按 AsyncLocal 槽位为每个异步流解析 SqlSugarClient；
        // 宿主启动链上存在非异步写入点（DI 构造等无状态机屏障的路径）时，槽位会带着共享 client 沿启动链
        // ExecutionContext 向下继承——本 Worker 的扫描 + N 路消费者若直接继承，将共享同一 client 与同一
        // NpgsqlConnection，并发命令叠连接触发 NpgsqlOperationInProgressException（docker 栈 2026-09-27 16:47
        // 实测崩溃形态；同 client 跨流竞态另有 17:04:07 MappingColumns "Collection was modified" 佐证）。
        // 修复不变量：扫描与消费者一律在 ExecutionContext.SuppressFlow() 下 fork——子流从默认上下文起步、
        // 槽位为空，首次访问各自创建独立 client，与宿主启动链携带的任何共享槽位彻底隔离（顺序无关）。
        var loops = new List<Task>();
        using (ExecutionContext.SuppressFlow())
        {
            loops.Add(Task.Run(() => ScanLoopAsync(stoppingToken)));
            for (var i = 0; i < Math.Max(1, _options.CurrentValue.WorkerConcurrency); i++)
            {
                loops.Add(Task.Run(() => ConsumeLoopAsync(stoppingToken)));
            }
        }

        await Task.WhenAll(loops.ToArray());
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _queue.Complete();
        await base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// 扫描循环：启动等目录就绪 → 首扫（触发③）→ 周期兜底扫描（触发④，间隔运行时读取支持热更）。
    /// </summary>
    private async Task ScanLoopAsync(CancellationToken stoppingToken)
    {
        try
        {
            await WaitUntilCatalogReadyAsync(stoppingToken);
            await ScanOnceAsync(TriggerStartup, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // 下限 60s 与 TenantMenuSyncOptions 注释同口径：0/负值会让 PeriodicTimer 抛异常致扫描循环退出
                var intervalSeconds = Math.Max(60, _options.CurrentValue.ScanIntervalSeconds);
                using (var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds)))
                {
                    if (!await timer.WaitForNextTickAsync(stoppingToken))
                    {
                        break;
                    }
                }

                await ScanOnceAsync(TriggerScan, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // 正常停机
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "租户菜单同步扫描循环异常退出。");
        }
    }

    private async Task WaitUntilCatalogReadyAsync(CancellationToken stoppingToken)
    {
        var waited = 0;
        while (waited < CatalogReadyMaxWaitSeconds)
        {
            try
            {
                _ = MenuSeedCatalog.CatalogVersion;
                return;
            }
            catch (InvalidOperationException)
            {
                // ApiEndpointCatalogSync 尚未完成 LoadFromDb，等待重试
            }

            await Task.Delay(TimeSpan.FromSeconds(CatalogReadyPollDelaySeconds), stoppingToken);
            waited += CatalogReadyPollDelaySeconds;
        }

        _logger.LogWarning("等待菜单目录就绪超时（{Seconds}s），放弃本次启动扫描，等待下个周期。", CatalogReadyMaxWaitSeconds);
    }

    /// <summary>
    /// 一次扫描：枚举同步范围内租户 + 读状态行 → 纯函数 Plan → 逐项入队。
    /// </summary>
    private async Task ScanOnceAsync(string trigger, CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var tenantService = scope.ServiceProvider.GetRequiredService<SystemTenantService>();
                var mainDb = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>()
                                  .AsTenant()
                                  .GetConnectionScope(SqlSugarConst.MainConfigId);

                var tenants = await tenantService.GetTenantDbListAsync();
                var stateEntities = await mainDb.Queryable<SystemTenantMenuSyncStateEntity>()
                                                .Filter(null, true)
                                                .ToListAsync(stoppingToken);
                var stateRows = stateEntities
                    .Select(u => new TenantMenuSyncScanPlanner.StateRow(u.Id, u.LastCatalogVersion, u.Status, u.NextRetryTime))
                    .ToList();

                var plan = TenantMenuSyncScanPlanner.Plan(
                    tenants.Select(u => u.Id).ToArray(),
                    stateRows,
                    MenuSeedCatalog.CatalogVersion,
                    TimerHelper.UnixTimeSeconds());

                foreach (var item in plan)
                {
                    _queue.TryEnqueue(item.TenantId, $"{trigger}:{item.Reason}");
                }

                if (plan.Count > 0)
                {
                    _logger.LogInformation("租户菜单同步 {Trigger} 扫描入队 {Count} 个租户。", trigger, plan.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "租户菜单同步 {Trigger} 扫描失败。", trigger);
        }
    }

    /// <summary>
    /// 消费者循环：出队 → 解除去重占位 → 执行期复核范围 → Applier → 成功清零 / 异常退避延迟重入队。
    /// </summary>
    private async Task ConsumeLoopAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            TenantMenuSyncQueue.QueueItem item;
            try
            {
                item = await _queue.ReadAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ChannelClosedException)
            {
                // StopAsync 先 Complete 队列再等待执行任务：通道关闭属正常停机信号，优雅退出而非冒泡
                break;
            }

            _queue.MarkDequeued(item.TenantId);

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var applier = scope.ServiceProvider.GetRequiredService<TenantMenuSyncApplier>();
                    var tenant = await LoadEligibleTenantAsync(scope, item.TenantId);
                    if (tenant == null)
                    {
                        // 执行期已不在同步范围（删除 / 禁用 / 非Db）：丢弃本项，等待人工触发或状态行由删租户钩子清理
                        continue;
                    }

                    var result = await applier.SyncTenantAsync(tenant, item.Reason);
                    if (result.Succeeded || result.SkippedDangling)
                    {
                        _consecutiveFailures.TryRemove(item.TenantId, out _);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                var failures = _consecutiveFailures.AddOrUpdate(item.TenantId, 1, (_, count) => count + 1);
                var periods = Math.Min(failures, 3);
                var delaySeconds = _options.CurrentValue.ScanIntervalSeconds * periods;

                _logger.LogError(ex, "租户 {TenantId} 菜单同步失败（连续第 {Failures} 次），{Delay}s 后延迟重入队。", item.TenantId, failures, delaySeconds);

                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var applier = scope.ServiceProvider.GetRequiredService<TenantMenuSyncApplier>();
                        await applier.MarkFailedAsync(item.TenantId, ex.Message, delaySeconds);
                    }
                }
                catch (Exception markEx)
                {
                    _logger.LogError(markEx, "租户 {TenantId} 菜单同步失败状态落库异常。", item.TenantId);
                }

                _queue.ScheduleDelayed(item.TenantId, TriggerRetry, TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }

    /// <summary>
    /// 执行期单点复核同步范围（与 GetTenantDbListAsync 谓词同口径；guard 测试锁定两者等价）。
    /// </summary>
    private static async Task<SystemTenantEntity?> LoadEligibleTenantAsync(IServiceScope scope, long tenantId)
    {
        var mainDb = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>()
                          .AsTenant()
                          .GetConnectionScope(SqlSugarConst.MainConfigId);
        var tenant = await mainDb.Queryable<SystemTenantEntity>()
                                 .Filter(null, true)
                                 .FirstAsync(u => u.Id == tenantId);
        return IsInSyncScope(tenant) ? tenant : null;
    }

    /// <summary>
    /// 同步范围谓词（与 <see cref="SystemTenantService.GetTenantDbListAsync"/> 同口径）：
    /// TenantType=Db ∧ IsEnabled ∧ ConfigId != main。
    /// C354 T6 起默认租户（独立 ConfigId）纳入同步范围——Enterprise 套餐菜单启动幂等补全
    /// （AddSqlSugar → InitDefaultTenantDatabase）先于本 Worker 首扫（HostedService），
    /// 「补全先于纳入」保证默认租户收敛结果 = C189 全量、无振荡（守护测试锁定）。
    /// </summary>
    internal static bool IsInSyncScope(SystemTenantEntity? tenant)
    {
        return tenant != null
               && tenant.IsDeleted != true
               && tenant.IsEnabled == true
               && tenant.TenantType == TenantTypeEnum.Db
               && tenant.ConfigId != SqlSugarConst.MainConfigId;
    }
}
