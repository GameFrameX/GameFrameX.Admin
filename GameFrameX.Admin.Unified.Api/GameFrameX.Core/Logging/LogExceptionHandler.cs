//using Microsoft.AspNetCore.Mvc.Controllers;
//using System.Security.Claims;

//namespace GameFrameX.Core.Logging;

///// <summary>
///// 全局异常处理
///// </summary>
//public class LogExceptionHandler : IGlobalExceptionHandler
//{
//    private readonly IEventPublisher _eventPublisher;

//    public LogExceptionHandler(IEventPublisher eventPublisher)
//    {
//        _eventPublisher = eventPublisher;
//    }

//    public async Task OnExceptionAsync(ExceptionContext context)
//    {
//        var actionMethod = (context.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;
//        var displayNameAttribute = actionMethod.IsDefined(typeof(DisplayNameAttribute), true) ? actionMethod.GetCustomAttribute<DisplayNameAttribute>(true) : default;

//        var systemLogEx = new LogException
//        {
//            Account = user?.FindFirstValue(ClaimConst.Account),
//            RealName = user?.FindFirstValue(ClaimConst.RealName),
//            ControllerName = actionMethod.DeclaringType.FullName,
//            ActionName = actionMethod.Name,
//            DisplayTitle = displayNameAttribute?.DisplayName,
//            Exception = $"异常信息:{context.Exception.Message} 异常来源：{context.Exception.Source} 堆栈信息：{context.Exception.StackTrace}",
//            Message = "全局异常",
//            RequestParam = context.Exception.TargetSite.GetParameters().ToString(),
//            HttpMethod = context.HttpContext.Request.Method,
//            RequestUrl = context.HttpContext.Request.GetRequestUrlAddress(),
//            RemoteIp = context.HttpContext.GetRemoteIpAddressToIPv4(),
//            Browser = context.HttpContext.Request.Headers["User-Agent"],
//            TraceId = traceId,
//            ThreadId = threadId,
//            LogDateTime = DateTime.Now,
//            LogLevel = LogLevel.Error
//        };

//        await _eventPublisher.PublishAsync(new ChannelEventSource("AddAsync:ExLog", systemLogEx));

//        await _eventPublisher.PublishAsync("Send:ErrorMail", systemLogEx);
//    }
//}

