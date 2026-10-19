using System.Collections.Generic;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;
using GameFrameX.Application.Game.Online.LiveOps.PlayerSegment;
using GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.Dto;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Game.Online;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.Online;
using GameFrameX.Foundation.Orm.Entity;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.LiveOps;

// C343 / GFX-755（vault:C8 S7.5-S7.6）：玩家分群服务测试（fake 单一入口）。
// 覆盖：条件白名单校验（字段越白名单拒 / 操作符越白名单拒 / 条件值空拒 / 白名单内收）、
// 新增默认 NotSynced、syncOnline 成功回写（Synced + Online 分配标识）、syncOnline 失败置 Failed 并重抛可重试、
// 被灰度版本行引用的分群不可删除。
public sealed class TenantGameOnlinePlayerSegmentServiceTests : IDisposable
{
    private const long TenantA = 1001;
    private const long App100 = 100;

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"online_segment_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;
    private readonly FakeDispatchService _dispatch = new FakeDispatchService();

    public TenantGameOnlinePlayerSegmentServiceTests()
    {
        var config = new ConnectionConfig
        {
            ConfigId = "online-segment",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-segment");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameOnlinePlayerSegmentEntity),
            typeof(TenantGameOnlineConfigRolloutEntity));
    }

    /// <summary>
    /// 条件字段越白名单（players 字段不存在）：新增即拒绝，不落库。
    /// </summary>
    [Fact]
    public async Task AddAsync_RejectsConditionFieldOutsideWhitelist()
    {
        await SeedAsync();
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(NewAdd("[{\"Field\":\"nickname\",\"Op\":\"eq\",\"Value\":\"bot\"}]")));

        Assert.Equal(20000002, exception.Code);
        Assert.Equal(0, await _db.Queryable<TenantGameOnlinePlayerSegmentEntity>().CountAsync());
    }

    /// <summary>
    /// 条件操作符越白名单（gt 不在 gte/lte/eq 内）：拒绝。
    /// </summary>
    [Fact]
    public async Task AddAsync_RejectsOperatorOutsideWhitelist()
    {
        await SeedAsync();
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(NewAdd("[{\"Field\":\"activeDays\",\"Op\":\"gt\",\"Value\":\"7\"}]")));
    }

    /// <summary>
    /// 条件值空：拒绝（空值条件无业务语义）。
    /// </summary>
    [Fact]
    public async Task AddAsync_RejectsEmptyConditionValue()
    {
        await SeedAsync();
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(NewAdd("[{\"Field\":\"activeDays\",\"Op\":\"gte\",\"Value\":\" \"}]")));
    }

    /// <summary>
    /// 白名单内条件可收：三字段 × 三操作符组合通过，初始 NotSynced。
    /// </summary>
    [Fact]
    public async Task AddAsync_AcceptsWhitelistedConditions_AndStartsNotSynced()
    {
        await SeedAsync();
        var service = CreateService();

        var response = await service.AddAsync(NewAdd(
            "[{\"Field\":\"activeDays\",\"Op\":\"gte\",\"Value\":\"7\"},{\"Field\":\"retentionDays\",\"Op\":\"lte\",\"Value\":\"30\"},{\"Field\":\"totalPaymentAmount\",\"Op\":\"eq\",\"Value\":\"0\"}]"));

        var persisted = await _db.Queryable<TenantGameOnlinePlayerSegmentEntity>().FirstAsync(u => u.Id == response.Id);
        Assert.NotNull(persisted);
        Assert.Equal(OnlinePlayerSegmentSyncStatus.NotSynced, persisted.SyncStatus);
        Assert.Equal(App100, persisted.AppId);
    }

    /// <summary>
    /// syncOnline 成功：Synced + Online 分配标识回写 + 回执版本透传。
    /// </summary>
    [Fact]
    public async Task SyncOnlineAsync_SuccessWritesBackOnlineSegmentId()
    {
        await SeedAsync();
        var service = CreateService();
        var segment = await service.AddAsync(NewAdd(null));
        _dispatch.NextOnlineSegmentId = 501;
        _dispatch.NextConfirmVersion = "seg-cv-9";

        var result = await service.SyncOnlineAsync(new SyncPlayerSegmentOnlineRequest
        {
            Id = segment.Id,
            ServerId = 2001,
            Reason = "unit-test-sync",
        });

        Assert.Equal(OnlinePlayerSegmentSyncStatus.Synced, result.SyncStatus);
        Assert.Equal(501, result.OnlineSegmentId);
        Assert.Equal("seg-cv-9", result.ConfirmVersion);

        var persisted = await _db.Queryable<TenantGameOnlinePlayerSegmentEntity>().FirstAsync(u => u.Id == segment.Id);
        Assert.Equal(OnlinePlayerSegmentSyncStatus.Synced, persisted.SyncStatus);
        Assert.Equal(501, persisted.LastOnlineSegmentId);
        Assert.Equal(2001, persisted.LastSyncServerId);
    }

    /// <summary>
    /// syncOnline 失败（Online 非 0 协议码镜像异常）：行置 Failed 落库可见后原样重抛，可重试。
    /// </summary>
    [Fact]
    public async Task SyncOnlineAsync_FailureMarksFailedAndRethrows()
    {
        await SeedAsync();
        var service = CreateService();
        var segment = await service.AddAsync(NewAdd(null));
        _dispatch.FailNextSync = true;

        await Assert.ThrowsAsync<BusinessException>(() => service.SyncOnlineAsync(new SyncPlayerSegmentOnlineRequest
        {
            Id = segment.Id,
            ServerId = 2001,
            Reason = "unit-test-sync",
        }));

        var persisted = await _db.Queryable<TenantGameOnlinePlayerSegmentEntity>().FirstAsync(u => u.Id == segment.Id);
        Assert.Equal(OnlinePlayerSegmentSyncStatus.Failed, persisted.SyncStatus);
    }

    /// <summary>
    /// 被灰度版本行引用的分群不可删除（版本行为不可变审计历史，引用不可悬空）。
    /// </summary>
    [Fact]
    public async Task DeleteAsync_RejectsSegmentReferencedByRollout()
    {
        await SeedAsync();
        var service = CreateService();
        var segment = await service.AddAsync(NewAdd(null));

        await _db.Insertable(new TenantGameOnlineConfigRolloutEntity
        {
            Id = 6001,
            AppId = App100,
            ServerId = 2001,
            ConfigKind = OnlineConfigRolloutKind.RemoteConfig,
            ConfigKey = "9001",
            ConfigVersion = 1,
            SegmentId = segment.Id,
            Percent = 50,
            Status = OnlineConfigRolloutStatus.RolledBack, // 即便已被替代的历史行也算引用
            TargetVersion = 0,
            Reason = "unit-test",
            DispatchOperator = "tester",
            PublishedTime = 1,
            CreatedTime = 1,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.DeleteAsync(new BaseDeleteRequest { Id = segment.Id, }));

        Assert.Equal(20000002, exception.Code);
    }

    private AddTenantGameOnlinePlayerSegmentRequest NewAdd(string? conditionsJson)
    {
        return new AddTenantGameOnlinePlayerSegmentRequest
        {
            AppId = App100,
            Name = "unit-segment",
            Description = null,
            ConditionsJson = conditionsJson,
        };
    }

    private TenantGameOnlinePlayerSegmentService CreateService()
    {
        return new TenantGameOnlinePlayerSegmentService(
            Repository<TenantGameOnlinePlayerSegmentEntity>(),
            Repository<TenantGameAppEntity>(),
            Repository<TenantGameOnlineConfigRolloutEntity>(),
            _dispatch);
    }

    private SqlSugarRepository<T> Repository<T>() where T : EntityBase, new()
    {
        return new SqlSugarRepository<T> { Context = _db, };
    }

    private async Task SeedAsync()
    {
        await _db.Insertable(new TenantGameAppEntity
        {
            Id = App100,
            Name = "app-100",
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            IsDeleted = false,
        }).ExecuteCommandAsync();
    }

    private sealed class FakeDispatchService : IOnlineLiveOpsDispatchService
    {
        public long NextOnlineSegmentId { get; set; }

        public string? NextConfirmVersion { get; set; }

        public bool FailNextSync { get; set; }

        public Task<OnlineLiveOpsDispatchResponse> DispatchAsync(OnlineLiveOpsDispatchRequest input)
        {
            throw new NotSupportedException("统一下发入口行为由 OnlineLiveOpsDispatchServiceTests 覆盖");
        }

        public Task EnsureSourceExistsAsync(LiveOpsSourceKind source, long businessKey, long appId)
        {
            return Task.CompletedTask;
        }

        public Task<OnlinePlayerSegmentSyncResponse> SyncSegmentToOnlineAsync(TenantGameOnlinePlayerSegmentEntity segment, long serverId, string reason)
        {
            if (FailNextSync)
            {
                throw BusinessError.Oh(OnlineErrorCode.StateOperationForbidden);
            }

            return Task.FromResult(new OnlinePlayerSegmentSyncResponse
            {
                OnlineAction = "syncPlayerSegment",
                OnlineSegmentId = NextOnlineSegmentId,
                ConfirmVersion = NextConfirmVersion,
                SyncedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            });
        }

        public Task<OnlineConfigRolloutDispatchResponse> PublishRolloutToOnlineAsync(TenantGameOnlineConfigRolloutEntity rollout)
        {
            throw new NotSupportedException("灰度发布行为由 TenantGameOnlineConfigRolloutServiceTests 覆盖");
        }

        public Task<OnlineConfigRolloutDispatchResponse> RollbackRolloutToOnlineAsync(TenantGameOnlineConfigRolloutEntity rollout)
        {
            throw new NotSupportedException("灰度回滚行为由 TenantGameOnlineConfigRolloutServiceTests 覆盖");
        }
    }

    public void Dispose()
    {
        _scopeOwner.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
