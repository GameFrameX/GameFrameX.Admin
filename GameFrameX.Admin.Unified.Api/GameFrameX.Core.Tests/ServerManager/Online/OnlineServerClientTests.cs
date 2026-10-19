using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using GameFrameX.Application.Game.ServerManager.Online;
using GameFrameX.Core.Const;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;
using GameFrameX.Foundation.Orm.Entity;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.ServerManager.Online;

// C339 / GFX-750（vault:C2 S1.7）：Online 管理 API 客户端契约自检（typed client 基座）。
// 覆盖：URL 形态（{HttpManageUrl}/{OnlineAdminApiPrefix}/{action}，默认 online/admin）、
// 作用域三元组并入 POST 请求体顶层（业务体携带的伪作用域字段被鉴权值覆盖）与 GET 查询参数、
// Foundation 信封解包（失败 / 成功缺 Data 抛 InvalidOperationException）、
// Online 协议码非 0 按协议段映射镜像段成员抛 BusinessError（协议段 Nxxx ↔ 镜像段 32(N-1)，
// 段外未知码兜底系统段——VC-1.10 不崩溃、不误判成功）、原始协议码随 Message errors 透出、
// 作用域构造后区服被删的时序漂移拒绝。
public sealed class OnlineServerClientTests : IDisposable
{
    private const long TenantA = 1001;
    private const long App100 = 100;
    private const long Area1001 = 2001;

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"online_client_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _scopeOwner;
    private readonly SqlSugarScopeProvider _db;

    public OnlineServerClientTests()
    {
        // 专属 ConfigId（C339）：SqlSugar 实体信息（含表名映射）按连接配置静态缓存且进程级共享；
        // 不设 ConfigId 会与裸配置 fixture（AdvancedQueryAreaFilterTests / TenantGameOrderShipServiceTests，
        // PascalCase 表名）互抢默认缓存键，后到者 InitTables no-op → insert 报 no such table
        //（AdvancedQueryAreaFilterTests ctor 注释记录的同类坑，ClientApi/Home 系靠租户 ConfigId 天然隔离）。
        // 专属键经 List 构造保留 ConfigId（同 ClientApiTenantDbProvider 范式），静态缓存彻底隔离。
        var config = new ConnectionConfig
        {
            ConfigId = "online-client",
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _scopeOwner = new SqlSugarScope(new List<ConnectionConfig> { config });
        _db = _scopeOwner.GetConnectionScope("online-client");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAreaEntity));
    }

    /// <summary>
    /// POST URL 形态：默认前缀 online/admin 下 <c>{root}/{prefix}/{action}</c>（kind=OnlineAdmin 第五档）。
    /// </summary>
    [Fact]
    public async Task PostAsync_BuildsUrl_UnderOnlineAdminPrefix()
    {
        await SeedAsync();

        var handler = new FakeHandler(SuccessEnvelope(new OnlineDemoData { PlayerId = "p-1", Reason = 1, }));
        var client = CreateClient(handler);

        await client.PostAsync<OnlineDemoData>("kick_player", new { Reason = 1, }, await CreateScopeAsync());

        Assert.Equal("https://game-server.test/online/admin/kick_player", handler.LastRequestUrl);
        Assert.Equal("POST", handler.LastRequestMethod);
    }

    /// <summary>
    /// 自定义前缀：区服 OnlineAdminApiPrefix 覆写时 URL 随之变化（区服级 DB 配置承接，C173/C175 架构）。
    /// </summary>
    [Fact]
    public async Task PostAsync_Respects_CustomOnlineAdminPrefix_FromArea()
    {
        await SeedAsync();

        var area = await _db.Queryable<TenantGameAreaEntity>().FirstAsync(u => u.Id == Area1001);
        area.OnlineAdminApiPrefix = "online/v2";
        await _db.Updateable(area).ExecuteCommandAsync();

        var handler = new FakeHandler(SuccessEnvelope(new OnlineDemoData { PlayerId = "p-1", }));
        var client = CreateClient(handler);

        await client.PostAsync<OnlineDemoData>("kick_player", new { Reason = 1, }, await CreateScopeAsync());

        Assert.Equal("https://game-server.test/online/v2/kick_player", handler.LastRequestUrl);
    }

    /// <summary>
    /// 作用域注入：POST 请求体顶层强制携带 TenantId/AppId/ServerId；业务体自带的伪作用域字段被鉴权值覆盖
    ///（「以鉴权与服务端路由为准，客户端字段不可直接覆盖」vault 契约）；业务字段原样保留。
    /// </summary>
    [Fact]
    public async Task PostAsync_MergesScopeIntoPayload_OverridingClientScopeFields()
    {
        await SeedAsync();

        var handler = new FakeHandler(SuccessEnvelope(new OnlineDemoData { PlayerId = "p-1", }));
        var client = CreateClient(handler);

        await client.PostAsync<OnlineDemoData>("kick_player", new { TenantId = 999L, AppId = 888L, ServerId = 777L, Reason = 3, }, await CreateScopeAsync());

        var body = handler.LastRequestBodyJson;
        Assert.Equal(TenantA, Convert.ToInt64(body["TenantId"]?.ToString()));
        Assert.Equal(App100, Convert.ToInt64(body["AppId"]?.ToString()));
        Assert.Equal(Area1001, Convert.ToInt64(body["ServerId"]?.ToString()));
        Assert.Equal(3, Convert.ToInt32(body["Reason"]?.ToString()));
    }

    /// <summary>
    /// 成功路径：Code=0 → Foundation 信封解包 → 返回 Data 载荷（反序列化为业务类型）。
    /// </summary>
    [Fact]
    public async Task PostAsync_UnwrapsEnvelope_ToDataPayload()
    {
        await SeedAsync();

        var handler = new FakeHandler(SuccessEnvelope(new OnlineDemoData { PlayerId = "p-9527", Reason = 2, }));
        var client = CreateClient(handler);

        var data = await client.PostAsync<OnlineDemoData>("kick_player", new { Reason = 2, }, await CreateScopeAsync());

        Assert.Equal("p-9527", data.PlayerId);
        Assert.Equal(2, data.Reason);
    }

    /// <summary>
    /// GET：作用域三元组作为查询参数附加（POST 并体 / GET 并 query 的双形态契约）。
    /// </summary>
    [Fact]
    public async Task GetAsync_AppendsScopeQueryParameters()
    {
        await SeedAsync();

        var handler = new FakeHandler(SuccessEnvelope(new OnlineDemoData { PlayerId = "p-1", }));
        var client = CreateClient(handler);

        await client.GetAsync<OnlineDemoData>("query_state", await CreateScopeAsync());

        Assert.Equal($"https://game-server.test/online/admin/query_state?tenantId={TenantA}&appId={App100}&serverId={Area1001}", handler.LastRequestUrl);
        Assert.Equal("GET", handler.LastRequestMethod);
    }

    /// <summary>
    /// 协议段映射（vault 八段）：Online 协议码非 0 按协议段 Nxxx → 镜像段 32(N-1) 代表成员抛 BusinessError；
    /// 段外未知码兜底系统段 InternalError（VC-1.10：不崩溃、不误判成功）。
    /// </summary>
    [Theory]
    [InlineData(1001, 32000001)] // 1xxx 系统与网络 → 段 320 InternalError
    [InlineData(2004, 32100003)] // 2xxx 鉴权与 Session → 段 321 SessionInvalid
    [InlineData(3005, 32200001)] // 3xxx 作用域 → 段 322 ScopeDenied
    [InlineData(4040, 32300001)] // 4xxx 参数与资源 → 段 323 ParameterInvalid
    [InlineData(5002, 32400003)] // 5xxx 业务状态 → 段 324 StateOperationForbidden
    [InlineData(6001, 32500001)] // 6xxx 幂等与冲突 → 段 325 DuplicateRequest
    [InlineData(7003, 32600001)] // 7xxx 限流风控 → 段 326 RateLimitExceeded
    [InlineData(8002, 32700001)] // 8xxx 临时失败 → 段 327 ServiceBusy
    [InlineData(9999, 32000001)] // 协议段外未知码 → 兜底系统段 InternalError
    public async Task PostAsync_MapsProtocolCode_ToMirrorSegmentMember(int onlineCode, int expectedMirrorCode)
    {
        await SeedAsync();

        var handler = new FakeHandler(FailureEnvelope(onlineCode, "online.error.key", "req-0001"));
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<BusinessException>(async () =>
                                                                        await client.PostAsync<OnlineDemoData>("kick_player", new { Reason = 1, }, await CreateScopeAsync()));

        Assert.Equal(expectedMirrorCode, exception.Code);
    }

    /// <summary>
    /// 原始协议码透出：BusinessError Message 的 errors JSON 携带 onlineCode / messageKey / requestId（前端按数字码展示与排查）。
    /// </summary>
    [Fact]
    public async Task PostAsync_NonZeroCode_CarriesOriginalProtocolCodeInErrors()
    {
        await SeedAsync();

        var handler = new FakeHandler(FailureEnvelope(3005, "online.scope.denied", "req-abc-123"));
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<BusinessException>(async () =>
                                                                        await client.PostAsync<OnlineDemoData>("kick_player", new { Reason = 1, }, await CreateScopeAsync()));

        Assert.Contains("3005", exception.Message);
        Assert.Contains("online.scope.denied", exception.Message);
        Assert.Contains("req-abc-123", exception.Message);
    }

    /// <summary>
    /// Foundation 传输信封失败（非 Online 业务码路径）：抛 InvalidOperationException（基础设施错误，非业务错误码）。
    /// </summary>
    [Fact]
    public async Task PostAsync_WhenEnvelopeFails_ThrowsInvalidOperationException()
    {
        await SeedAsync();

        var handler = new FakeHandler(HttpJsonResultData<string>.Fail("server rejected").ToString());
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                                                               await client.PostAsync<OnlineDemoData>("kick_player", new { Reason = 1, }, await CreateScopeAsync()));
    }

    /// <summary>
    /// Code=0 但 Data 缺失：成功响应缺载荷按契约非法拒绝（不返回 null 数据伪装成功）。
    /// </summary>
    [Fact]
    public async Task PostAsync_WhenSuccessMissingData_ThrowsInvalidOperationException()
    {
        await SeedAsync();

        var handler = new FakeHandler(SuccessEnvelope(data: null));
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                                                               await client.PostAsync<OnlineDemoData>("kick_player", new { Reason = 1, }, await CreateScopeAsync()));
    }

    /// <summary>
    /// 时序漂移：作用域构造成功后区服被删（或跨库不可见），调用期按 ServerScopeDenied 拒绝，不落到裸构造 URL。
    /// </summary>
    [Fact]
    public async Task PostAsync_WhenAreaDeletedAfterScopeCreated_ThrowsServerScopeDenied()
    {
        await SeedAsync();

        var handler = new FakeHandler(SuccessEnvelope(new OnlineDemoData { PlayerId = "p-1", }));
        var client = CreateClient(handler);
        var scope = await CreateScopeAsync();

        await _db.Deleteable<TenantGameAreaEntity>().Where(u => u.Id == Area1001).ExecuteCommandAsync();

        var exception = await Assert.ThrowsAsync<BusinessException>(async () =>
                                                                        await client.PostAsync<OnlineDemoData>("kick_player", new { Reason = 1, }, scope));
        Assert.Equal(32200004, exception.Code);
    }

    /// <summary>
    /// 测试业务载荷（嵌套类型遵守 one-type-per-file 例外）。
    /// </summary>
    private sealed class OnlineDemoData
    {
        public string PlayerId { get; set; } = string.Empty;

        public int Reason { get; set; }
    }

    private OnlineServerClient CreateClient(FakeHandler handler)
    {
        return new OnlineServerClient(new HttpClient(handler), new OnlineServerEndpointResolver(), Repository<TenantGameAreaEntity>());
    }

    private async Task<OnlineScopeContext> CreateScopeAsync()
    {
        var factory = new OnlineScopeContextFactory(CreateUserContext(TenantA), Repository<TenantGameAppEntity>(), Repository<TenantGameAreaEntity>());
        return await factory.CreateAsync(App100, Area1001, "unit-test");
    }

    private static CurrentUserContext CreateUserContext(long tenantId)
    {
        var identity = new ClaimsIdentity("TestAuth");
        identity.AddClaim(new Claim(ClaimConst.TenantId, tenantId.ToString()));
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity), };
        var accessor = new HttpContextAccessor { HttpContext = httpContext, };
        return new CurrentUserContext(accessor);
    }

    private SqlSugarRepository<T> Repository<T>() where T : EntityBase, new()
    {
        return new SqlSugarRepository<T> { Context = _db, };
    }

    /// <summary>
    /// Code=0 成功业务响应包 Foundation 传输信封（信封 data = OnlineApiResponse JSON 字符串，双层信封契约）。
    /// </summary>
    private static string SuccessEnvelope(OnlineDemoData? data)
    {
        var response = new OnlineApiResponse<OnlineDemoData>
        {
            Code = 0,
            MessageKey = "ok",
            Message = "ok",
            RequestId = "req-0000",
            ServerTime = 1780000000,
            Data = data!,
        };
        return HttpJsonResultData<string>.Success(JsonHelper.Serialize(response)).ToString();
    }

    /// <summary>
    /// 非 0 协议码业务响应包 Foundation 传输信封。
    /// </summary>
    private static string FailureEnvelope(int onlineCode, string messageKey, string requestId)
    {
        var response = new OnlineApiResponse<OnlineDemoData>
        {
            Code = onlineCode,
            MessageKey = messageKey,
            Message = "rejected",
            RequestId = requestId,
            ServerTime = 1780000000,
        };
        return HttpJsonResultData<string>.Success(JsonHelper.Serialize(response)).ToString();
    }

    private async Task SeedAsync()
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

    /// <summary>
    /// 伪造 HttpMessageHandler：捕获请求 URL/方法/体，返回预设 JSON 信封（同 MailCampaignServerClientTests 范式）。
    /// </summary>
    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly string _responsePayload;

        public string LastRequestUrl { get; private set; } = string.Empty;

        public string LastRequestMethod { get; private set; } = string.Empty;

        public Dictionary<string, object?> LastRequestBodyJson { get; private set; } = new();

        public FakeHandler(string responsePayload)
        {
            _responsePayload = responsePayload;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUrl = request.RequestUri?.AbsoluteUri ?? string.Empty;
            LastRequestMethod = request.Method.Method;

            var body = string.Empty;
            if (request.Content != null)
            {
                body = request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            }

            LastRequestBodyJson = string.IsNullOrWhiteSpace(body)
                                      ? new Dictionary<string, object?>()
                                      : JsonSerializer.Deserialize<Dictionary<string, object?>>(body) ?? new Dictionary<string, object?>();

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responsePayload),
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
