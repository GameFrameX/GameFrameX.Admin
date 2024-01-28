using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace GameFrameX.Web.Api.Filter
{
    /// <summary>
    /// 全局请求响应返回处理
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
                ControllerActionDescriptor controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
                Console.WriteLine(
                    $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}: resp ==> {controllerActionDescriptor.ControllerName}/{controllerActionDescriptor.ActionName} {context.ActionDescriptor.Id}: {JsonConvert.SerializeObject(objectResult.Value)}");
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
}