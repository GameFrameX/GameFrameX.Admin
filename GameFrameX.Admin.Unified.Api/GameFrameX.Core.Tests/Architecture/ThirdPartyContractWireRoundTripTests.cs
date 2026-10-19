using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Client.Api.GrafanaLoki;
using GameFrameX.Client.Api.Integrations.Advertisement.Dto;
using GameFrameX.Client.Api.Integrations.Payment.Dto;
using GameFrameX.Client.Api.Integrations.Payment.Dto.Google;
using GameFrameX.Email.TencentSes;
using GameFrameX.Foundation.Json;
using GameFrameX.Hub.Api.Dto.WebHook;
using GameFrameX.Payment.Xsolla.Order;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace GameFrameX.Core.Tests.Architecture;

// C292 / GFX-698：第三方契约 DTO PascalCase 化后的 wire 冻结验证。
// 任务 1 的承诺是 wire format 零变化：Client.Api / Hub.Api 侧由 ClientHubDtoWireSnapshotTests（按
// JsonPropertyName ?? Name 计算的快照）零变化覆盖；本测试补齐快照外程序集
// （Payment.Xsolla / Email.TencentSes / GrafanaLoki 命名空间）的 round-trip 与真实样例反序列化，
// 并锁定 [FromQuery(Name)] query 绑定路径（MVC query 绑定不读 JsonPropertyName，AdMob/UnityAds/zlpay
// 回调的 wire 是 query string，绑定名必须靠 FromQueryAttribute 显式锁定）。
public sealed class ThirdPartyContractWireRoundTripTests
{
    private readonly ITestOutputHelper _output;

    public ThirdPartyContractWireRoundTripTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Xsolla_OrderCreate_SerializesOriginalWireNames()
    {
        var orderParams = new OrderManager.OrderParamsCreate
        {
            Sandbox = true,
        };
        orderParams.User.Id.Value = "1125905017757380608";
        orderParams.User.Email.Value = "player@example.test";
        orderParams.Settings.Currency = "SGD";
        orderParams.Settings.ExternalId = "700505389293573";
        orderParams.Purchase.Items.Add(new OrderManager.OrderParamsCreateItem(2, "sku-9001"));

        // 与生产 CreateAsync 同路径：JsonHelper.Serialize(x, JsonHelper.DefaultOptions)
        var json = JsonHelper.Serialize(orderParams, JsonHelper.DefaultOptions);
        _output.WriteLine(json);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(2, root.GetProperty("purchase").GetProperty("items")[0].GetProperty("quantity").GetInt32());
        Assert.Equal("sku-9001", root.GetProperty("purchase").GetProperty("items")[0].GetProperty("sku").GetString());
        Assert.Equal("SGD", root.GetProperty("settings").GetProperty("currency").GetString());
        Assert.Equal("700505389293573", root.GetProperty("settings").GetProperty("external_id").GetString());
        Assert.Equal("1125905017757380608", root.GetProperty("user").GetProperty("id").GetProperty("value").GetString());
        Assert.Equal("player@example.test", root.GetProperty("user").GetProperty("email").GetProperty("value").GetString());
        Assert.True(root.GetProperty("sandbox").GetBoolean());
    }

    [Fact]
    public void Xsolla_OrderCreateResponse_DeserializesOriginalWireNames()
    {
        const string payload = "{\"token\":\"t-42\",\"order_id\":362310922}";
        var response = JsonHelper.Deserialize<OrderManager.OrderParamsCreateResponse>(payload)!;
        Assert.Equal("t-42", response.Token);
        Assert.Equal(362310922, response.OrderId);
    }

    [Fact]
    public void Xsolla_OrderPaidWebHook_ParsesRealPayload()
    {
        // 真实样例报文（节选自 XsollaWebHookOrderPaidRequest 备注中的 order_paid 实收回调）
        const string payload = "{\"items\":[{\"sku\":\"18004\",\"type\":\"virtual_good\",\"is_pre_order\":false,\"quantity\":1,\"amount\":\"6.5000\",\"promotions\":[]}],"
                               + "\"notification_type\":\"order_paid\","
                               + "\"order\":{\"id\":362310922,\"mode\":\"sandbox\",\"currency_type\":\"real\",\"currency\":\"SGD\",\"amount\":\"6.5000\",\"status\":\"paid\",\"platform\":\"xsolla\",\"comment\":null,\"invoice_id\":\"1734537180\",\"promotions\":[]},"
                               + "\"user\":{\"external_id\":\"1125905017757380608\",\"email\":\"zl_dev@zilancreative.com\",\"country\":\"SG\"}}";

        var webhook = JsonHelper.Deserialize<XsollaWebHookOrderPaidRequest>(payload)!;
        Assert.Equal("order_paid", webhook.NotificationType);
        Assert.Equal(362310922, webhook.Order.Id);
        Assert.Equal("sandbox", webhook.Order.Mode);
        Assert.Equal("real", webhook.Order.CurrencyType);
        Assert.Equal("SGD", webhook.Order.Currency);
        Assert.Equal("6.5000", webhook.Order.Amount);
        Assert.Equal("paid", webhook.Order.Status);
        Assert.Equal("1734537180", webhook.Order.InvoiceId);
        Assert.Equal("1125905017757380608", webhook.User.ExternalId);
        Assert.Equal("zl_dev@zilancreative.com", webhook.User.Email);
    }

    [Fact]
    public void Xsolla_UserValidationWebHook_ParsesRealPayload()
    {
        // Xsolla 官方 user_validation 报文样例（user.email 等镜像类未建模的字段应被忽略而不报错）
        const string payload = "{\"notification_type\":\"user_validation\",\"settings\":{\"project_id\":280066,\"merchant_id\":789907},"
                               + "\"user\":{\"id\":\"user-1\",\"email\":\"player@example.test\"}}";

        var webhook = JsonHelper.Deserialize<XsollaWebHookUserValidationRequest>(payload)!;
        Assert.Equal("user_validation", webhook.NotificationType);
        Assert.Equal(280066, webhook.Settings.ProjectId);
        Assert.Equal(789907, webhook.Settings.MerchantId);
        Assert.Equal("user-1", webhook.User.Id);
    }

    [Fact]
    public void Google_PubSubPush_ParsesRealPayloadWithBothNamingVariants()
    {
        // Google 双命名镜像：Pub/Sub push 为 camelCase（messageId/publishTime），HTTP push 镜像为 snake_case
        const string payload = "{\"message\":{\"data\":\"eyJ2ZXJzaW9uIjoiMS4wIn0=\",\"messageId\":\"msg-camel\",\"message_id\":\"msg-snake\","
                               + "\"publishTime\":\"2025-07-22T15:24:02Z\",\"publish_time\":\"2025-07-22T15:24:02Z\"},"
                               + "\"subscription\":\"subscriptions/gameframe-x\"}";

        var webhook = JsonHelper.Deserialize<GoogleWebHookRequest>(payload)!;
        Assert.Equal("subscriptions/gameframe-x", webhook.Subscription);
        Assert.Equal("eyJ2ZXJzaW9uIjoiMS4wIn0=", webhook.Message.Data);
        Assert.Equal("msg-camel", webhook.Message.messageId);
        Assert.Equal("msg-snake", webhook.Message.message_id);
        Assert.Equal("2025-07-22T15:24:02Z", webhook.Message.publishTime);
        Assert.Equal("2025-07-22T15:24:02Z", webhook.Message.publish_time);
    }

    [Fact]
    public void Tapd_WebHook_ParsesRealPayload()
    {
        // TAPD workitem 变更 webhook 真实样例（event/from 为关键字字段，Pascal 化后由 JsonPropertyName 锁定）
        const string payload = "{\"event\":{\"object_info\":{\"current_owner\":\"owner-1\",\"name\":\"story-name\",\"title\":\"bug-title\"},"
                               + "\"workspace_id\":\"ws-1\",\"user\":\"operator\",\"object_type\":\"story\",\"id\":\"1125905017757380608\","
                               + "\"timestamp\":1734537180,\"event_key\":\"workitem.update\",\"from\":\"webhook\",\"event_id\":\"event-1\"},"
                               + "\"workspace_id\":\"ws-1\"}";

        var webhook = JsonHelper.Deserialize<TapdWebHookData>(payload)!;
        Assert.Equal("ws-1", webhook.WorkspaceId);
        Assert.Equal("workitem.update", webhook.Event.EventKey);
        Assert.Equal("webhook", webhook.Event.From);
        Assert.Equal("1125905017757380608", webhook.Event.Id);
        Assert.Equal("story", webhook.Event.ObjectType);
        Assert.Equal("owner-1", webhook.Event.ObjectInfo.CurrentOwner);
        Assert.Equal("story-name", webhook.Event.ObjectInfo.Name);
        Assert.Equal("bug-title", webhook.Event.ObjectInfo.Title);
    }

    [Fact]
    public void AdMob_QueryBindingNames_AreLockedToSsvContract()
    {
        // AdMob SSV 回调为 GET query 绑定：[FromQuery(Name)] 必须逐项等于原 SSV 参数名
        AssertQueryBindingLocked<AdMobCallbackRequest>(new Dictionary<string, string>
        {
            { "AdUnit", "ad_unit" },
            { "AdNetwork", "ad_network" },
            { "UserId", "user_id" },
            { "RewardItem", "reward_item" },
            { "RewardAmount", "reward_amount" },
            { "Timestamp", "timestamp" },
            { "TransactionId", "transaction_id" },
            { "Signature", "signature" },
            { "KeyId", "key_id" },
            { "CustomData", "custom_data" },
        });
    }

    [Fact]
    public void UnityAds_QueryBindingNames_AreLockedToCallbackContract()
    {
        AssertQueryBindingLocked<AdvertisementUnityAdsCallbackRequest>(new Dictionary<string, string>
        {
            { "Sid", "sid" },
            { "Oid", "oid" },
            { "Hmac", "hmac" },
        });
    }

    [Fact]
    public void WebShop_zlpay_QueryBindingNames_AreLockedToNotifyContract()
    {
        // zlpay（易支付口径）notify 回传为 GET query 绑定，参数名与验签字典 key（BuildNotifySignParameters）一一对应
        AssertQueryBindingLocked<TenantGameWebShopEntityBuyNotifyRequest>(new Dictionary<string, string>
        {
            { "Pid", "pid" },
            { "TradeNo", "trade_no" },
            { "OutTradeNo", "out_trade_no" },
            { "Type", "type" },
            { "Name", "name" },
            { "Money", "money" },
            { "TradeStatus", "trade_status" },
            { "Param", "param" },
            { "Sign", "sign" },
            { "SignType", "sign_type" },
        });
    }

    [Fact]
    public void TencentSes_TemplateData_RoundTripsOriginalWireNames()
    {
        // 生产路径 TencentSesEmailSender 用原生 JsonSerializer（无 options）双向序列化 TemplateData
        var data = new SendEmailTemplateData
        {
            Username = "player@example.test",
            VerificationCode = "123456",
        };

        var json = JsonSerializer.Serialize(data);
        using var document = JsonDocument.Parse(json);
        Assert.Equal("player@example.test", document.RootElement.GetProperty("username").GetString());
        Assert.Equal("123456", document.RootElement.GetProperty("verification_code").GetString());
        Assert.Equal("5", document.RootElement.GetProperty("expiry_minutes").GetString());

        var restored = JsonSerializer.Deserialize<SendEmailTemplateData>("{\"username\":\"u1\",\"verification_code\":\"654321\",\"expiry_minutes\":\"10\"}")!;
        Assert.Equal("u1", restored.Username);
        Assert.Equal("654321", restored.VerificationCode);
        Assert.Equal("10", restored.ExpiryMinutes);
    }

    [Fact]
    public void GrafanaLoki_Payloads_SerializeOriginalWireNames()
    {
        // 出站 Loki JSON：EventDataWrapper（event_name/event_data）与推送体（streams/stream/values）
        var wrapper = new GrafanaLokiAnalyticsEntry.EventDataWrapper("login", "{\"uid\":1}");
        var wrapperJson = JsonHelper.Serialize(wrapper);
        using var wrapperDocument = JsonDocument.Parse(wrapperJson);
        Assert.Equal("login", wrapperDocument.RootElement.GetProperty("event_name").GetString());
        Assert.Equal("{\"uid\":1}", wrapperDocument.RootElement.GetProperty("event_data").GetString());

        var payload = new GrafanaLokiAnalyticsData();
        var stream = new GrafanaLokiAnalyticsDataStreamsItem();
        stream.Stream["app"] = "gameframex";
        stream.Values.Add(new List<string> { "1734537180000000000", "line-1" });
        payload.Streams.Add(stream);
        var payloadJson = JsonHelper.Serialize(payload);
        using var payloadDocument = JsonDocument.Parse(payloadJson);
        var firstStream = payloadDocument.RootElement.GetProperty("streams")[0];
        Assert.Equal("gameframex", firstStream.GetProperty("stream").GetProperty("app").GetString());
        Assert.Equal("line-1", firstStream.GetProperty("values")[0][1].GetString());
    }

    /// <summary>
    /// query 绑定契约锁定：每个属性必须同时带 [FromQuery(Name = 原参数名)]（MVC query 绑定）
    /// 与 [JsonPropertyName(原参数名)]（wire 快照口径），防止任一通道回归旧小写属性名。
    /// </summary>
    private static void AssertQueryBindingLocked<TType>(Dictionary<string, string> expectedByPropertyName)
    {
        foreach (var (propertyName, expectedName) in expectedByPropertyName)
        {
            var property = typeof(TType).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
                            ?? throw new InvalidOperationException($"缺少属性 {typeof(TType).Name}.{propertyName}");

            var fromQuery = property.GetCustomAttribute<FromQueryAttribute>();
            Assert.True(fromQuery != null, $"{typeof(TType).Name}.{propertyName} 缺少 [FromQuery(Name)]，query 绑定将无法匹配原参数 {expectedName}");
            Assert.Equal(expectedName, fromQuery.Name);

            var jsonPropertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
            Assert.Equal(expectedName, jsonPropertyName);
        }
    }
}
