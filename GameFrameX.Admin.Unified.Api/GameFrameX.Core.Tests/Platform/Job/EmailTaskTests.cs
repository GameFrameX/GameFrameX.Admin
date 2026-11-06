using System.Net;
using System.Text;
using GameFrameX.Application.NoticeCenter.GameMail.Dto;
using GameFrameX.Application.Platform.Job;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace GameFrameX.Core.Tests.Platform.Job;

/// <summary>
/// C404 §9.8：EmailTask 正向发送路径——草稿邮件经 Fulfillment 通道投递成功后状态变 Sent、
/// MailId 缓存键清除；非草稿邮件直接返回（不发起 HTTP）。
/// </summary>
public sealed class EmailTaskTests : IDisposable
{
    private readonly ClientApiTestDb _database = new();
    private readonly TestRecordingCache _cache = new();
    private readonly ServiceProvider _provider;
    private readonly CapturingHttpMessageHandler _httpHandler = new();

    public EmailTaskTests()
    {
        _database.InitTenantTables(typeof(TenantGameMailEntity), typeof(TenantGameAreaEntity), typeof(TenantGameRoleEntity));
        SeedArea(91);

        var services = new ServiceCollection();
        services.AddSingleton<ISqlSugarClient>(_database.MainDb);
        services.AddScoped(typeof(SqlSugarRepository<>));
        services.AddSingleton<GameFrameX.Core.Extension.Cache.ICache>(_cache);
        services.AddSingleton<IOptions<CacheOptions>>(Options.Create(new CacheOptions()));
        services.AddSingleton<SystemCacheService>();
        _provider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _database.Dispose();
    }

    [Fact]
    public async Task DraftMail_SendsViaFulfillment_AndBecomesSent()
    {
        InsertMail(601, GameMailStatus.Draft, isAllPost: false, playerIdListJson: "[]");
        var task = BuildTask();
        _cache.Set("MailId", 601L);

        var innerTask = await task.ExecuteAsync(CancellationToken.None);
        await innerTask!;

        var mail = _database.MainDb.Queryable<TenantGameMailEntity>().Single(u => u.Id == 601);
        Assert.Equal(GameMailStatus.Sent, mail.Status);
        // MailId 通道键已清除（与旧引擎行为一致）。
        Assert.False(_cache.ContainsKey("MailId"));
        // Fulfillment 请求真实发出，载荷为 post_mail 的 PostMailRequest。
        var request = Assert.Single(_httpHandler.Requests);
        Assert.Equal("http://127.0.0.1:21991/game/api/post_mail", request.RequestUri!.ToString());
        Assert.Contains("\"Title\":\"EmailTask 正向测试\"", request.Body!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SentMail_ReturnsWithoutSending()
    {
        InsertMail(602, GameMailStatus.Sent, isAllPost: false, playerIdListJson: "[]");
        var task = BuildTask();
        _cache.Set("MailId", 602L);

        var innerTask = await task.ExecuteAsync(CancellationToken.None);
        if (innerTask != null)
        {
            await innerTask;
        }

        Assert.Empty(_httpHandler.Requests);
        Assert.Equal(GameMailStatus.Sent, _database.MainDb.Queryable<TenantGameMailEntity>().Single(u => u.Id == 602).Status);
        Assert.False(_cache.ContainsKey("MailId"));
    }

    [Fact]
    public async Task ZeroMailId_ReturnsImmediately()
    {
        var task = BuildTask();
        _cache.Set("MailId", 0L);

        var innerTask = await task.ExecuteAsync(CancellationToken.None);

        Assert.Null(innerTask);
        Assert.Empty(_httpHandler.Requests);
    }

    private EmailTask BuildTask()
    {
        var scopeFactory = _provider.GetRequiredService<IServiceScopeFactory>();
        using (var scope = scopeFactory.CreateScope())
        {
            return new EmailTask(NullLogger<EmailTask>.Instance,
                                 scope.ServiceProvider.GetRequiredService<SqlSugarRepository<TenantGameMailEntity>>(),
                                 scope.ServiceProvider.GetRequiredService<SqlSugarRepository<TenantGameAreaEntity>>(),
                                 scope.ServiceProvider.GetRequiredService<SqlSugarRepository<TenantGameRoleEntity>>(),
                                 new HttpClient(_httpHandler),
                                 scope.ServiceProvider.GetRequiredService<SystemCacheService>());
        }
    }

    private void SeedArea(long areaId)
    {
        _database.MainDb.Insertable(new TenantGameAreaEntity
        {
            Id = areaId,
            Name = $"EmailTaskArea-{areaId}",
            ClassifyId = 1,
            IsEnabled = true,
            IsDeleted = false,
            ServerStatus = GameServerStatus.Online,
            HttpManageUrl = "http://127.0.0.1:21991",
            TcpSocketUrl = "tcp://127.0.0.1:21991:7000",
            WebSocketUrl = "ws://127.0.0.1:21991/ws",
            Capacity = 100,
            CurrentLoad = 1,
            CreationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            LastMaintenanceTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ServerVersion = "1.0.0",
            ServerType = "Normal",
            LocalizationKey = "area_test",
            Language = GameLanguage.ChineseSimplified,
            AllowedPlayerProfile = "all",
            ServerEvents = "none",
            AccessControls = "public",
            ServerContainerId = "container",
            ServerParams = "{}",
            AppVersion = "1.0.0",
        }).ExecuteCommand();
    }

    private void InsertMail(long mailId, GameMailStatus status, bool isAllPost, string playerIdListJson)
    {
        _database.MainDb.Insertable(new TenantGameMailEntity
        {
            Id = mailId,
            AreaId = 91,
            PlayerIdListJson = playerIdListJson,
            IsAllPost = isAllPost,
            Title = "EmailTask 正向测试",
            SubTitle = "sub",
            Body = "body",
            AttachmentsJson = "[]",
            Status = status,
            ExpirationTime = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds(),
            MailType = GameMailType.SystemMail,
            IsEnabled = true,
            IsDeleted = false,
        }).ExecuteCommand();
    }

    /// <summary>
    /// 捕获请求并返回 Foundation 成功信封（HttpJsonResultData，code=0）。
    /// </summary>
    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        public List<CapturedRequest> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content == null ? null : request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            Requests.Add(new CapturedRequest(request.RequestUri, body));
            var envelope = HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PostMailResponse()));
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonHelper.Serialize(envelope), Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// 捕获的 HTTP 请求（测试断言用；C# 元组语法在后端禁用，见 NoTupleSyntaxTests）。
    /// </summary>
    private sealed record CapturedRequest(Uri? RequestUri, string? Body);
}
