using GameFrameX.Client.Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.ClientApi;

public class PaymentOptionSettingProviderAppContextTests
{
    [Fact]
    public void GetCurrentAppId_ReturnsMiddlewareAppIdKey()
    {
        var context = new DefaultHttpContext();
        context.Items[AppResolutionMiddleware.AppIdKey] = 123L;

        var appId = PaymentOptionSettingProvider.GetCurrentAppId(context);

        Assert.Equal(123L, appId);
    }

    [Fact]
    public void GetCurrentAppId_IgnoresAppIdHeaderWhenMiddlewareKeyMissing()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["GameFrameX-App-Id"] = "456";

        var appId = PaymentOptionSettingProvider.GetCurrentAppId(context);

        // header 不再是权威来源：缺少中间件解析的 AppIdKey 时返回 null，而非回退到头。
        Assert.Null(appId);
    }

    [Fact]
    public void GetCurrentAppId_IgnoresAppIdQueryWhenMiddlewareKeyMissing()
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = QueryString.Create("appId", "789");

        var appId = PaymentOptionSettingProvider.GetCurrentAppId(context);

        // query 不再是权威来源：缺少中间件解析的 AppIdKey 时返回 null。
        Assert.Null(appId);
    }

    [Fact]
    public void GetCurrentAppId_PrefersMiddlewareKeyOverHeaderAndQuery()
    {
        var context = new DefaultHttpContext();
        context.Items[AppResolutionMiddleware.AppIdKey] = 111L;
        context.Request.Headers["GameFrameX-App-Id"] = "222";
        context.Request.QueryString = QueryString.Create("appId", "333");

        var appId = PaymentOptionSettingProvider.GetCurrentAppId(context);

        Assert.Equal(111L, appId);
    }

    [Fact]
    public void GetCurrentAppId_ReturnsNullWhenContextNull()
    {
#pragma warning disable CS8625 // 测试意图：传入 null HttpContext 验证回退为 null AppId
        var appId = PaymentOptionSettingProvider.GetCurrentAppId(null);
#pragma warning restore CS8625

        Assert.Null(appId);
    }
}