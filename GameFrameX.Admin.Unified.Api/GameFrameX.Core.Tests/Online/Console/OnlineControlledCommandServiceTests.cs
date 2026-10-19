using GameFrameX.Application.Game.Online.Console;
using GameFrameX.Application.Game.Online.Console.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Tests.Online.Asset;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.Console;

// C344 / GFX-760（vault:C9 S8.1 / VC-8.4）：受控操作命令 Service 契约自检。
// 覆盖：四值命令路由（action 名 + 目标透传 + 禁填目标清空）、开关方向必填/单向禁填、
// 双人复核字段必填、目标组合错配拒绝、幂等键派生规则（方向参与、同参稳定、跨命令/方向/目标隔离）。
public sealed class OnlineControlledCommandServiceTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"controlled_cmd_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlineControlledCommandServiceTests()
    {
        var config = new ConnectionConfig
        {
            ConfigId = "online-controlled-command",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-controlled-command");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAreaEntity));
    }

    /// <summary>
    /// 四值命令路由：每命令映射正确 action 名、目标字段透传、命令不消费的目标字段清空下发。
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_RoutesAllFourCommands_ToTheirActions()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlineControlledCommandAckResponse { Status = "Accepted", });
        var service = CreateService(fake);

        await service.ExecuteAsync(new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            Command = OnlineControlledCommandKind.CancelTicket,
            TicketId = "ticket-1",
            Reason = "取消排队",
            ReviewerOperatorId = "reviewer-1",
        });
        await service.ExecuteAsync(new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            Command = OnlineControlledCommandKind.EndAbnormalMatch,
            MatchId = "match-1",
            Reason = "对局卡死",
            ReviewerOperatorId = "reviewer-1",
        });
        await service.ExecuteAsync(new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            Command = OnlineControlledCommandKind.SuspendResumePlayer,
            PlayerId = "player-9",
            Direction = ControlledCommandDirection.Apply,
            Reason = "暂停账号",
            ReviewerOperatorId = "reviewer-1",
        });
        await service.ExecuteAsync(new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            Command = OnlineControlledCommandKind.MatchIsolation,
            PlayerId = "player-9",
            Direction = ControlledCommandDirection.Lift,
            Reason = "解除隔离",
            ReviewerOperatorId = "reviewer-1",
        });

        Assert.Equal(4, fake.Calls.Count);
        var cancel = Assert.IsType<OnlineControlledCommandDispatchRequest>(fake.Calls[0].Request);
        Assert.Equal("cancel_match_ticket", fake.Calls[0].Action);
        Assert.Equal("cancel_match_ticket", cancel.Command);
        Assert.Equal("ticket-1", cancel.TicketId);
        Assert.Null(cancel.MatchId);
        Assert.Null(cancel.PlayerId);
        Assert.Null(cancel.Direction);

        var endMatch = Assert.IsType<OnlineControlledCommandDispatchRequest>(fake.Calls[1].Request);
        Assert.Equal("end_abnormal_match", fake.Calls[1].Action);
        Assert.Equal("match-1", endMatch.MatchId);
        Assert.Null(endMatch.TicketId);
        Assert.Null(endMatch.PlayerId);

        Assert.Equal("suspend_resume_player", fake.Calls[2].Action);
        var suspend = Assert.IsType<OnlineControlledCommandDispatchRequest>(fake.Calls[2].Request);
        Assert.Equal("player-9", suspend.PlayerId);
        Assert.Equal("Apply", suspend.Direction);

        Assert.Equal("match_isolation", fake.Calls[3].Action);
        var isolation = Assert.IsType<OnlineControlledCommandDispatchRequest>(fake.Calls[3].Request);
        Assert.Equal("player-9", isolation.PlayerId);
        Assert.Equal("Lift", isolation.Direction);

        foreach (var call in fake.Calls)
        {
            var body = Assert.IsType<OnlineControlledCommandDispatchRequest>(call.Request);
            Assert.False(string.IsNullOrWhiteSpace(body.Reason));
            Assert.Equal("reviewer-1", body.ReviewerOperatorId);
        }
    }

    /// <summary>
    /// 幂等键派生（VC-8.4）：同（命令,方向,目标,区服）两次调用键相同；方向不同键不同；单向命令无方向段；
    /// 区服不同键不同。键形态 onlineControlled-{command}[-{direction}]-{targetId}-{serverId}。
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_DerivesIdempotencyKey_ByCommandDirectionTargetServer()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db, secondArea: true);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        fake.SetPayload(new OnlineControlledCommandAckResponse { Status = "Accepted", });
        var service = CreateService(fake);

        var suspendApply = BuildSuspendRequest(ControlledCommandDirection.Apply, OnlineAssetTestSupport.Area1001);
        var suspendApplyAgain = BuildSuspendRequest(ControlledCommandDirection.Apply, OnlineAssetTestSupport.Area1001);
        var suspendLift = BuildSuspendRequest(ControlledCommandDirection.Lift, OnlineAssetTestSupport.Area1001);
        var suspendOtherServer = BuildSuspendRequest(ControlledCommandDirection.Apply, OnlineAssetTestSupport.Area1002);

        await service.ExecuteAsync(suspendApply);
        await service.ExecuteAsync(suspendApplyAgain);
        await service.ExecuteAsync(suspendLift);
        await service.ExecuteAsync(suspendOtherServer);
        await service.ExecuteAsync(new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            Command = OnlineControlledCommandKind.CancelTicket,
            TicketId = "ticket-1",
            Reason = "取消排队",
            ReviewerOperatorId = "reviewer-1",
        });

        var keys = fake.Calls.Select(c => Assert.IsType<OnlineControlledCommandDispatchRequest>(c.Request).IdempotencyKey).ToList();
        Assert.Equal(keys[0], keys[1]);
        Assert.Equal("onlineControlled-suspend_resume_player-apply-player-9-2001", keys[0]);
        Assert.Equal("onlineControlled-suspend_resume_player-lift-player-9-2001", keys[2]);
        Assert.Equal("onlineControlled-suspend_resume_player-apply-player-9-2002", keys[3]);
        Assert.Equal("onlineControlled-cancel_match_ticket-ticket-1-2001", keys[4]);
    }

    /// <summary>
    /// 开关型命令缺方向 → ParameterValidationFailed，Online 调用零次。
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_RejectsToggleCommandWithoutDirection()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteAsync(new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            Command = OnlineControlledCommandKind.SuspendResumePlayer,
            PlayerId = "player-9",
            Reason = "暂停账号",
            ReviewerOperatorId = "reviewer-1",
        }));

        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, exception.Code);
        Assert.Empty(fake.Calls);
    }

    /// <summary>
    /// 单向命令携带方向 → ParameterValidationFailed（方向只对开关型命令有意义）。
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_RejectsOneWayCommandWithDirection()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteAsync(new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            Command = OnlineControlledCommandKind.CancelTicket,
            TicketId = "ticket-1",
            Direction = ControlledCommandDirection.Apply,
            Reason = "取消排队",
            ReviewerOperatorId = "reviewer-1",
        }));

        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, exception.Code);
        Assert.Empty(fake.Calls);
    }

    /// <summary>
    /// 目标组合错配：EndAbnormalMatch 带 TicketId（非自身目标字段）→ ParameterValidationFailed；缺自身目标（MatchId）同样拒绝。
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_RejectsMismatchedOrMissingTargets()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var mismatched = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteAsync(new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            Command = OnlineControlledCommandKind.EndAbnormalMatch,
            MatchId = "match-1",
            TicketId = "ticket-extra",
            Reason = "对局卡死",
            ReviewerOperatorId = "reviewer-1",
        }));
        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, mismatched.Code);

        var missing = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteAsync(new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            Command = OnlineControlledCommandKind.EndAbnormalMatch,
            Reason = "对局卡死",
            ReviewerOperatorId = "reviewer-1",
        }));
        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, missing.Code);

        Assert.Empty(fake.Calls);
    }

    /// <summary>
    /// 双人复核字段：Reason / ReviewerOperatorId 空白 → ParameterValidationFailed，Online 调用零次。
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_RejectsMissingReasonOrReviewer()
    {
        await OnlineAssetTestSupport.SeedScopeAsync(_db);
        var fake = new OnlineAssetTestSupport.FakeOnlineServerClient();
        var service = CreateService(fake);

        var noReason = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteAsync(new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            Command = OnlineControlledCommandKind.CancelTicket,
            TicketId = "ticket-1",
            Reason = " ",
            ReviewerOperatorId = "reviewer-1",
        }));
        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, noReason.Code);

        var noReviewer = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteAsync(new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = OnlineAssetTestSupport.Area1001,
            Command = OnlineControlledCommandKind.CancelTicket,
            TicketId = "ticket-1",
            Reason = "取消排队",
            ReviewerOperatorId = string.Empty,
        }));
        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, noReviewer.Code);

        Assert.Empty(fake.Calls);
    }

    private static OnlineControlledCommandRequest BuildSuspendRequest(ControlledCommandDirection direction, long serverId)
    {
        return new OnlineControlledCommandRequest
        {
            AppId = OnlineAssetTestSupport.App100,
            ServerId = serverId,
            Command = OnlineControlledCommandKind.SuspendResumePlayer,
            PlayerId = "player-9",
            Direction = direction,
            Reason = "开关方向幂等键自检",
            ReviewerOperatorId = "reviewer-1",
        };
    }

    private OnlineControlledCommandService CreateService(IOnlineServerClient onlineServerClient)
    {
        var factory = new OnlineScopeContextFactory(OnlineAssetTestSupport.CreateUserContext(), OnlineAssetTestSupport.Repository<TenantGameAppEntity>(_db), OnlineAssetTestSupport.Repository<TenantGameAreaEntity>(_db));
        return new OnlineControlledCommandService(onlineServerClient, factory);
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
