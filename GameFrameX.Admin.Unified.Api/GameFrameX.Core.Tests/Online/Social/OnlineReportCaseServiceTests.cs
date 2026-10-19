using System.Collections.Generic;
using System.Security.Claims;
using GameFrameX.Application.Game.Online.Social;
using GameFrameX.Application.Game.Online.Social.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty;
using GameFrameX.Core.Const;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Orm.Entity;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.Online.Social;

// C350 / GFX-754（vault:C7）：举报案件处置流契约自检（三步流：事实源写入 → 联动命令 → 通知/案件回写）。
// 覆盖：Mute 处置落账事实源（审计字段服务端强制）+ 联动 + 通知 + 回写全链成功；
// Dismiss 不写事实源不通知；联动失败不回滚事实源且后续步骤继续（联动可重试）；
// 案件回写失败落响应原因不阻断；Mute 缺时长服务端兜底拒绝；Page 过滤透传与游标往返。
public sealed class OnlineReportCaseServiceTests : IDisposable
{
    private const long TenantA = 4001;
    private const long App100 = 100;
    private const long Area1001 = 2001;
    private const long OperatorUserId = 77;

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"report_case_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlineReportCaseServiceTests()
    {
        // 专属 ConfigId：防静态实体信息缓存与其它 fixture 互抢（同 OnlineServerClientTests ctor 注释）。
        var config = new ConnectionConfig
        {
            ConfigId = "report-case",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("report-case");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAreaEntity),
            typeof(TenantGamePenaltyEntity));
    }

    /// <summary>
    /// Mute 处置全链成功：处罚事实源落账（审计字段强制）→ mute_player 联动 → 当事人通知 → 案件回写 Resolved。
    /// </summary>
    [Fact]
    public async Task HandleAsync_MuteDecision_WritesPenaltyFactThenLinksThenUpdatesCase()
    {
        await SeedScopeAsync();
        var fake = new FakeOnlineServerClient();
        var service = CreateService(fake);

        var response = await service.HandleAsync(new OnlineReportHandleRequest
        {
            AppId = App100,
            ServerId = Area1001,
            CaseId = "case-0001",
            ReportedPlayerId = "player-9",
            Decision = OnlineReportDecisionKind.Mute,
            Reason = "辱骂他人",
            DurationMinutes = 60,
        });

        // 事实源：Mute 处罚一行，审计字段由服务端强制（Active + 当前操作人）。
        var penalty = await _db.Queryable<TenantGamePenaltyEntity>().FirstAsync(u => u.TargetId == "player-9");
        Assert.NotNull(penalty);
        Assert.Equal(App100, penalty.AppId);
        Assert.Equal(Area1001, penalty.AreaId);
        Assert.Equal(PenaltyTargetTypeEnum.Account, penalty.TargetType);
        Assert.Equal(PenaltyActionTypeEnum.Mute, penalty.ActionType);
        Assert.Equal(PenaltyStatusEnum.Active, penalty.Status);
        Assert.Equal(OperatorUserId, penalty.OperatorId);
        Assert.Contains("case-0001", penalty.Reason);
        Assert.Equal(response.HandledAt + 60 * 60L, penalty.ExpireTime);

        // 联动 + 通知 + 回写：mute_player 先于 send_admin_notification / update_report_case。
        Assert.Equal(new[] { "mute_player", "send_admin_notification", "update_report_case" },
                     fake.Calls.Select(u => u.Action).ToArray());
        var linkBody = Assert.IsType<OnlineLinkCommandRequest>(fake.Calls[0].Request);
        Assert.Equal(penalty.Id, linkBody.PenaltyId);
        var caseUpdate = Assert.IsType<OnlineReportCaseUpdateRequest>(fake.Calls[2].Request);
        Assert.Equal("case-0001", caseUpdate.CaseId);
        Assert.Equal(OnlineReportCaseStatusKind.Resolved, caseUpdate.Status);
        Assert.Contains($"处罚Id {penalty.Id}", caseUpdate.HandleResult);

        Assert.Equal(penalty.Id, response.PenaltyId);
        Assert.True(response.LinkCommand!.AllSucceeded);
        Assert.True(response.PlayerNotified);
        Assert.True(response.CaseUpdated);
    }

    /// <summary>
    /// Dismiss 处置：不写事实源、不发联动、不打扰当事人，仅案件回写 Rejected。
    /// </summary>
    [Fact]
    public async Task HandleAsync_DismissDecision_WritesNoPenaltyFactAndSkipsNotification()
    {
        await SeedScopeAsync();
        var fake = new FakeOnlineServerClient();
        var service = CreateService(fake);

        var response = await service.HandleAsync(new OnlineReportHandleRequest
        {
            AppId = App100,
            ServerId = Area1001,
            CaseId = "case-0002",
            ReportedPlayerId = "player-9",
            Decision = OnlineReportDecisionKind.Dismiss,
            Reason = "证据不足",
        });

        Assert.Equal(0, await _db.Queryable<TenantGamePenaltyEntity>().CountAsync(u => u.TargetId == "player-9"));
        Assert.Equal(new[] { "update_report_case" }, fake.Calls.Select(u => u.Action).ToArray());
        Assert.Equal(OnlineReportCaseStatusKind.Rejected, Assert.IsType<OnlineReportCaseUpdateRequest>(fake.Calls[0].Request).Status);

        Assert.Null(response.PenaltyId);
        Assert.Null(response.LinkCommand);
        Assert.False(response.PlayerNotified);
        Assert.True(response.CaseUpdated);
    }

    /// <summary>
    /// 联动失败（Online 业务码）：事实源不回滚（PenaltyId 已落响应）、通知与案件回写继续——联动可重试语义。
    /// </summary>
    [Fact]
    public async Task HandleAsync_LinkFailure_DoesNotRollbackFactAndStillNotifies()
    {
        await SeedScopeAsync();
        var fake = new FakeOnlineServerClient();
        fake.FailOn("revoke_token", BusinessError.Oh(OnlineErrorCode.StateOperationForbidden));
        var service = CreateService(fake);

        var response = await service.HandleAsync(new OnlineReportHandleRequest
        {
            AppId = App100,
            ServerId = Area1001,
            CaseId = "case-0003",
            ReportedPlayerId = "player-9",
            Decision = OnlineReportDecisionKind.Ban,
            Reason = "恶意破坏对局",
            DurationMinutes = 1440,
        });

        Assert.NotNull(response.PenaltyId);
        Assert.NotNull(await _db.Queryable<TenantGamePenaltyEntity>().FirstAsync(u => u.Id == response.PenaltyId!.Value));
        Assert.False(response.LinkCommand!.AllSucceeded);
        Assert.Contains("send_admin_notification", fake.Calls.Select(u => u.Action));
        Assert.Contains("update_report_case", fake.Calls.Select(u => u.Action));
        Assert.True(response.PlayerNotified);
        Assert.True(response.CaseUpdated);
    }

    /// <summary>
    /// 案件回写失败：处置本身不失败——CaseUpdated=false + 原因落响应字段（提示人工重查）。
    /// </summary>
    [Fact]
    public async Task HandleAsync_CaseUpdateFailure_ReportsReasonWithoutFailing()
    {
        await SeedScopeAsync();
        var fake = new FakeOnlineServerClient();
        fake.FailOn("update_report_case", new InvalidOperationException("envelope broken"));
        var service = CreateService(fake);

        var response = await service.HandleAsync(new OnlineReportHandleRequest
        {
            AppId = App100,
            ServerId = Area1001,
            CaseId = "case-0004",
            ReportedPlayerId = "player-9",
            Decision = OnlineReportDecisionKind.Warn,
            Reason = "轻度违规",
        });

        Assert.False(response.CaseUpdated);
        Assert.NotNull(response.CaseUpdateFailureReason);
        Assert.Contains("envelope broken", response.CaseUpdateFailureReason);
        Assert.True(response.PlayerNotified);
    }

    /// <summary>
    /// Mute / Ban 缺时长：服务端兜底拒绝（DTO Range 校验不覆盖必填语义）。
    /// </summary>
    [Fact]
    public async Task HandleAsync_MuteWithoutDuration_RejectedByServer()
    {
        await SeedScopeAsync();
        var service = CreateService(new FakeOnlineServerClient());

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.HandleAsync(new OnlineReportHandleRequest
        {
            AppId = App100,
            ServerId = Area1001,
            CaseId = "case-0005",
            ReportedPlayerId = "player-9",
            Decision = OnlineReportDecisionKind.Mute,
            Reason = "辱骂他人",
            DurationMinutes = null,
        }));

        Assert.Equal(CommonErrorCode.ParameterValidationFailed, Assert.IsType<CommonErrorCode>(exception.ErrorCode));
    }

    /// <summary>
    /// Page：过滤条件与游标透传 Online，案件行同构映射返回（NextCursor / HasMore 往返）。
    /// </summary>
    [Fact]
    public async Task PageAsync_PassesThroughFiltersAndCursor()
    {
        await SeedScopeAsync();
        var fake = new FakeOnlineServerClient();
        fake.Respond("query_report_cases", new OnlineReportCaseQueryResultResponse
        {
            Items = new List<OnlineReportCaseResponse>
            {
                new()
                {
                    CaseId = "case-0001",
                    AppId = App100,
                    ServerId = Area1001,
                    ReporterPlayerId = "player-1",
                    ReportedPlayerId = "player-9",
                    Scene = "chat",
                    MatchId = null,
                    ChatMessageId = "msg-1",
                    Reason = "abuse",
                    Evidence = null,
                    Status = OnlineReportCaseStatusKind.Pending,
                    HandleResult = null,
                    CreatedAt = 1780000000,
                    UpdatedAt = 1780000000,
                },
            },
            NextCursor = "cursor-2",
            HasMore = true,
        });
        var service = CreateService(fake);

        var page = await service.PageAsync(new PageOnlineReportCaseRequest
        {
            AppId = App100,
            ServerId = Area1001,
            Status = OnlineReportCaseStatusKind.Pending,
            ReporterPlayerId = "player-1",
            Cursor = "cursor-1",
            PageSize = 20,
        });

        var query = Assert.IsType<OnlineReportCaseQueryRequest>(Assert.Single(fake.Calls).Request);
        Assert.Equal(OnlineReportCaseStatusKind.Pending, query.Status);
        Assert.Equal("player-1", query.ReporterPlayerId);
        Assert.Equal("cursor-1", query.Cursor);
        Assert.Equal(20, query.PageSize);

        var item = Assert.Single(page.Items);
        Assert.Equal("case-0001", item.CaseId);
        Assert.Equal("player-9", item.ReportedPlayerId);
        Assert.Equal("msg-1", item.ChatMessageId);
        Assert.Equal("cursor-2", page.NextCursor);
        Assert.True(page.HasMore);
    }

    private OnlineReportCaseService CreateService(IOnlineServerClient onlineServerClient)
    {
        var userContext = CreateUserContext(TenantA, OperatorUserId);
        var factory = new OnlineScopeContextFactory(userContext, Repository<TenantGameAppEntity>(), Repository<TenantGameAreaEntity>());
        var linkService = new OnlinePenaltyLinkCommandService(
            Repository<TenantGamePenaltyEntity>(),
            Repository<TenantGameAreaEntity>(),
            onlineServerClient,
            factory);
        var penaltyService = new TenantGamePenaltyEntityService(
            Repository<TenantGamePenaltyEntity>(),
            userContext,
            linkService);
        return new OnlineReportCaseService(onlineServerClient, factory, penaltyService, linkService);
    }

    private async Task SeedScopeAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.Insertable(new TenantGameAppEntity
        {
            Id = App100,
            Name = "app-100",
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAreaEntity
        {
            Id = Area1001,
            Name = "area-1001",
            Description = string.Empty,
            GameAppId = App100,
            HttpManageUrl = "https://game-server.test",
            TcpSocketUrl = "tcp://game-server.test",
            WebSocketUrl = "wss://game-server.test",
            ServerVersion = "1.0.0",
            ServerType = "default",
            LocalizationKey = "area.key",
            AllowedPlayerProfile = "{}",
            ServerEvents = "[]",
            AccessControls = "{}",
            ServerContainerId = string.Empty,
            ServerParams = "{}",
            AppVersion = "1.0.0",
            GmApiPrefix = "gm/api",
            ControlApiPrefix = "ctl",
            OnlineAdminApiPrefix = "online/admin",
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();
    }

    private SqlSugarRepository<T> Repository<T>() where T : EntityBase, new()
    {
        return new SqlSugarRepository<T> { Context = _db, };
    }

    private static CurrentUserContext CreateUserContext(long tenantId, long userId)
    {
        var identity = new ClaimsIdentity("TestAuth");
        identity.AddClaim(new Claim(ClaimConst.TenantId, tenantId.ToString()));
        identity.AddClaim(new Claim(ClaimConst.UserId, userId.ToString()));
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity), };
        var accessor = new HttpContextAccessor { HttpContext = httpContext, };
        return new CurrentUserContext(accessor);
    }

    /// <summary>
    /// 手写 fake：按 action 记录调用、可配置异常与响应载荷；默认联动命令返回成功确认、其余返回空对象。
    /// </summary>
    private sealed class FakeOnlineServerClient : IOnlineServerClient
    {
        private readonly Dictionary<string, Exception> _failures = new(StringComparer.Ordinal);
        private readonly Dictionary<string, object> _responses = new(StringComparer.Ordinal);

        public List<OnlineCall> Calls { get; } = new();

        public void FailOn(string action, Exception exception)
        {
            _failures[action] = exception;
        }

        public void Respond(string action, object payload)
        {
            _responses[action] = payload;
        }

        public Task<TResponse> PostAsync<TResponse>(string action, object request, OnlineScopeContext scope) where TResponse : class
        {
            Calls.Add(new OnlineCall(action, request, scope.ServerId));
            if (_failures.TryGetValue(action, out var failure))
            {
                throw failure;
            }

            if (_responses.TryGetValue(action, out var configured) && configured is TResponse typed)
            {
                return Task.FromResult(typed);
            }

            // 通知 / 案件回写走 PostAsync<object>：任意占位实例满足解包语义即可。
            var payload = typeof(TResponse) == typeof(object)
                ? new object()
                : new OnlineLinkCommandAckResponse { TokenRevokedConfirmed = true, };
            return Task.FromResult((TResponse)payload);
        }

        public Task<TResponse> GetAsync<TResponse>(string action, OnlineScopeContext scope) where TResponse : class
        {
            throw new NotSupportedException("举报案件服务不使用 GET 通道");
        }
    }

    /// <summary>
    /// fake 调用记录（action + 请求体 + 作用域区服）。
    /// </summary>
    private sealed record OnlineCall(string Action, object Request, long ServerId);

    public void Dispose()
    {
        _scopeOwner.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
