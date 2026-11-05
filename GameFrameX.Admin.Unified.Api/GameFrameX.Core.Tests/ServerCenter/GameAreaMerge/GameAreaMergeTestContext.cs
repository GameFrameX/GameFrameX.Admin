using System.Net;
using System.Security.Claims;
using System.Text.Json;
using GameFrameX.Application.ServerCenter.GameAreaMerge;
using GameFrameX.Application.ServerCenter.GameAreaMerge.Dto;
using GameFrameX.Application.ServerCenter.Online;
using GameFrameX.Core.Const;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;
using GameFrameX.Foundation.Orm.Entity;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.ServerCenter.GameAreaMerge;

internal sealed class GameAreaMergeTestContext : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"gameframex-merge-{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _owner;
    private readonly HttpClient _httpClient;
    public SqlSugarScopeProvider Database { get; }
    public HttpContextAccessor Accessor { get; } = new() { HttpContext = new DefaultHttpContext() };
    public MergeHandler Handler { get; } = new();
    public GameAreaMergeService Service { get; }

    public GameAreaMergeTestContext()
    {
        var configuration = new ConnectionConfig { ConfigId = "game-area-merge-tests", DbType = DbType.Sqlite, ConnectionString = $"DataSource={_databasePath}" };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(configuration);
        _owner = new SqlSugarScope(new List<ConnectionConfig> { configuration });
        Database = _owner.GetConnectionScope("game-area-merge-tests");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(Database);
        Database.CodeFirst.InitTables(typeof(TenantGameAreaMergePlanEntity), typeof(TenantGameAreaEntity), typeof(TenantGameAreaClassifyEntity));
        Database.Insertable(new TenantGameAreaClassifyEntity { Id = 10, Name = "合服测试分类", LocalizationKey = "area", DockerRegister = "", ImageName = "", DockerUserName = "", DockerPassword = "", ContainerNetName = "", ContainerSubnet = "", ContainerGateway = "", IsEnabled = true, IsDeleted = false }).ExecuteCommand();
        foreach (var identifier in new long[] { 101, 102, 103, 104 })
        {
            Database.Insertable(new TenantGameAreaEntity
            {
                Id = identifier, Name = $"合服区服{identifier}", ClassifyId = 10, IsDeleted = false,
                HttpManageUrl = "http://127.0.0.1:22390", TcpSocketUrl = "tcp://localhost", WebSocketUrl = "ws://localhost",
                ServerVersion = "1", ServerType = "normal", LocalizationKey = "area", AllowedPlayerProfile = "{}",
                ServerEvents = "[]", AccessControls = "{}", ServerContainerId = "", ServerParams = "{}", AppVersion = "1",
                GmApiPrefix = "gm/api", ControlApiPrefix = "control", OnlineAdminApiPrefix = "online/admin",
            }).ExecuteCommand();
        }
        Login(11);
        _httpClient = new HttpClient(Handler);
        var client = new GameAreaMergeClient(_httpClient, new OnlineServerEndpointResolver(), Repository<TenantGameAreaEntity>(),
            new OnlineScopeContextFactory(Repository<TenantGameAreaEntity>()));
        Service = new GameAreaMergeService(Repository<TenantGameAreaMergePlanEntity>(), Repository<TenantGameAreaEntity>(), client, new CurrentUserContext(Accessor));
    }

    public SqlSugarRepository<T> Repository<T>() where T : EntityBase, new() => new() { Context = Database };

    public void Login(long identifier)
    {
        Accessor.HttpContext!.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimConst.UserId, identifier.ToString()) }, "test"));
    }

    public Task<GameAreaMergePlanResponse> CreateAsync() => Service.AddAsync(new CreateGameAreaMergePlanRequest
    {
        TargetServerId = 101, SourceServerIds = new List<long> { 103, 102 }, Reason = "合服演练",
    });

    public async Task<GameAreaMergePlanResponse> ApproveAsync()
    {
        var plan = await CreateAsync();
        await Service.DryRunAsync(new() { Id = plan.Id });
        Login(12);
        return await Service.ApproveAsync(new() { Id = plan.Id, ReviewerOperatorId = 12, Reason = "复核无全局数据写入" });
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _owner.Dispose();
        File.Delete(_databasePath);
    }

    internal sealed class MergeHandler : HttpMessageHandler
    {
        public long GlobalRowsTouched { get; set; }
        public bool AlreadyExecuted { get; set; }
        public string? FailAction { get; set; }
        public Action? OnExecute { get; set; }
        public List<string> Actions { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var action = request.RequestUri!.Segments.Last();
            Actions.Add(action);
            if (action == FailAction)
            {
                throw new HttpRequestException("test failure");
            }
            using (var document = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken)))
            {
                var payload = document.RootElement;
                Assert.Equal(101, payload.GetProperty("ServerId").GetInt64());
                Assert.Equal(101, payload.GetProperty("TargetServerId").GetInt64());
                Assert.Equal("102,103", payload.GetProperty("SourceServerIds").GetString());
            }
            if (action == "execute_server_merge")
            {
                OnExecute?.Invoke();
                cancellationToken.ThrowIfCancellationRequested();
            }
            var report = new GameAreaMergeReportResponse
            {
                PlanId = "merge-101-102-103", TargetServerId = 101, SourceServerIds = new() { 102, 103 },
                DryRun = action == "dry_run_server_merge", AlreadyExecuted = AlreadyExecuted,
                RowKeySchemaVersion = 1, GlobalDomainRowsTouched = GlobalRowsTouched,
                Steps = new() { new() { Step = "players", AffectedCount = 20, Details = new() { ["sourceRows"] = 20 } } },
            };
            var response = new OnlineApiResponse<GameAreaMergeReportResponse> { Code = 0, MessageKey = "ok", RequestId = "merge-test", Data = report };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(HttpJsonResultData<string>.Success(JsonHelper.Serialize(response)).ToString()),
            };
        }
    }
}
