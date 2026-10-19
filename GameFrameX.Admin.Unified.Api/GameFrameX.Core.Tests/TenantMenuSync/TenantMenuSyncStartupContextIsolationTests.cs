using System.Collections.Concurrent;
using System.Reflection;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Config;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.TenantMenuSync;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.System;
using GameFrameX.Entity.System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace GameFrameX.Core.Tests.TenantMenuSync;

/// <summary>
/// GFX-872 / C380 回归：宿主启动链 ExecutionContext 若携带共享 SqlSugarScope AsyncLocal 槽位
/// （docker 栈 2026-09-27 实测：同 client 跨流并发 → NpgsqlOperationInProgressException /
/// SqlSugar MappingColumns "Collection was modified"），Worker 的扫描与消费者不得继承该槽位。
/// 修复不变量：执行体在 ExecutionContext.SuppressFlow() 下 fork，子流从默认上下文起步、
/// 槽位为空、首次访问自建独立 client。判别手段：把槽位预置为一个连接串必然失败的 client
/// （损坏 client）——修复前扫描/消费者继承槽位复用损坏 client，startup 扫描必失败并留下
/// LogError 记录；修复后扫描走自建 client 成功，零扫描错误。
/// </summary>
[Collection("MenuSeedCatalog")]
public sealed class TenantMenuSyncStartupContextIsolationTests
{
    [Fact]
    public async Task 启动链槽位携带损坏client时扫描与消费者不得继承()
    {
        TenantMenuSyncTestHost host = null!;
        TenantMenuSyncWorker worker = null!;
        var logger = new RecordingLogger();
        try
        {
            // 准备阶段整体在 Task.Run 的线程池同步栈上执行：TenantMenuSyncTestHost 构造与其 MainDb 属性
            // （经 SqlSugarScope.GetConnectionScope 分发）在同步栈走 ThreadLocal 路径，不触碰测试方法的
            // AsyncLocal 槽位，前置断言才不被测试自身污染。
            worker = await Task.Run(() =>
            {
                host = new TenantMenuSyncTestHost();
                // 保证菜单目录已加载：启动扫描走「目录就绪」分支才会触达 SqlSugar，回归才有判别力。
                MenuSeedCatalog.LoadFromDb(host.MainDb);
                return new TenantMenuSyncWorker(
                    new TenantMenuSyncQueue(),
                    BuildScopeFactory(host),
                    new StaticOptionsMonitor(new TenantMenuSyncOptions { WorkerConcurrency = 2, ScanIntervalSeconds = 60, }),
                    logger);
            });

            var scopeSlotKey = ScopeSlotKey(host);
            var ambientSlotBefore = CallContextAsync<SqlSugarClient>.GetData(scopeSlotKey);
            Assert.Null(ambientSlotBefore);

            // 模拟生产启动链污染：槽位预置损坏 client（连接串指向不存在目录，任何查询即抛错）。
            var corruptedClient = new SqlSugarClient(new List<ConnectionConfig>
            {
                new ConnectionConfig
                {
                    ConfigId = SqlSugarConst.MainConfigId,
                    DbType = DbType.Sqlite,
                    ConnectionString = "DataSource=/nonexistent-gfx872-isolation-dir/ corrupted.db",
                    IsAutoCloseConnection = true,
                },
            });
            CallContextAsync<SqlSugarClient>.SetData(scopeSlotKey, corruptedClient);

            await worker.StartAsync(CancellationToken.None);
            // 给 startup 扫描留出执行窗口（修复前：损坏 client 上 state 查询立即失败并 LogError）。
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            Assert.True(logger.ScanFailureCount == 0,
                $"启动扫描不得复用启动链槽位里的共享（损坏）client。扫描错误记录：{logger.DescribeScanFailures()}");

            await worker.StopAsync(CancellationToken.None);

            // Worker 内部各流的 SqlSugar 写入不得回流调用方 ExecutionContext：损坏 client 占位保持原样。
            var ambientSlotAfter = CallContextAsync<SqlSugarClient>.GetData(scopeSlotKey);
            Assert.Same(corruptedClient, ambientSlotAfter);
        }
        finally
        {
            worker?.Dispose();
            host?.Dispose();
        }
    }

    /// <summary>
    /// 记录型 ILogger：捕获 ScanOnceAsync 失败路径的 LogError（消息含「扫描失败」），供断言判别。
    /// </summary>
    private sealed class RecordingLogger : ILogger<TenantMenuSyncWorker>
    {
        private readonly ConcurrentQueue<string> _scanFailures = new();

        public int ScanFailureCount => _scanFailures.Count;

        public string DescribeScanFailures()
        {
            return string.Join(" | ", _scanFailures.Take(3));
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            if (logLevel >= LogLevel.Error && message.Contains("扫描失败", StringComparison.Ordinal))
            {
                _scanFailures.Enqueue(exception == null ? message : $"{message}: {exception.GetType().Name} {exception.Message}");
            }
        }
    }

    /// <summary>
    /// 读取 SqlSugarScope 的 AsyncLocal 槽位 key（= _configs.GetHashCode()，私有字段反射一次取得）。
    /// </summary>
    private static string ScopeSlotKey(TenantMenuSyncTestHost host)
    {
        var configs = (List<ConnectionConfig>?)typeof(SqlSugarScope)
            .GetField("_configs", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(host.Client);
        Assert.NotNull(configs);
        return configs!.GetHashCode().ToString();
    }

    private static IServiceScopeFactory BuildScopeFactory(TenantMenuSyncTestHost host)
    {
        SqlSugarCache.Configure(new FakeCache());
        var cacheService = new SystemCacheService(new FakeCache(), Options.Create(new CacheOptions()));
        var mainConfig = new DbConnectionConfig
        {
            ConfigId = SqlSugarConst.MainConfigId,
            DbType = DbType.Sqlite,
            ConnectionString = host.MainDb.Ado.Connection.ConnectionString,
            IsAutoCloseConnection = true,
        };
        var dbOptions = Options.Create(new DbConnectionOptions { ConnectionConfigs = new List<DbConnectionConfig> { mainConfig, }, });
        var tenantService = new SystemTenantService(
            new SqlSugarRepository<SystemTenantEntity> { Context = host.MainDb, },
            new SqlSugarRepository<SystemTenantMenuSyncStateEntity> { Context = host.MainDb, },
            new SystemConfigService(
                new SqlSugarRepository<SystemConfigEntity> { Context = host.MainDb, },
                cacheService),
            cacheService,
            new TenantDbConnectionProvider(
                host.Client,
                cacheService,
                new DelegateSqlSugarTenantIdResolver(() => null),
                dbOptions,
                new HttpContextAccessor()),
            new PostgreSqlTenantDatabaseProvisioner(),
            NullLogger<SystemTenantService>.Instance,
            dbOptions,
            host.Client,
            new EmptyServiceProvider(),
            new TenantMenuSyncQueue());
        var provider = new StubServiceProvider(host.Client, tenantService, host.BuildApplier());
        return new StubScopeFactory(provider);
    }

    /// <summary>
    /// 固定值 IOptionsMonitor 桩（测试项目无现成实现）。
    /// </summary>
    private sealed class StaticOptionsMonitor : IOptionsMonitor<TenantMenuSyncOptions>
    {
        private readonly TenantMenuSyncOptions _options;

        public StaticOptionsMonitor(TenantMenuSyncOptions options)
        {
            _options = options;
        }

        public TenantMenuSyncOptions CurrentValue => _options;

        public IDisposable? OnChange(Action<TenantMenuSyncOptions, string?> listener)
        {
            return null;
        }

        public TenantMenuSyncOptions Get(string? name)
        {
            return _options;
        }
    }

    /// <summary>
    /// 仅解析 TenantMenuSyncWorker 扫描/消费路径触达的三类服务，其余返回 null。
    /// </summary>
    private sealed class StubServiceProvider : IServiceProvider
    {
        private readonly ISqlSugarClient _client;
        private readonly SystemTenantService _tenantService;
        private readonly TenantMenuSyncApplier _applier;

        public StubServiceProvider(ISqlSugarClient client, SystemTenantService tenantService, TenantMenuSyncApplier applier)
        {
            _client = client;
            _tenantService = tenantService;
            _applier = applier;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(ISqlSugarClient))
            {
                return _client;
            }

            if (serviceType == typeof(SystemTenantService))
            {
                return _tenantService;
            }

            if (serviceType == typeof(TenantMenuSyncApplier))
            {
                return _applier;
            }

            return null;
        }
    }

    private sealed class StubScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceProvider _provider;

        public StubScopeFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IServiceScope CreateScope()
        {
            return new StubScope(_provider);
        }

        private sealed class StubScope : IServiceScope
        {
            public StubScope(IServiceProvider provider)
            {
                ServiceProvider = provider;
            }

            public IServiceProvider ServiceProvider { get; }

            public void Dispose()
            {
            }
        }
    }

    /// <summary>
    /// PageAsync 不解析 <see cref="IServiceProvider"/>；保留扩展点用（同 SystemTenantServicePageMenuSyncHydrateTests 先例）。
    /// </summary>
    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            return null;
        }
    }
}
