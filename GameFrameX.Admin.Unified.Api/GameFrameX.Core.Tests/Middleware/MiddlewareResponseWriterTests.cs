using System.Text.Json;
using Microsoft.AspNetCore.Http;
using GameFrameX.Core.Middleware;

namespace GameFrameX.Core.Tests.Middleware;

/// <summary>
/// C270 共享中间件错误响应 writer 行为测试：两种 wire 形态（PascalCase 通用错误 / camelCase 信封码）与 Code 回退语义。
/// </summary>
public sealed class MiddlewareResponseWriterTests
{
    [Fact]
    public async Task WriteRejectedAsync_WithoutErrorCode_CodeFallsBackToStatusCode()
    {
        var context = CreateContext();

        await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "无效的应用 ID");

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        using (var document = await ParseBodyAsync(context))
        {
            Assert.Equal(StatusCodes.Status401Unauthorized, document.RootElement.GetProperty("Code").GetInt32());
            Assert.Equal("无效的应用 ID", document.RootElement.GetProperty("Message").GetString());
            Assert.False(document.RootElement.TryGetProperty("code", out _));
        }
    }

    [Fact]
    public async Task WriteRejectedAsync_WithErrorCode_CodeUsesErrorCode()
    {
        var context = CreateContext();

        await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status403Forbidden, "租户已禁用", 30600006);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        using (var document = await ParseBodyAsync(context))
        {
            Assert.Equal(30600006, document.RootElement.GetProperty("Code").GetInt32());
            Assert.Equal("租户已禁用", document.RootElement.GetProperty("Message").GetString());
        }
    }

    [Fact]
    public async Task WriteEnvelopeRejectedAsync_CamelCaseEnvelopeShape()
    {
        var context = CreateContext();

        await MiddlewareResponseWriter.WriteEnvelopeRejectedAsync(context, StatusCodes.Status400BadRequest, 60106, "信封解密失败");

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        using (var document = await ParseBodyAsync(context))
        {
            Assert.Equal(60106, document.RootElement.GetProperty("code").GetInt32());
            Assert.Equal("信封解密失败", document.RootElement.GetProperty("message").GetString());
            Assert.False(document.RootElement.TryGetProperty("Code", out _));
        }
    }

    private static DefaultHttpContext CreateContext()
    {
        return new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream(),
            },
        };
    }

    private static async Task<JsonDocument> ParseBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}