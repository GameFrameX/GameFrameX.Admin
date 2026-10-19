using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using GameFrameX.Application.Game.Online.Social;
using GameFrameX.Application.Game.Online.Social.Dto;
using GameFrameX.Application.Game.ServerManager.Online;
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

// C350 / GFX-754（vault:C7、vault:C2 S1.4/S1.5）：处罚联动命令服务契约自检。
// 覆盖：命令 action 三值映射（Kick→kick_player / Ban→revoke_token / Mute→mute_player）、
// IdempotencyKey 稳定形态（penalty-{id}-{command}-{serverId}）、全区服处罚逐区服聚合、
// 单区服失败（Online 业务码 / 传输层）不抛不回滚事实源、非 Active / 纯落账动作 / 全区服无目标拒绝。
public sealed class OnlinePenaltyLinkCommandServiceTests : IDisposable
{
    private const long TenantA = 3001;
    private const long App100 = 100;
    private const long Area1001 = 2001;
    private const long Area1002 = 2002;

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"penalty_link_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlinePenaltyLinkCommandServiceTests()
    {
        // 专属 ConfigId：防静态实体信息缓存与其它 fixture 互抢（同 OnlineServerClientTests ctor 注释）。
        var config = new ConnectionConfig
        {
            ConfigId = "penalty-link",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("penalty-link");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAreaEntity),
            typeof(TenantGamePenaltyEntity));
    }

    /// <summary>
    /// Mute 命令成功路径：action 映射 mute_player，请求体带处罚 Id / 目标玩家 / 稳定 IdempotencyKey，响应聚合全成功。
    /// </summary>
    [Fact]
    public async Task ExecuteLinkAsync_MuteCommand_SendsMutePlayerWithStableIdempotencyKey()
    {
        await SeedScopeAsync();
        var penaltyId = await SeedPenaltyAsync(actionType: PenaltyActionTypeEnum.Mute, areaId: Area1001, targetId: "player-1");
        var fake = new FakeOnlineServerClient();
        var service = CreateService(fake);

        var response = await service.ExecuteLinkAsync(penaltyId, OnlinePenaltyCommandKind.Mute);

        var call = Assert.Single(fake.Calls);
        Assert.Equal("mute_player", call.Action);
        var body = Assert.IsType<OnlineLinkCommandRequest>(call.Request);
        Assert.Equal(penaltyId, body.PenaltyId);
        Assert.Equal("player-1", body.PlayerId);
        Assert.Equal($"penalty-{penaltyId}-mute-{Area1001}", body.IdempotencyKey);

        Assert.True(response.AllSucceeded);
        var result = Assert.Single(response.Results);
        Assert.True(result.Success);
        Assert.True(result.TokenRevokedConfirmed);
    }

    /// <summary>
    /// Kick 命令映射 kick_player（三命令白名单各值锚定；Ban→revoke_token 同构，经处置流与页面同步路径间接覆盖）。
    /// </summary>
    [Fact]
    public async Task ExecuteLinkAsync_KickCommand_MapsToKickPlayerAction()
    {
        await SeedScopeAsync();
        var penaltyId = await SeedPenaltyAsync(actionType: PenaltyActionTypeEnum.Ban, areaId: Area1001, targetId: "player-1");
        var fake = new FakeOnlineServerClient();
        var service = CreateService(fake);

        await service.ExecuteLinkAsync(penaltyId, OnlinePenaltyCommandKind.Kick);

        Assert.Equal("kick_player", Assert.Single(fake.Calls).Action);
    }

    /// <summary>
    /// 全区服处罚（AreaId=null）：App 下每个区服逐一执行并各落一条结果。
    /// </summary>
    [Fact]
    public async Task ExecuteLinkAsync_GlobalPenalty_ExecutesPerAppServer()
    {
        await SeedScopeAsync();
        var penaltyId = await SeedPenaltyAsync(actionType: PenaltyActionTypeEnum.Mute, areaId: null, targetId: "player-1");
        var fake = new FakeOnlineServerClient();
        var service = CreateService(fake);

        var response = await service.ExecuteLinkAsync(penaltyId, OnlinePenaltyCommandKind.Mute);

        Assert.Equal(2, fake.Calls.Count);
        Assert.Equal(new[] { Area1001, Area1002 }, fake.Calls.Select(u => u.ServerId).Order().ToArray());
        Assert.Equal(2, response.Results.Count);
        Assert.True(response.AllSucceeded);
    }

    /// <summary>
    /// 单区服 Online 业务码失败：降级为该区服结果行（OnlineCode 透出镜像码），不抛异常、不回滚事实源（联动可重试）。
    /// </summary>
    [Fact]
    public async Task ExecuteLinkAsync_ServerBusinessFailure_ReportedPerServerWithoutThrowing()
    {
        await SeedScopeAsync();
        var penaltyId = await SeedPenaltyAsync(actionType: PenaltyActionTypeEnum.Mute, areaId: Area1001, targetId: "player-1");
        var fake = new FakeOnlineServerClient();
        fake.FailOn("mute_player", BusinessError.Oh(OnlineErrorCode.StateOperationForbidden));
        var service = CreateService(fake);

        var response = await service.ExecuteLinkAsync(penaltyId, OnlinePenaltyCommandKind.Mute);

        Assert.False(response.AllSucceeded);
        var result = Assert.Single(response.Results);
        Assert.False(result.Success);
        Assert.Equal((int)OnlineErrorCode.StateOperationForbidden, result.OnlineCode);
        Assert.False(result.TokenRevokedConfirmed);

        // 事实源保持 Active 原状（不因联动失败被改动）——重试入口不变。
        var penalty = await _db.Queryable<TenantGamePenaltyEntity>().FirstAsync(u => u.Id == penaltyId);
        Assert.Equal(PenaltyStatusEnum.Active, penalty.Status);
    }

    /// <summary>
    /// 传输层失败（HTTP 不可达 / 信封解包失败）：同为可重试结果行（OnlineCode 为 null）。
    /// </summary>
    [Fact]
    public async Task ExecuteLinkAsync_TransportFailure_ReportedAsRetryableResult()
    {
        await SeedScopeAsync();
        var penaltyId = await SeedPenaltyAsync(actionType: PenaltyActionTypeEnum.Mute, areaId: Area1001, targetId: "player-1");
        var fake = new FakeOnlineServerClient();
        fake.FailOn("mute_player", new HttpRequestException("connection refused"));
        var service = CreateService(fake);

        var response = await service.ExecuteLinkAsync(penaltyId, OnlinePenaltyCommandKind.Mute);

        Assert.False(response.AllSucceeded);
        var result = Assert.Single(response.Results);
        Assert.False(result.Success);
        Assert.Null(result.OnlineCode);
        Assert.Contains("connection refused", result.FailureMessage);
    }

    /// <summary>
    /// 非 Active 处罚拒绝联动（31700001 PenaltyNotActive）：已撤销的处罚无运行时同步语义。
    /// </summary>
    [Fact]
    public async Task ExecuteLinkAsync_RejectsPenaltyNotActive()
    {
        await SeedScopeAsync();
        var penaltyId = await SeedPenaltyAsync(actionType: PenaltyActionTypeEnum.Mute, areaId: Area1001, targetId: "player-1", status: PenaltyStatusEnum.Revoked);
        var service = CreateService(new FakeOnlineServerClient());

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteLinkAsync(penaltyId, OnlinePenaltyCommandKind.Mute));

        Assert.Equal(31700001, exception.Code);
    }

    /// <summary>
    /// 纯落账动作（限制交易 / 限制匹配）拒绝联动（31700002 PenaltyActionNotLinkable）。
    /// </summary>
    [Fact]
    public async Task ExecuteLinkAsync_RejectsNonLinkableAction()
    {
        await SeedScopeAsync();
        var penaltyId = await SeedPenaltyAsync(actionType: PenaltyActionTypeEnum.RestrictTrade, areaId: Area1001, targetId: "player-1");
        var service = CreateService(new FakeOnlineServerClient());

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteLinkAsync(penaltyId, OnlinePenaltyCommandKind.Mute));

        Assert.Equal(31700002, exception.Code);
    }

    /// <summary>
    /// 处罚行不存在：RecordNotFound（不区分跨租户与不存在，作用域语义统一）。
    /// </summary>
    [Fact]
    public async Task ExecuteLinkAsync_RejectsMissingPenalty()
    {
        await SeedScopeAsync();
        var service = CreateService(new FakeOnlineServerClient());

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteLinkAsync(9999, OnlinePenaltyCommandKind.Mute));

        Assert.Equal(CommonErrorCode.RecordNotFound, Assert.IsType<CommonErrorCode>(exception.ErrorCode));
    }

    /// <summary>
    /// 全区服处罚但 App 下无任何区服（31700003 PenaltyLinkTargetMissing）。
    /// </summary>
    [Fact]
    public async Task ExecuteLinkAsync_RejectsGlobalPenaltyWithoutTargetServers()
    {
        await SeedScopeAsync(appServers: false);
        var penaltyId = await SeedPenaltyAsync(actionType: PenaltyActionTypeEnum.Mute, areaId: null, targetId: "player-1");
        var service = CreateService(new FakeOnlineServerClient());

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteLinkAsync(penaltyId, OnlinePenaltyCommandKind.Mute));

        Assert.Equal(31700003, exception.Code);
    }

    private OnlinePenaltyLinkCommandService CreateService(IOnlineServerClient onlineServerClient)
    {
        var userContext = CreateUserContext(TenantA);
        var factory = new OnlineScopeContextFactory(userContext, Repository<TenantGameAppEntity>(), Repository<TenantGameAreaEntity>());
        return new OnlinePenaltyLinkCommandService(
            Repository<TenantGamePenaltyEntity>(),
            Repository<TenantGameAreaEntity>(),
            onlineServerClient,
            factory);
    }

    private async Task SeedScopeAsync(bool appServers = true)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.Insertable(new TenantGameAppEntity
        {
            Id = App100,
            Name = "app-100",
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        if (appServers)
        {
            await _db.Insertable(NewArea(Area1001, now)).ExecuteCommandAsync();
            await _db.Insertable(NewArea(Area1002, now)).ExecuteCommandAsync();
        }
    }

    private async Task<long> SeedPenaltyAsync(PenaltyActionTypeEnum actionType, long? areaId, string targetId, PenaltyStatusEnum status = PenaltyStatusEnum.Active)
    {
        var penalty = new TenantGamePenaltyEntity
        {
            AppId = App100,
            AreaId = areaId,
            TargetType = PenaltyTargetTypeEnum.Account,
            TargetId = targetId,
            ActionType = actionType,
            Status = status,
            Reason = "unit-test",
            OperatorId = 1,
            ExpireTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600,
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            IsDeleted = false,
        };
        await _db.Insertable(penalty).ExecuteCommandAsync();
        return penalty.Id;
    }

    private static TenantGameAreaEntity NewArea(long id, long now)
    {
        return new TenantGameAreaEntity
        {
            Id = id,
            Name = $"area-{id}",
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
        };
    }

    private SqlSugarRepository<T> Repository<T>() where T : EntityBase, new()
    {
        return new SqlSugarRepository<T> { Context = _db, };
    }

    private static CurrentUserContext CreateUserContext(long tenantId)
    {
        var identity = new ClaimsIdentity("TestAuth");
        identity.AddClaim(new Claim(ClaimConst.TenantId, tenantId.ToString()));
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity), };
        var accessor = new HttpContextAccessor { HttpContext = httpContext, };
        return new CurrentUserContext(accessor);
    }

    /// <summary>
    /// 手写 fake：按 action 记录调用（含作用域 ServerId），可配置异常；默认返回联动命令成功确认。
    /// </summary>
    private sealed class FakeOnlineServerClient : IOnlineServerClient
    {
        private readonly Dictionary<string, Exception> _failures = new(StringComparer.Ordinal);

        public List<OnlineCall> Calls { get; } = new();

        public void FailOn(string action, Exception exception)
        {
            _failures[action] = exception;
        }

        public Task<TResponse> PostAsync<TResponse>(string action, object request, OnlineScopeContext scope) where TResponse : class
        {
            Calls.Add(new OnlineCall(action, request, scope.ServerId));
            if (_failures.TryGetValue(action, out var failure))
            {
                throw failure;
            }

            var payload = new OnlineLinkCommandAckResponse { TokenRevokedConfirmed = true, };
            return Task.FromResult((TResponse)(object)payload);
        }

        public Task<TResponse> GetAsync<TResponse>(string action, OnlineScopeContext scope) where TResponse : class
        {
            throw new NotSupportedException("联动命令服务不使用 GET 通道");
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
