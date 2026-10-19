using System.Net;
using System.Text.Json;
using GameFrameX.Application.Game.ServerManager.Mail;
using GameFrameX.Entity.Game.Mail;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;

namespace GameFrameX.Core.Tests.MailCampaign;

/// <summary>
/// 运营邮件 Campaign Server 接口客户端契约自检（C134 / GFX-141，C175 调整调用签名）。
/// <para>跨仓库契约铁律：Admin 与 Server 分属不同仓库，DTO 字段名 / 路由 action 名 / 响应业务码必须逐字对齐。
/// 本测试通过伪造 <see cref="HttpMessageHandler"/> 端到端验证：</para>
/// <list type="number">
///   <item>URL 路由：<c>{HttpManageUrl}/{FulfillmentApiPrefix}/{publish,preview,revoke,query}_mail_campaign</c>（snake_case action，全部 POST，C175 起前缀取自区服而非全局配置）。</item>
///   <item>请求体字段名：序列化 JSON 键名集合与 Server <c>PublishMailCampaignRequest</c> 等逐字对齐。</item>
///   <item>响应信封解包：<c>HttpJsonResultData<string>.Success(JsonHelper.Serialize(data))</c> → 业务对象；<c>HttpJsonResultData<string>.Fail</c> → 抛 <see cref="InvalidOperationException"/>。</item>
///   <item>Server 业务码原样透传（不做映射，映射在服务层）。</item>
/// </list>
/// </summary>
public class MailCampaignServerClientTests
{
    private const string HttpManageUrl = "https://game-server.test";

    [Fact]
    public async Task PublishAsync_Posts_To_SnakeCase_Action_Under_FulfillmentPrefix()
    {
        var handler = new FakeHandler(HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PublishMailCampaignResponse
        {
            Code = MailCampaignServerCode.Ok,
            CampaignId = 9527,
            Version = 3,
            PublishedTime = 1780000000,
            Message = "ok",
        })).ToString());

        var client = CreateClient(handler);

        await client.PublishAsync(CreateArea(), NewPublishRequest());

        Assert.Equal("https://game-server.test/game/api/publish_mail_campaign", handler.LastRequestUrl);
        Assert.Equal("POST", handler.LastRequestMethod);
    }

    [Fact]
    public async Task PublishAsync_Request_Body_Carries_Server_Contract_Field_Names()
    {
        var handler = new FakeHandler(HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PublishMailCampaignResponse
        {
            Code = MailCampaignServerCode.Ok,
            CampaignId = 1,
        })).ToString());
        var client = CreateClient(handler);

        await client.PublishAsync(CreateArea(), NewPublishRequest());

        var json = handler.LastRequestBodyJson;
        // Server 契约字段名（Foundation 序列化为 PascalCase，逐字对齐 C# 属性名，不允许漂移）
        Assert.Contains("CampaignId", json.Keys);
        Assert.Contains("TemplateId", json.Keys);
        Assert.Contains("MailType", json.Keys);
        Assert.Contains("Titles", json.Keys);
        Assert.Contains("Contents", json.Keys);
        Assert.Contains("SenderNames", json.Keys);
        Assert.Contains("Attachments", json.Keys);
        Assert.Contains("ServerIds", json.Keys);
        Assert.Contains("ChannelIds", json.Keys);
        Assert.Contains("MinLevel", json.Keys);
        Assert.Contains("MaxLevel", json.Keys);
        Assert.Contains("ExpireAttachmentPolicy", json.Keys);
        Assert.Contains("ExpireTime", json.Keys);
        Assert.Contains("Operator", json.Keys);
    }

    [Fact]
    public async Task PublishAsync_Unwraps_Envelope_And_Returns_Server_Code()
    {
        var handler = new FakeHandler(HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PublishMailCampaignResponse
        {
            Code = MailCampaignServerCode.HasExist,
            CampaignId = 9527,
            Version = 1,
            PublishedTime = 1780000000,
            Message = "已存在",
        })).ToString());
        var client = CreateClient(handler);

        var resp = await client.PublishAsync(CreateArea(), NewPublishRequest());

        Assert.Equal(MailCampaignServerCode.HasExist, resp.Code);
        Assert.Equal(9527, resp.CampaignId);
        Assert.Equal(1, resp.Version);
        Assert.Equal(1780000000, resp.PublishedTime);
        Assert.Equal("已存在", resp.Message);
    }

    [Fact]
    public async Task PublishAsync_When_Server_Envelope_Fails_Throws_InvalidOperationException()
    {
        var handler = new FakeHandler(HttpJsonResultData<string>.Fail("server rejected").ToString());
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
                                                                client.PublishAsync(CreateArea(), NewPublishRequest()));
    }

    [Fact]
    public async Task PreviewAsync_Posts_To_Preview_Action()
    {
        var handler = new FakeHandler(HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PreviewMailCampaignResponse
        {
            Code = MailCampaignServerCode.Ok,
            EstimatedHitCount = -1,
            Message = "ok",
        })).ToString());
        var client = CreateClient(handler);

        var resp = await client.PreviewAsync(CreateArea(), new PreviewMailCampaignRequest
        {
            MailType = MailCampaignMailType.Operation,
            Titles = new List<MailLocalizedContent> { new() { Language = "zh", Text = "t", }, },
        });

        Assert.Equal("https://game-server.test/game/api/preview_mail_campaign", handler.LastRequestUrl);
        Assert.Equal(MailCampaignServerCode.Ok, resp.Code);
        Assert.Equal(-1, resp.EstimatedHitCount);
    }

    [Fact]
    public async Task RevokeAsync_Posts_To_Revoke_Action_With_CampaignId_And_Operator()
    {
        var handler = new FakeHandler(HttpJsonResultData<string>.Success(JsonHelper.Serialize(new RevokeMailCampaignResponse
        {
            Code = MailCampaignServerCode.Ok,
            CampaignId = 9527,
            RevokedTime = 1780000999,
            Message = "ok",
        })).ToString());
        var client = CreateClient(handler);

        var resp = await client.RevokeAsync(CreateArea(), new RevokeMailCampaignRequest
        {
            CampaignId = 9527,
            Operator = "admin01",
        });

        Assert.Equal("https://game-server.test/game/api/revoke_mail_campaign", handler.LastRequestUrl);
        Assert.Equal("9527", handler.LastRequestBodyJson["CampaignId"]?.ToString());
        Assert.Equal("admin01", handler.LastRequestBodyJson["Operator"]?.ToString());
        Assert.Equal(MailCampaignServerCode.Ok, resp.Code);
        Assert.Equal(1780000999, resp.RevokedTime);
    }

    [Fact]
    public async Task QueryAsync_Posts_To_Query_Action()
    {
        var handler = new FakeHandler(HttpJsonResultData<string>.Success(JsonHelper.Serialize(new QueryMailCampaignResponse
        {
            Code = MailCampaignServerCode.Ok,
            Total = 0,
            Campaigns = new List<ServerMailCampaignState>(),
        })).ToString());
        var client = CreateClient(handler);

        var resp = await client.QueryAsync(CreateArea(), new QueryMailCampaignRequest
        {
            CampaignId = 9527,
        });

        Assert.Equal("https://game-server.test/game/api/query_mail_campaign", handler.LastRequestUrl);
        Assert.Equal(MailCampaignServerCode.Ok, resp.Code);
        Assert.Equal(0, resp.Total);
    }

    [Fact]
    public async Task PublishAsync_Respects_Custom_FulfillmentPrefix_From_Area()
    {
        var handler = new FakeHandler(HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PublishMailCampaignResponse
        {
            Code = MailCampaignServerCode.Ok,
        })).ToString());
        var client = CreateClient(handler);

        await client.PublishAsync(CreateArea(fulfillmentPrefix: "custom/prefix"), NewPublishRequest());

        Assert.Equal("https://game-server.test/custom/prefix/publish_mail_campaign", handler.LastRequestUrl);
    }

    [Fact]
    public void Action_Constants_Match_Server_SnakeCase_Names()
    {
        Assert.Equal("publish_mail_campaign", MailCampaignServerClient.PublishAction);
        Assert.Equal("preview_mail_campaign", MailCampaignServerClient.PreviewAction);
        Assert.Equal("revoke_mail_campaign", MailCampaignServerClient.RevokeAction);
        Assert.Equal("query_mail_campaign", MailCampaignServerClient.QueryAction);
    }

    private static PublishMailCampaignRequest NewPublishRequest()
    {
        return new PublishMailCampaignRequest
        {
            CampaignId = 0,
            TemplateId = 0,
            MailType = MailCampaignMailType.Operation,
            Titles = new List<MailLocalizedContent> { new() { Language = "zh", Text = "title", }, },
            Contents = new List<MailLocalizedContent> { new() { Language = "zh", Text = "body", }, },
            SenderNames = new List<MailLocalizedContent> { new() { Language = "zh", Text = "sender", }, },
            Attachments = new List<MailCampaignAttachment>
            {
                new() { SlotId = 1, RewardType = 1, ItemId = 1001, Amount = 10, IconId = 0, },
            },
            ServerIds = new List<int> { 1, 2, },
            ChannelIds = new List<int>(),
            MinLevel = 0,
            MaxLevel = 0,
            ExpireAttachmentPolicy = MailCampaignExpirePolicy.KeepUnclaimed,
            ExpireTime = 0,
            Operator = "admin01",
        };
    }

    private static MailCampaignServerClient CreateClient(FakeHandler handler)
    {
        var httpClient = new HttpClient(handler);
        return new MailCampaignServerClient(httpClient);
    }

    private static TenantGameAreaEntity CreateArea(string httpManageUrl = HttpManageUrl, string fulfillmentPrefix = "game/api")
    {
        return new TenantGameAreaEntity
        {
            HttpManageUrl = httpManageUrl,
            FulfillmentApiPrefix = fulfillmentPrefix,
        };
    }

    /// <summary>
    /// 伪造 HttpMessageHandler：捕获请求 URL/方法/体，返回预设 JSON 信封。
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
}