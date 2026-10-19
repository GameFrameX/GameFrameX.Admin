using System.Text.Json.Nodes;
using GameFrameX.Core.Logging;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// C162 日志字段 JSON 形状回归守护。lesson 2026-07-11 的核心回归是：DatabaseLoggingWriter
/// 解析侧与落盘侧用了不同的 JSON 框架（Newtonsoft JToken 解析 + System.Text.Json 序列化），
/// 导致 STJ 把每个 JToken 当作可枚举的 children，对叶子节点序列化成 []。迁回 STJ 后，
/// 解析与序列化同为 JsonNode/System.Text.Json，这些用例锁定落盘字段必须保持对象形状 JSON，
/// 不得重新退化为 []。
/// </summary>
public class LogFieldSerializationGuardTests
{
    // 构造一条历史框架 loggingMonitor 风格的真实日志 JSON（结构对照 DatabaseLoggingWriter 实际读取的字段）。
    private const string LoggingMonitorJson = @"
{
    ""controllerName"": ""SystemAuthService"",
    ""actionName"": ""login"",
    ""actionTypeName"": ""Login"",
    ""displayTitle"": ""登录"",
    ""httpMethod"": ""POST"",
    ""requestUrl"": ""/api/systemAuth/login"",
    ""remoteIPv4"": ""127.0.0.1"",
    ""userAgent"": ""Mozilla/5.0"",
    ""timeOperationElapsedMilliseconds"": 42,
    ""authorizationClaims"": [
        { ""type"": ""Account"", ""value"": ""admin"" },
        { ""type"": ""AccountType"", ""value"": ""1"" }
    ],
    ""parameters"": [
        {
            ""name"": ""input"",
            ""value"": { ""account"": ""admin"", ""password"": ""secret"", ""tenantId"": 1300000000001 }
        }
    ],
    ""returnInformation"": {
        ""httpStatusCode"": 200,
        ""type"": ""object"",
        ""value"": { ""code"": 200, ""type"": ""success"" }
    },
    ""exception"": null
}";

    private static JsonObject ParseMonitor()
    {
        // 与 DatabaseLoggingWriter.WriteSingleAsync 解析方式完全一致。
        return JsonNode.Parse(LoggingMonitorJson)!.AsObject();
    }

    [Fact]
    public void ReturnInformation_SerializesAsObjectJson_NotEmptyArray()
    {
        var monitor = ParseMonitor();

        // 复刻 FillOperationFields 中 target.ReturnResult 的赋值表达式。
        var returnResult = monitor["returnInformation"]?.ToJsonString();

        Assert.NotNull(returnResult);
        // 对象形状 JSON：必须包含 httpStatusCode 数字字段，不得退化为 [] 或 {"httpStatusCode":[]}。
        Assert.Contains("\"httpStatusCode\":200", returnResult);
        Assert.DoesNotContain("[]", returnResult);
    }

    [Fact]
    public void RequestParam_SerializesAsObjectJson_NotEmptyArray()
    {
        var monitor = ParseMonitor();
        var parameters = monitor["parameters"]?.AsArray();

        // 复刻 FillOperationFields 中 target.RequestParam 的赋值表达式（取首个参数的 value）。
        var requestParam = parameters == null || parameters.Count == 0
                               ? null
                               : parameters[0]?["value"]?.ToJsonString();

        Assert.NotNull(requestParam);
        Assert.Contains("\"account\":\"admin\"", requestParam);
        Assert.DoesNotContain("[]", requestParam);
    }

    [Fact]
    public void Exception_SerializesAsObjectJson_NotEmptyArray()
    {
        // 显式给一条非空异常日志，复刻异常/操作日志分支对 exception 字段的序列化。
        var json = LoggingMonitorJson.Replace(@"""exception"": null", @"""exception"": { ""message"": ""boom"", ""type"": ""System.InvalidOperationException"" }");
        var monitor = JsonNode.Parse(json)!.AsObject();

        var exception = monitor["exception"]?.ToJsonString();

        Assert.NotNull(exception);
        Assert.Contains("\"message\":\"boom\"", exception);
        Assert.DoesNotContain("[]", exception);
    }

    [Fact]
    public void StatusField_HttpStatusCode_SerializesAsStringNumber()
    {
        // Status 字段是 string，存 httpStatusCode 数字。ToJsonString 对数字节点产出 ""200""，与历史 dynamic 行为一致。
        var monitor = ParseMonitor();
        var status = monitor["returnInformation"]?["httpStatusCode"]?.ToJsonString();

        Assert.Equal("200", status);
    }

    [Fact]
    public void SerializedReturnInformation_RoundTripsThroughSuccessDetection()
    {
        // 落盘得到的 returnInformation JSON 喂回 IsSuccessfulUnifiedReturn 必须仍判为成功，
        // 登录租户回填依赖这条链路（lesson 2026-07-11：损坏的 [] 会让回填全 miss）。
        var monitor = ParseMonitor();
        var returnResult = monitor["returnInformation"]?.ToJsonString();

        Assert.NotNull(returnResult);
        Assert.True(DatabaseLoggingWriter.IsSuccessfulUnifiedReturn(returnResult));
    }

    [Fact]
    public void SerializedParameters_RoundTripsThroughLoginScope()
    {
        var monitor = ParseMonitor();
        var parametersJson = monitor["parameters"]?.ToJsonString();

        Assert.NotNull(parametersJson);
        var scope = DatabaseLoggingWriter.TryReadLoginScope(parametersJson);
        Assert.NotNull(scope);
        Assert.Equal("admin", scope.Account);
        Assert.Equal(1300000000001, scope.TenantId);
    }
}