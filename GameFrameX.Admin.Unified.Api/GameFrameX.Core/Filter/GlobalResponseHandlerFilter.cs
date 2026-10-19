using GameFrameX.Foundation.Json;
using GameFrameX.Foundation.Logger;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GameFrameX.Core.Filter;

/// <summary>
/// 全局请求响应返回处理（Client / Hub 共享版，C236 收敛）。
/// <para>三 host 统一契约语义：对 <see cref="ObjectResult"/> 打印响应日志；
/// console 开发态输出含 Controller/Action 的长格式，非 console（生产）输出仅状态码与载荷的紧凑格式。</para>
/// <para>共享原因：Client / Hub 原各自持有一份近似实现，收敛到 Core 单一实现（以 Client 超集版为准）。</para>
/// </summary>
public class GlobalResponseHandlerFilter : IActionFilter
{
    /// <summary>
    ///
    /// </summary>
    public GlobalResponseHandlerFilter()
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            if (LogOptions.Default.IsConsole)
            {
                LogHelper.InfoConsole($"resp ==> {controllerActionDescriptor.ControllerName}/{controllerActionDescriptor.ActionName} {context.ActionDescriptor.Id}: {context.HttpContext.Response.StatusCode} {JsonHelper.Serialize(objectResult.Value)}");
            }
            else
            {
                LogHelper.Info($"resp ==> StatusCode : {context.HttpContext.Response.StatusCode} => {JsonHelper.Serialize(objectResult.Value)}");
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}