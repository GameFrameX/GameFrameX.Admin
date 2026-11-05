using GameFrameX.Application.Platform.Job;
using GameFrameX.Core.Job;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace GameFrameX.Core.Tests.Platform.Job;
/// <summary>
/// 业务定时任务扫描派发作业（GameTimeJobDispatchJob）语义测试——调度合一后的行为契约：
/// cron 到期执行、IsOnce 只执行一次且删行、增删改后调度随之更新、任务名解析失败不空转。
/// <para>时钟与类型解析器均经 internal 钩子替换，测试内直接调用 ExecuteAsync（引擎侧由
/// SchedulerTriggerLoopTests 覆盖）。与 BusinessJobTypeResolutionTests 共享静态钩子，同 Collection 禁止并行。</para>
/// </summary>
[Collection(nameof(GameTimeJobDispatchJobTests))]
public sealed class GameTimeJobDispatchJobTests : IDisposable
{
    private readonly ClientApiTestDb _database = new();
    private readonly RecordingCache _cache = new();
    private readonly ServiceProvider _provider;
    private readonly GameTimeJobDispatchJob _dispatcher;
    private DateTime _fakeNow = new(2026, 11, 5, 10, 0, 0);

    public GameTimeJobDispatchJobTests()
    {
        _database.InitTenantTables(typeof(TenantGameTimeJobEntity));

        var services = new ServiceCollection();
        services.AddSingleton<ISqlSugarClient>(_database.MainDb);
        services.AddScoped(_ => new SqlSugarRepository<TenantGameTimeJobEntity> { Context = _database.MainDb, });
        services.AddSingleton<GameFrameX.Core.Extension.Cache.ICache>(_cache);
        services.AddSingleton<IOptions<CacheOptions>>(Options.Create(new CacheOptions()));
        services.AddSingleton<SystemCacheService>();
        services.AddTransient<ProbeScheduleJob>();
        _provider = services.BuildServiceProvider();

        _dispatcher = new GameTimeJobDispatchJob(_provider.GetRequiredService<IServiceScopeFactory>(),
                                                 NullLogger<GameTimeJobDispatchJob>.Instance);

        GameTimeJobDispatchJob.ResetTrackingForTests();
        GameTimeJobDispatchJob.BusinessJobTypeResolver = name => name == "probe" ? typeof(ProbeScheduleJob) : null;
        GameTimeJobDispatchJob.Clock = () => _fakeNow;
        ProbeScheduleJob.ReceivedMailIdentifiers.Clear();
    }

    public void Dispose()
    {
        GameTimeJobDispatchJob.ResetRuntimeHooksForTests();
        GameTimeJobDispatchJob.ResetTrackingForTests();
        _provider.Dispose();
        _database.Dispose();
    }

    [Fact]
    public async Task CronDue_ExecutesOncePerOccurrence()
    {
        InsertRow(9001, cron: "* * * * * *", isOnce: false, mailId: 777);

        // 首次扫描：初始化 NextRun（下一个整秒），不执行——与旧引擎"加载时从当下起算"语义一致。
        await DispatchAsync();
        Assert.Empty(ProbeScheduleJob.ReceivedMailIdentifiers);

        // 时间越过下一秒：到期执行一次，MailId 经缓存通道传入。
        _fakeNow = _fakeNow.AddSeconds(2);
        await DispatchAsync();
        Assert.Equal(new[] { 777L, }, ProbeScheduleJob.ReceivedMailIdentifiers);

        // 同一周期内重复扫描不重复执行（NextRun 已推进到下一秒）。
        _fakeNow = _fakeNow.AddMilliseconds(500);
        await DispatchAsync();
        Assert.Single(ProbeScheduleJob.ReceivedMailIdentifiers);
        Assert.Equal(1, CountRows());
    }

    [Fact]
    public async Task IsOnceTask_ExecutesOnceAndDeletesRow()
    {
        InsertRow(9002, cron: "* * * * * *", isOnce: true, mailId: 888);

        await DispatchAsync();
        _fakeNow = _fakeNow.AddSeconds(2);
        await DispatchAsync();

        Assert.Equal(new[] { 888L, }, ProbeScheduleJob.ReceivedMailIdentifiers);
        Assert.Equal(0, CountRows());

        // 行已删除：此后即使时间推进也不再执行（只执行一次语义）。
        _fakeNow = _fakeNow.AddSeconds(5);
        await DispatchAsync();
        Assert.Single(ProbeScheduleJob.ReceivedMailIdentifiers);
    }

    [Fact]
    public async Task ScheduleChanges_AppliedWithoutRestart()
    {
        // 修改：远期 cron（每年一次）初始化后，改成每秒触发——指纹变化重置 NextRun，随后到期执行。
        InsertRow(9003, cron: "0 0 3 1 1 *", isOnce: false, mailId: 1);
        await DispatchAsync();
        _fakeNow = _fakeNow.AddSeconds(10);
        await DispatchAsync();
        Assert.Empty(ProbeScheduleJob.ReceivedMailIdentifiers);

        UpdateRowCron(9003, "* * * * * *");
        _fakeNow = _fakeNow.AddSeconds(2);
        await DispatchAsync();
        // 变更后从当下重算：本 tick 尚未到期。
        Assert.Empty(ProbeScheduleJob.ReceivedMailIdentifiers);

        _fakeNow = _fakeNow.AddSeconds(1);
        await DispatchAsync();
        Assert.Single(ProbeScheduleJob.ReceivedMailIdentifiers);
    }

    [Fact]
    public async Task AddedRow_PickedUpWithoutRestart()
    {
        await DispatchAsync();
        Assert.Empty(ProbeScheduleJob.ReceivedMailIdentifiers);

        // 新增：新行进入跟踪，一个周期后到期执行。
        InsertRow(9004, cron: "* * * * * *", isOnce: false, mailId: 2);
        _fakeNow = _fakeNow.AddSeconds(5);
        await DispatchAsync();
        Assert.Empty(ProbeScheduleJob.ReceivedMailIdentifiers);

        _fakeNow = _fakeNow.AddSeconds(1);
        await DispatchAsync();
        Assert.Equal(new[] { 2L, }, ProbeScheduleJob.ReceivedMailIdentifiers);
    }

    [Fact]
    public async Task DeletedRow_DroppedFromSchedule()
    {
        InsertRow(9005, cron: "* * * * * *", isOnce: false, mailId: 3);
        await DispatchAsync();

        // 删除：行消失后调度移除，时间推进也不执行。
        _database.MainDb.Deleteable<TenantGameTimeJobEntity>().Where(row => row.Id == 9005).ExecuteCommand();
        _fakeNow = _fakeNow.AddSeconds(10);
        await DispatchAsync();

        Assert.Empty(ProbeScheduleJob.ReceivedMailIdentifiers);
    }

    [Fact]
    public async Task UnresolvableName_DoesNotExecuteAndDoesNotSpin()
    {
        InsertRow(9006, cron: "* * * * * *", isOnce: false, mailId: 4, name: "missing");

        await DispatchAsync();
        _fakeNow = _fakeNow.AddSeconds(2);
        await DispatchAsync();
        _fakeNow = _fakeNow.AddSeconds(2);
        await DispatchAsync();

        // 解析失败不执行、不抛错（告警路径）；后续扫描 NextRun 已推进，不再是每轮到期。
        Assert.Empty(ProbeScheduleJob.ReceivedMailIdentifiers);
    }

    private Task DispatchAsync()
    {
        return _dispatcher.ExecuteAsync(new JobExecutingContext(), CancellationToken.None);
    }

    private void InsertRow(long rowId, string cron, bool isOnce, long mailId, string name = "probe")
    {
        _database.MainDb.Insertable(new TenantGameTimeJobEntity
        {
            Id = rowId,
            Name = name,
            Description = $"业务定时任务 {rowId}",
            Cron = cron,
            IsOnce = isOnce,
            MailId = mailId,
            Source = "Manual",
            IsEnabled = true,
            IsDeleted = false,
        }).ExecuteCommand();
    }

    private void UpdateRowCron(long rowId, string cron)
    {
        _database.MainDb.Updateable<TenantGameTimeJobEntity>()
                 .SetColumns(row => row.Cron == cron)
                 .Where(row => row.Id == rowId)
                 .ExecuteCommand();
    }

    private int CountRows()
    {
        return _database.MainDb.Queryable<TenantGameTimeJobEntity>().Count();
    }

    /// <summary>
    /// 记录 MailId 的探针任务（模拟 EmailTask 从 SystemCacheService 取值的通道）。
    /// </summary>
    private sealed class ProbeScheduleJob : IScheduleJob
    {
        public static readonly List<long> ReceivedMailIdentifiers = new();

        private readonly SystemCacheService _systemCacheService;

        public ProbeScheduleJob(SystemCacheService systemCacheService)
        {
            _systemCacheService = systemCacheService;
        }

        public Task<Task> ExecuteAsync(CancellationToken cancellationToken)
        {
            ReceivedMailIdentifiers.Add((long)_systemCacheService.GetValue("MailId")!);
            return Task.FromResult(Task.CompletedTask);
        }

        public Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
        {
            return ExecuteAsync(stoppingToken);
        }
    }

    /// <summary>
    /// 可读回的缓存（FakeCache 不存值，无法验证 MailId 通道）。
    /// </summary>
    private sealed class RecordingCache : GameFrameX.Core.Extension.Cache.ICache
    {
        private readonly Dictionary<string, object> _values = new();

        public ICollection<string> Keys
        {
            get { return _values.Keys.ToArray(); }
        }

        public T Get<T>(string key)
        {
            return _values.TryGetValue(key, out var value) ? (T)value : default!;
        }

        public bool Set(string key, object value)
        {
            _values[key] = value;
            return true;
        }

        public bool Set(string key, object value, TimeSpan expire)
        {
            return Set(key, value);
        }

        public void Add(string key, object value)
        {
            _values[key] = value;
        }

        public int Remove(string key)
        {
            return _values.Remove(key) ? 1 : 0;
        }

        public int Remove(string[] keys)
        {
            return keys.Count(key => Remove(key) > 0);
        }

        public bool ContainsKey(string key)
        {
            return _values.ContainsKey(key);
        }

        public T GetOrAdd<T>(string key, Func<string, T> callback, int expire = -1)
        {
            if (_values.TryGetValue(key, out var value))
            {
                return (T)value;
            }

            var added = callback(key);
            _values[key] = added!;
            return added;
        }

        public bool SetExpire(string key, TimeSpan expire)
        {
            return true;
        }

        public IDisposable AcquireLock(string key, int msTimeout)
        {
            return null!;
        }

        public IDisposable AcquireLock(string key, int msTimeout, int msExpire, bool waitFor)
        {
            return null!;
        }

        public IEnumerable<string> Search(string pattern, int maxCount)
        {
            return Array.Empty<string>();
        }
    }
}
