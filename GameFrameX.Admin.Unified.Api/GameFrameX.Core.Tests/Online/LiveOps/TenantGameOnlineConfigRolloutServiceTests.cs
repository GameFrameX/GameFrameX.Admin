using System.Collections.Generic;
using System.Security.Claims;
using GameFrameX.Application.Game.Online.LiveOps.ConfigRollout;
using GameFrameX.Application.Game.Online.LiveOps.ConfigRollout.Dto;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;
using GameFrameX.Core.Const;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Extension;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Game.Online;
using GameFrameX.Entity.Tenant.Game.Online;
using GameFrameX.Foundation.Orm.Entity;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.LiveOps;

// C343 / GFX-755（vault:C8 S7.7-S7.8 / VC-7.12）：配置灰度版本行服务测试（fake 单一入口，联调延后）。
// 覆盖：Percent 边界（0 拒 / 101 拒 / 1 与 100 收）、ConfigKey 数字校验、分群未同步拒绝、
// 版本递增分配 + 旧生效行置 RolledBack、回滚 = 版本化新行（TargetVersion 指向恢复目标 + 语义字段拷贝）、
// 回滚目标不存在拒绝、下发失败删行重抛（不留半生效审计行）。
public sealed class TenantGameOnlineConfigRolloutServiceTests : IDisposable
{
    private const long TenantA = 1001;
    private const long App100 = 100;
    private const long Area1001 = 2001;
    private const long SegmentSynced = 7001;

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"online_rollout_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;
    private readonly FakeDispatchService _dispatch = new FakeDispatchService();

    public TenantGameOnlineConfigRolloutServiceTests()
    {
        var config = new ConnectionConfig
        {
            ConfigId = "online-rollout",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-rollout");
        // 基础审计 AOP（同 SqlSugarClientConfig.AddSqlSugarClientService）：雪花 Id / CreatedTime / UpdateTime。
        // 版本行经 service.PublishAsync 多次插入，无 AOP 时主键恒 0 撞唯一约束。
        _db.Aop.DataExecuting = (oldValue, entityInfo) => SqlSugarAopHelper.HandleBaseAudit(oldValue, entityInfo);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameOnlineConfigRolloutEntity),
            typeof(TenantGameOnlinePlayerSegmentEntity));
    }

    /// <summary>
    /// Percent 边界：0（无意义）与 101（越界）拒绝，且不产生任何版本行。
    /// </summary>
    [Fact]
    public async Task PublishAsync_RejectsPercentZeroAndBeyondHundred()
    {
        await SeedAsync();
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() => service.PublishAsync(NewPublish(0)));
        await Assert.ThrowsAsync<BusinessException>(() => service.PublishAsync(NewPublish(101)));

        Assert.Equal(0, await _db.Queryable<TenantGameOnlineConfigRolloutEntity>().CountAsync());
    }

    /// <summary>
    /// Percent 合法边界：1 与 100 均可发布；版本号按配置维度递增，旧生效行置 RolledBack，Online 确认版本回写。
    /// </summary>
    [Fact]
    public async Task PublishAsync_AcceptsBoundariesOneAndHundred_AndVersionsIncrement()
    {
        await SeedAsync();
        var service = CreateService();

        var first = await service.PublishAsync(NewPublish(1));
        var second = await service.PublishAsync(NewPublish(100));

        Assert.Equal(1, first.ConfigVersion);
        Assert.Equal(2, second.ConfigVersion);
        Assert.Equal(OnlineConfigRolloutStatus.Published, second.Status);
        Assert.Equal("cv-publish", second.OnlineConfirmVersion); // fake 回执回写

        // 旧生效行被替代：v1 → RolledBack，v2 为唯一生效行
        var rows = await _db.Queryable<TenantGameOnlineConfigRolloutEntity>().OrderBy(u => u.ConfigVersion).ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(OnlineConfigRolloutStatus.RolledBack, rows[0].Status);
        Assert.Equal(OnlineConfigRolloutStatus.Published, rows[1].Status);
    }

    /// <summary>
    /// ConfigKey 非数字（非来源实体 Id）：拒绝（来源校验前置拦截）。
    /// </summary>
    [Fact]
    public async Task PublishAsync_RejectsNonNumericConfigKey()
    {
        await SeedAsync();
        var service = CreateService();

        var input = NewPublish(50);
        input.ConfigKey = "not-a-number";
        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.PublishAsync(input));

        Assert.Equal(20000002, exception.Code);
        Assert.Equal(0, _dispatch.PublishCalls);
    }

    /// <summary>
    /// 分群未同步 Online：拒绝发布（Online 无法解释未同步分群的命中）。
    /// </summary>
    [Fact]
    public async Task PublishAsync_RejectsUnsyncedSegment()
    {
        await SeedAsync();
        var service = CreateService();

        var input = NewPublish(50);
        input.SegmentId = 7002; // NotSynced
        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.PublishAsync(input));

        Assert.Equal(20000002, exception.Code);
        Assert.Equal(0, _dispatch.PublishCalls);
    }

    /// <summary>
    /// 回滚 = 版本化新下发：新行 ConfigVersion 续列（v3）、TargetVersion 指向恢复目标（v1）、
    /// SegmentId/Percent 从目标版本拷贝（恢复目标语义）、v2 置 RolledBack。
    /// </summary>
    [Fact]
    public async Task RollbackAsync_CreatesVersionedRowPointingAtTarget()
    {
        await SeedAsync();
        var service = CreateService();
        await service.PublishAsync(NewPublish(30)); // v1：30%
        await service.PublishAsync(NewPublish(80)); // v2：80%

        var rollback = await service.RollbackAsync(NewRollback(targetVersion: 1));

        Assert.Equal(3, rollback.ConfigVersion);
        Assert.Equal(1, rollback.TargetVersion);
        Assert.Equal(30, rollback.Percent); // 恢复 v1 语义
        Assert.Equal(SegmentSynced, rollback.SegmentId);
        Assert.Equal(OnlineConfigRolloutStatus.Published, rollback.Status);
        Assert.Equal("cv-rollback", rollback.OnlineConfirmVersion);

        var rows = await _db.Queryable<TenantGameOnlineConfigRolloutEntity>().OrderBy(u => u.ConfigVersion).ToListAsync();
        Assert.Equal(3, rows.Count);
        Assert.All(rows.Take(2), row => Assert.Equal(OnlineConfigRolloutStatus.RolledBack, row.Status)); // v1、v2 均被替代
        Assert.Equal(OnlineConfigRolloutStatus.Published, rows[2].Status); // v3 唯一生效
        Assert.Equal(1, _dispatch.RollbackCalls);
    }

    /// <summary>
    /// 回滚目标版本不存在：RecordNotFound，不产生新行。
    /// </summary>
    [Fact]
    public async Task RollbackAsync_RejectsUnknownTargetVersion()
    {
        await SeedAsync();
        var service = CreateService();
        await service.PublishAsync(NewPublish(30)); // 只有 v1

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.RollbackAsync(NewRollback(targetVersion: 42)));

        Assert.Equal(20000003, exception.Code);
        Assert.Equal(0, _dispatch.RollbackCalls);
    }

    /// <summary>
    /// 下发失败（Online 非 0 协议码镜像异常）：删除未确认版本行后原样重抛——无生效事实不留半生效审计行。
    /// </summary>
    [Fact]
    public async Task PublishAsync_DispatchFailureDeletesRowAndRethrows()
    {
        await SeedAsync();
        var service = CreateService();
        _dispatch.FailNextPublish = true;

        await Assert.ThrowsAsync<BusinessException>(() => service.PublishAsync(NewPublish(50)));

        Assert.Equal(0, await _db.Queryable<TenantGameOnlineConfigRolloutEntity>().CountAsync());
        // 失败重试：版本号未被污染，仍从 1 起
        _dispatch.FailNextPublish = false;
        var retry = await service.PublishAsync(NewPublish(50));
        Assert.Equal(1, retry.ConfigVersion);
    }

    private PublishConfigRolloutRequest NewPublish(int percent)
    {
        return new PublishConfigRolloutRequest
        {
            AppId = App100,
            ServerId = Area1001,
            ConfigKind = OnlineConfigRolloutKind.RemoteConfig,
            ConfigKey = "9001",
            SegmentId = SegmentSynced,
            Percent = percent,
            Reason = "unit-test",
        };
    }

    private RollbackConfigRolloutRequest NewRollback(int targetVersion)
    {
        return new RollbackConfigRolloutRequest
        {
            AppId = App100,
            ServerId = Area1001,
            ConfigKind = OnlineConfigRolloutKind.RemoteConfig,
            ConfigKey = "9001",
            TargetVersion = targetVersion,
            Reason = "unit-test-rollback",
        };
    }

    private TenantGameOnlineConfigRolloutService CreateService()
    {
        return new TenantGameOnlineConfigRolloutService(
            Repository<TenantGameOnlineConfigRolloutEntity>(),
            Repository<TenantGameOnlinePlayerSegmentEntity>(),
            _dispatch,
            CreateUserContext(TenantA));
    }

    private static CurrentUserContext CreateUserContext(long? tenantId)
    {
        var identity = new ClaimsIdentity("TestAuth");
        if (tenantId.HasValue)
        {
            identity.AddClaim(new Claim(ClaimConst.TenantId, tenantId.Value.ToString()));
        }

        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity), };
        var accessor = new HttpContextAccessor { HttpContext = httpContext, };
        return new CurrentUserContext(accessor);
    }

    private SqlSugarRepository<T> Repository<T>() where T : EntityBase, new()
    {
        return new SqlSugarRepository<T> { Context = _db, };
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.Insertable(new TenantGameOnlinePlayerSegmentEntity
        {
            Id = SegmentSynced,
            AppId = App100,
            Name = "synced-segment",
            SyncStatus = OnlinePlayerSegmentSyncStatus.Synced,
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameOnlinePlayerSegmentEntity
        {
            Id = 7002,
            AppId = App100,
            Name = "unsynced-segment",
            SyncStatus = OnlinePlayerSegmentSyncStatus.NotSynced,
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();
    }

    private sealed class FakeDispatchService : IOnlineLiveOpsDispatchService
    {
        public int PublishCalls { get; private set; }

        public int RollbackCalls { get; private set; }

        public bool FailNextPublish { get; set; }

        public Task<OnlineLiveOpsDispatchResponse> DispatchAsync(OnlineLiveOpsDispatchRequest input)
        {
            throw new NotSupportedException("统一下发入口行为由 OnlineLiveOpsDispatchServiceTests 覆盖");
        }

        public Task EnsureSourceExistsAsync(LiveOpsSourceKind source, long businessKey, long appId)
        {
            return Task.CompletedTask; // 来源校验行为由 OnlineLiveOpsDispatchServiceTests 覆盖
        }

        public Task<OnlinePlayerSegmentSyncResponse> SyncSegmentToOnlineAsync(TenantGameOnlinePlayerSegmentEntity segment, long serverId, string reason)
        {
            throw new NotSupportedException("分群同步行为由 TenantGameOnlinePlayerSegmentServiceTests 覆盖");
        }

        public Task<OnlineConfigRolloutDispatchResponse> PublishRolloutToOnlineAsync(TenantGameOnlineConfigRolloutEntity rollout)
        {
            PublishCalls += 1;
            if (FailNextPublish)
            {
                throw BusinessError.Oh(OnlineErrorCode.StateOperationForbidden);
            }

            return Task.FromResult(new OnlineConfigRolloutDispatchResponse
            {
                OnlineAction = "publishConfigRollout",
                ConfirmVersion = "cv-publish",
                DispatchedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            });
        }

        public Task<OnlineConfigRolloutDispatchResponse> RollbackRolloutToOnlineAsync(TenantGameOnlineConfigRolloutEntity rollout)
        {
            RollbackCalls += 1;
            return Task.FromResult(new OnlineConfigRolloutDispatchResponse
            {
                OnlineAction = "rollbackConfigRollout",
                ConfirmVersion = "cv-rollback",
                DispatchedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            });
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
