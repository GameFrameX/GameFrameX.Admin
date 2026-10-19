using GameFrameX.Foundation.Http.Normalization;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Extension;

/// <summary>
/// 将 SqlSugar 乐观锁冲突转换为三个 API Host 共用的 HTTP 409 响应。
/// </summary>
public sealed class OptimisticConcurrencyMiddleware
{
    /// <summary>
    /// 客户端可据此提示用户刷新后重试的稳定错误消息。
    /// </summary>
    public const string ConflictMessage = "数据已被其他用户修改，请刷新后重试";

    /// <summary>
    /// 乐观并发冲突（409）统一数字错误码。Core.Extension 不引用 GameFrameX.Core（反向会成环），
    /// 故以 int 常量落地；与 CommonErrorCode.ConcurrencyConflict 同值，由 ErrorCodeContractTests 锁定同源防漂移。
    /// </summary>
    public const int ConflictErrorCode = 20000005;

    private readonly RequestDelegate _next;

    /// <summary>
    /// 初始化乐观锁异常中间件。
    /// </summary>
    public OptimisticConcurrencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// 执行请求并将版本冲突规范化为 HTTP 409。
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (VersionExceptions)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new HttpJsonResultData<object>
            {
                Code = ConflictErrorCode,
                Message = ConflictMessage,
            });
        }
    }
}