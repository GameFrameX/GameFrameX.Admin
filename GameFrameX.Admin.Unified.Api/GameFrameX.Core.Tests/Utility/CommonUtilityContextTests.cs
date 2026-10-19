using GameFrameX.Core.Utility;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.Utility;

/// <summary>
/// C40：CommonUtility 显式绑定 IHttpContextAccessor 后的请求上下文边界。
/// </summary>
public class CommonUtilityContextTests
{
    [Fact]
    public void GetLocalhost_NoHttpContext_ReturnsEmptyString()
    {
        CommonUtility.Configure(new HttpContextAccessor());

        Assert.Equal(string.Empty, CommonUtility.GetLocalhost());
    }

    [Fact]
    public void GetLocalhost_UsesOriginalHostHeaderWhenPresent()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("internal.example");
        context.Request.Headers["X-Original-Host"] = "admin.example";
        CommonUtility.Configure(new HttpContextAccessor { HttpContext = context, });

        Assert.Equal("https://admin.example", CommonUtility.GetLocalhost());
    }
}