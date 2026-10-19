using GameFrameX.Core.Provider;
using GameFrameX.Foundation.Http.Normalization;
using Microsoft.Extensions.Hosting;

namespace GameFrameX.Core.Filter;

/// <summary>
/// Admin 统一响应 MVC 过滤器。
/// </summary>
public sealed class AdminResultFilter : IAsyncActionFilter, IAsyncExceptionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        if (executed.Exception != null || executed.ExceptionHandled || ShouldSkip(context))
        {
            return;
        }

        if (executed.Result is ObjectResult objectResult)
        {
            // action 已显式返回统一信封（HttpJsonResultData&lt;Output&gt;）时直接透传，防止双重包装
            if (objectResult.Value is IHttpJsonResult)
            {
                return;
            }

            objectResult.Value = AdminResultFactory.Create(objectResult.Value);
            objectResult.DeclaredType = typeof(HttpJsonResultData<object>);
            objectResult.StatusCode ??= StatusCodes.Status200OK;
            return;
        }

        if (executed.Result is EmptyResult)
        {
            executed.Result = new JsonResult(AdminResultFactory.Create())
            {
                StatusCode = StatusCodes.Status200OK,
            };
        }
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        var error = ResolveException(context.Exception);
        // Development 环境把异常栈拼进响应消息并写日志（方便定位 Api#288 等深堆栈 NRE）
        var http = context.HttpContext;
        var env = http.RequestServices.GetService(typeof(Microsoft.Extensions.Hosting.IHostEnvironment)) as Microsoft.Extensions.Hosting.IHostEnvironment;
        if (env?.IsDevelopment() == true && context.Exception is not BusinessException and not VersionExceptions)
        {
            var stack = context.Exception.StackTrace ?? "<no stack>";
            if (stack.Length > 4000) stack = stack[..4000] + "...";
            error = new AdminExceptionResult(error.StatusCode, $"{error.Message}\n[STACK]\n{stack}", error.Code);
            var lf = http.RequestServices.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)) as Microsoft.Extensions.Logging.ILoggerFactory;
            lf?.CreateLogger("AdminResultFilter").LogError(context.Exception, "Unhandled exception");
        }
        context.Result = new JsonResult(AdminResultFactory.CreateError(error.StatusCode, error.Message, error.Code))
        {
            StatusCode = error.StatusCode,
        };
        context.ExceptionHandled = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 创建认证和授权失败响应体。
    /// </summary>
    public static HttpJsonResultData<object> CreateStatusCodeResult(int statusCode, string message, int code)
    {
        return AdminResultFactory.CreateError(statusCode, message, code);
    }

    private static bool ShouldSkip(FilterContext context)
    {
        return context.ActionDescriptor.EndpointMetadata.OfType<NonUnifyAttribute>().Any()
               || context.Filters.OfType<NonUnifyAttribute>().Any()
               || context.ActionDescriptor.EndpointMetadata.OfType<NonActionAttribute>().Any();
    }

    private static AdminExceptionResult ResolveException(Exception exception)
    {
        if (exception is VersionExceptions)
        {
            return new AdminExceptionResult(StatusCodes.Status409Conflict, OptimisticConcurrencyMiddleware.ConflictMessage, OptimisticConcurrencyMiddleware.ConflictErrorCode);
        }

        if (exception is BusinessException businessException)
        {
            // 纯文本构造路径 Code=0（无稳定错误码），出站回退为 HTTP 状态语义
            return new AdminExceptionResult(
                businessException.HttpStatusCode,
                businessException.Message,
                businessException.Code != 0 ? businessException.Code : businessException.HttpStatusCode);
        }

        return new AdminExceptionResult(StatusCodes.Status500InternalServerError, exception.Message, (int)SystemErrorCode.InnerError);
    }

    private sealed class AdminExceptionResult
    {
        public AdminExceptionResult(int statusCode, string message, int code)
        {
            StatusCode = statusCode;
            Message = message;
            Code = code;
        }

        public int StatusCode { get; }

        public string Message { get; }

        public int Code { get; }
    }
}