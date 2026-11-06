using GameFrameX.Core.Const.MenuSeed;
using System.Net;
using System.Text.Json;
using GameFrameX.Application.SeedData.System;
using GameFrameX.Application.ServerCenter.GameArea;
using GameFrameX.Application.ServerCenter.GameAreaProvision;
using GameFrameX.Application.ServerCenter.GameAreaProvision.Dto;
using GameFrameX.Application.ServerCenter.Online;
using GameFrameX.Client.Api.GameArea;
using GameFrameX.Client.Api.GameArea.Dto;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Orm.Entity;
using SqlSugar;

namespace GameFrameX.Core.Tests.ServerCenter;

public sealed class GameAreaProvisionServiceTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"gamearea-provision-{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _owner;
    private readonly SqlSugarScopeProvider _database;
    private readonly ProvisioningHandler _handler = new();
    private readonly HttpClient _httpClient;
    private readonly GameAreaProvisionService _service;

    public GameAreaProvisionServiceTests()
    {
        var configuration = new ConnectionConfig
        {
            ConfigId = "gamearea-provision", DbType = DbType.Sqlite, ConnectionString = $"DataSource={_databasePath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(configuration);
        _owner = new SqlSugarScope(new List<ConnectionConfig> { configuration });
        _database = _owner.GetConnectionScope("gamearea-provision");
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_database);
        SqlSugarUnifiedConfig.ConfigureClientFilters(_database);
        _database.CodeFirst.InitTables<GameAreaEntity, GameAreaClassifyEntity>();
        _httpClient = new HttpClient(_handler);
        var repository = Repository<GameAreaEntity>();
        _service = new GameAreaProvisionService(repository,
            new GameAreaService(repository, Repository<GameAreaClassifyEntity>(), _httpClient),
            new OnlineScopeContextFactory(repository),
            new OnlineServerClient(_httpClient, new OnlineServerEndpointResolver(), repository));
        Seed();
    }

    [Fact]
    public async Task Provision_Success_ClonesConfigurationAndEnablesOnlyAfterRegistration()
    {
        var preview = await _service.PreviewAsync(new BaseIdRequest { Id = 100 });
        Assert.Equal("1.2.3", preview.ClonedFields["ServerVersion"]);
        Assert.DoesNotContain("ServerContainerId", preview.ClonedFields.Keys);
        Assert.Contains("ServerParameters", preview.RequiredFields);
        Assert.Contains("Shared", preview.ClonedFields["ServerParameters"]);
        var created = await _service.CreateAsync(Request());
        Assert.False(created.IsEnabled);
        Assert.False(created.ContainerCreated);
        var result = await _service.ExecuteAsync(new BaseIdRequest { Id = 200 });
        Assert.True(result.IsEnabled && result.Registered && result.ContainerCreated);
        Assert.Equal(200, _handler.LastServerId);
        Assert.Equal("200", _handler.LastContainerName);
        var area = await Repository<GameAreaEntity>().GetByIdAsync(200);
        Assert.Equal(1000, area.Capacity);
        Assert.Equal(GameServerStatus.Online, area.ServerStatus);
        Assert.Equal(_handler.OpenedAt / 1000, area.CreationTime);
        Assert.False(area.IsDefault || area.IsRecommend || area.EnableClientGmApi);
        Assert.Equal("[]", area.ServerEvents);
        Assert.Equal(0, area.CurrentLoad);
    }

    [Fact]
    public async Task Provision_EnvironmentMapping_PreservesTemplateAndAppliesOperatorOverrides()
    {
        await _service.CreateAsync(Request());
        await _service.ExecuteAsync(new BaseIdRequest { Id = 200 });
        var area = await Repository<GameAreaEntity>().GetByIdAsync(200);
        var parameters = JsonSerializer.Deserialize<Dictionary<string, string>>(area.ServerParams)!;
        Assert.Equal("template", parameters["Shared"]);
        Assert.Equal("new", parameters["Override"]);
        Assert.Equal(parameters, _handler.LastEnvironment);
    }

    [Fact]
    public async Task Provision_ContainerFailure_IsExplicitAndRetryDoesNotDuplicateRow()
    {
        await _service.CreateAsync(Request());
        _handler.ContainerCode = 17;
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _service.ExecuteAsync(new BaseIdRequest { Id = 200 }));
        Assert.Equal(60600004, exception.Code);
        Assert.Contains("manager unavailable", exception.Message);
        Assert.False((await _service.StatusAsync(new BaseIdRequest { Id = 200 })).IsEnabled);
        Assert.Equal(0, _handler.RegistrationCount);
        _handler.ContainerCode = 0;
        await _service.CreateAsync(Request());
        Assert.True((await _service.ExecuteAsync(new BaseIdRequest { Id = 200 })).IsEnabled);
        Assert.Equal(2, await _database.Queryable<GameAreaEntity>().Filter(null, true).CountAsync());
    }

    [Fact]
    public async Task Provision_RegistrationFailure_RetryKeepsContainerAndEnablesLater()
    {
        await _service.CreateAsync(Request());
        _handler.RegistrationFailures = 1;
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _service.ExecuteAsync(new BaseIdRequest { Id = 200 }));
        Assert.Equal(60600005, exception.Code);
        var failed = await _service.StatusAsync(new BaseIdRequest { Id = 200 });
        Assert.True(failed.ContainerCreated);
        Assert.False(failed.Registered || failed.IsEnabled);
        Assert.True((await _service.ExecuteAsync(new BaseIdRequest { Id = 200 })).IsEnabled);
        Assert.Equal(1, _handler.ContainerCount);
        Assert.Equal(2, _handler.RegistrationCount);
    }

    [Fact]
    public async Task Provision_ResponseLostAfterRegistration_RecoversByCalendar()
    {
        await _service.CreateAsync(Request());
        _handler.LoseRegistrationResponse = true;
        await Assert.ThrowsAsync<BusinessException>(() => _service.ExecuteAsync(new BaseIdRequest { Id = 200 }));
        Assert.True((await _service.ExecuteAsync(new BaseIdRequest { Id = 200 })).IsEnabled);
        Assert.Equal(1, _handler.RegistrationCount);
        Assert.Equal(1, _handler.ContainerCount);
    }

    [Fact]
    public async Task Provision_RepeatedCreateAndExecute_AreIdempotent()
    {
        await _service.CreateAsync(Request());
        await _service.CreateAsync(Request());
        var first = await _service.ExecuteAsync(new BaseIdRequest { Id = 200 });
        var second = await _service.ExecuteAsync(new BaseIdRequest { Id = 200 });
        Assert.Equal(first.OpenedAtUnixMilliseconds, second.OpenedAtUnixMilliseconds);
        Assert.Equal(1, _handler.ContainerCount);
        Assert.Equal(1, _handler.RegistrationCount);
        var different = Request();
        different.Name = "different";
        Assert.Equal(60600003, (await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(different))).Code);
    }

    [Fact]
    public async Task Provision_EnabledServerAppearsInRealClientAreaList()
    {
        await _service.CreateAsync(Request());
        var controller = new GameAreaController(Repository<GameAreaEntity>(), FakeOptionsProvider.WithAreaPolicy(false));
        var before = await controller.GetAreaList(new GameAreaGetAreaListRequest());
        Assert.DoesNotContain(before.Data.SelectMany(category => category.GameAreaList), area => area.Id == 200);
        await _service.ExecuteAsync(new BaseIdRequest { Id = 200 });
        var after = await controller.GetAreaList(new GameAreaGetAreaListRequest());
        Assert.Contains(after.Data.SelectMany(category => category.GameAreaList), area => area.Id == 200 && area.Name == "new server");
    }

    [Fact]
    public async Task Provision_ExternallyDisabledCompletedServer_CannotBeReenabled()
    {
        await _service.CreateAsync(Request());
        await _service.ExecuteAsync(new BaseIdRequest { Id = 200 });
        var area = await Repository<GameAreaEntity>().GetByIdAsync(200);
        area.IsEnabled = false;
        await _database.Updateable(area).AS().ExecuteCommandAsync();
        Assert.Equal(60600009, (await Assert.ThrowsAsync<BusinessException>(() => _service.ExecuteAsync(new BaseIdRequest { Id = 200 }))).Code);
    }

    [Fact]
    public async Task Provision_InvalidInputAndDeletedRows_AreRejected()
    {
        var request = Request();
        request.ServerParameters = "[]";
        Assert.Equal(60600001, (await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(request))).Code);
        request = Request();
        request.ServerId = 100;
        await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(request));
        await _service.CreateAsync(Request());
        var area = await _database.Queryable<GameAreaEntity>().Filter(null, true).FirstAsync(area => area.Id == 200);
        area.IsDeleted = true;
        await _database.Updateable(area).AS().ExecuteCommandAsync();
        Assert.Equal(60600003, (await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(Request()))).Code);
        Assert.Equal(60600008, (await Assert.ThrowsAsync<BusinessException>(() => _service.StatusAsync(new BaseIdRequest { Id = 200 }))).Code);
    }

    [Fact]
    public void Provision_MenuBindsEveryEndpointUnderServerCenter()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var parent = Assert.Single(menus, menu => menu.Key == "route:gameAreaProvision");
        Assert.Equal(StableSeedId.Menu("route:serverCenter"), parent.Pid);
        Assert.Equal(110, parent.OrderNo);
        Assert.False(parent.SuperAdminOnly);
        foreach (var action in new[] { "templates", "preview", "create", "execute", "status" })
        {
            var button = Assert.Single(menus, menu => menu.Key == "gameAreaProvision:" + action);
            Assert.Equal(parent.Id, button.Pid);
            Assert.Equal("gameareaprovision:" + action, button.Permission);
        }
    }

    private SqlSugarRepository<T> Repository<T>() where T : EntityBase, new()
    {
        return new SqlSugarRepository<T> { Context = _database };
    }

    private static GameAreaProvisionCreateRequest Request()
    {
        return new GameAreaProvisionCreateRequest
        {
            TemplateId = 100, ServerId = 200, Name = "new server", HttpManageUrl = "http://provision.test",
            TcpSocketUrl = "tcp://provision.test:9001", WebSocketUrl = "ws://provision.test:9002", ServerParameters = "{\"Override\":\"new\"}",
        };
    }

    private void Seed()
    {
        _database.Insertable(new GameAreaClassifyEntity
        {
            Id = 10, Name = "classify", LocalizationKey = "classify", HttpManagerUrl = "http://provision.test/",
            DockerRegister = "registry", ImageName = "gameframex-server", DockerUserName = "", DockerPassword = "",
            ContainerNetName = "gameframex", ContainerSubnet = "", ContainerGateway = "", IsEnabled = true, IsDeleted = false,
        }).ExecuteCommand();
        _database.Insertable(new GameAreaEntity
        {
            Id = 100, ClassifyId = 10, Name = "template", Description = "template", HttpManageUrl = "http://template.test",
            TcpSocketUrl = "tcp://template.test", WebSocketUrl = "ws://template.test", Capacity = 1000,
            ServerVersion = "1.2.3", ServerType = "normal", LocalizationKey = "server", AllowedPlayerProfile = "{}",
            AccessControls = "{}", ServerContainerId = "template-container", ServerEvents = "[1]", ServerParams = "{\"Shared\":\"template\",\"Override\":\"old\"}",
            AppVersion = "1.0.0", GmApiPrefix = "gm", ControlApiPrefix = "control", IsEnabled = true, IsDeleted = false,
            IsDefault = true, IsRecommend = true, EnableClientGmApi = true, RowVersion = 1,
        }).ExecuteCommand();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _owner.Dispose();
        File.Delete(_databasePath);
    }

    private sealed class ProvisioningHandler : HttpMessageHandler
    {
        public Dictionary<string, string> LastEnvironment { get; private set; } = new();
        public int ContainerCode { get; set; }
        public int RegistrationFailures { get; set; }
        public int ContainerCount { get; private set; }
        public int RegistrationCount { get; private set; }
        public bool LoseRegistrationResponse { get; set; }
        public bool Registered { get; private set; }
        public long OpenedAt { get; } = 1793808000123;
        public long LastServerId { get; private set; }
        public string LastContainerName { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            using (var document = JsonDocument.Parse(body))
            {
                var root = document.RootElement;
                var path = request.RequestUri!.AbsolutePath;
                if (path == "/api/Containers/CreateAndStart")
                {
                    ContainerCount++;
                    LastContainerName = root.GetProperty("ContainerName").GetString()!;
                    LastEnvironment = root.GetProperty("Environments").Deserialize<Dictionary<string, string>>()!;
                    return Reply(JsonSerializer.Serialize(new { Code = ContainerCode, Message = "manager unavailable", Data = "container-200" }));
                }

                LastServerId = root.GetProperty("ServerId").GetInt64();
                if (path.EndsWith("register_server_open", StringComparison.Ordinal))
                {
                    RegistrationCount++;
                    if (RegistrationFailures-- > 0)
                    {
                        return OnlineReply(new { }, 8001);
                    }

                    Registered = true;
                    if (LoseRegistrationResponse)
                    {
                        LoseRegistrationResponse = false;
                        throw new HttpRequestException("registration response lost");
                    }

                    return OnlineReply(new { ServerId = LastServerId, OpenedAtUnixMilliseconds = OpenedAt, FirstRegistration = true });
                }

                Assert.EndsWith("query_server_calendar", path);
                return OnlineReply(new { ServerId = LastServerId, Registered, OpenedAtUnixMilliseconds = Registered ? OpenedAt : 0, MergedIntoServerId = 0, OpenServerDay = 1 });
            }
        }

        private static HttpResponseMessage OnlineReply(object data, int code = 0)
        {
            return Reply(HttpJsonResultData<string>.Success(JsonSerializer.Serialize(new { Code = code, MessageKey = "test", RequestId = "provision-test", Data = data })).ToString());
        }

        private static HttpResponseMessage Reply(string content)
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
        }
    }
}
