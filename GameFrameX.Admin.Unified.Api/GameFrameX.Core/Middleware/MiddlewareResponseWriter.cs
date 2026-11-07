namespace GameFrameX.Core.Middleware;

/// <summary>
/// 中间件错误响应共享出口：MVC 之前的中间件层拒绝响应统一由本类型书写，
/// 各中间件不得私写 <c>JsonSerializer.Serialize</c> / <c>Response.WriteAsync</c>
/// （守护测试 <c>MiddlewareErrorResponseWriterGuardTests</c> 锁定防回潮）。
/// <para>两种 wire 形态（历史契约并存，不可合并为单一形态）：</para>
/// <list type="bullet">
/// <item><see cref="WriteRejectedAsync"/>：PascalCase <c>{Code, Message}</c>，Code 恒为 HTTP 状态码 ——
/// Client.Api（PublicCallbackResolution）与 Hub.Api（GameAppOpenAccessAuth）及协议版本中间件的拒绝出口。</item>
/// <item><see cref="WriteEnvelopeRejectedAsync"/>：camelCase <c>{code, message}</c>，601xx 信封码
/// （签名 60101-60103 / 加密信封 60104-60106）—— 与 MVC 主信封 camelCase 一致、客户端 Lua 读 <c>tbErr.code</c> 的双端冻结契约。</item>
/// </list>
/// </summary>
public static class MiddlewareResponseWriter
{
    /// <summary>
    /// 写 PascalCase <c>{Code, Message}</c> JSON 拒绝响应；Code 恒等于 <paramref name="statusCode"/>。
    /// </summary>
    /// <param name="context">HTTP 上下文。</param>
    /// <param name="statusCode">HTTP 状态码（响应状态，兼作 Code 值）。</param>
    /// <param name="message">拒绝原因。</param>
    public static async Task WriteRejectedAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new { Code = statusCode, Message = message, });
        await context.Response.WriteAsync(result);
    }

    /// <summary>
    /// 写 camelCase <c>{code, message}</c> JSON 拒绝响应（601xx 信封码形态；与 MVC 主信封一致，客户端 Lua 读 <c>tbErr.code</c>）。
    /// </summary>
    /// <param name="context">HTTP 上下文。</param>
    /// <param name="statusCode">HTTP 状态码。</param>
    /// <param name="code">信封码（60101-60106）。</param>
    /// <param name="message">拒绝原因。</param>
    public static async Task WriteEnvelopeRejectedAsync(HttpContext context, int statusCode, int code, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { code, message, }));
    }
}
