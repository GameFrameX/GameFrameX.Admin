using GameFrameX.Core.Service;
using GameFrameX.Foundation.Json;
using GameFrameX.Foundation.Logger;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GameFrameX.Core.Filter;

/// <summary>
/// 全局请求过滤器（Client / Hub 共享版，C236 收敛）。
/// <para>三 host 统一契约语义：对继承 <see cref="GameFrameX.Core.Service.BaseController"/> 的控制器打印请求日志；
/// console 开发态输出含 Controller/Action 的长格式，非 console（生产）输出仅参数的紧凑格式。</para>
/// <para>共享原因：Client / Hub 原各自持有一份近似实现，复制分叉漏修有真实前科（ES Writer captive dependency），
/// 收敛到 Core 单一实现（以 Client 超集版为准，Hub 原死依赖 LauncherOptions 已丢弃）。</para>
/// </summary>
public class GlobalRequestHandlerFilter : IActionFilter
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is BaseController baseController)
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            if (LogOptions.Default.IsConsole)
            {
                LogHelper.InfoConsole($"req ==> {controllerActionDescriptor.ControllerName}/{controllerActionDescriptor.ActionName} {context.ActionDescriptor.Id}: {JsonHelper.Serialize(context.ActionArguments)}");
            }
            else
            {
                LogHelper.Info($"req ==> {JsonHelper.Serialize(context.ActionArguments)}");
            }
        }
    }
}