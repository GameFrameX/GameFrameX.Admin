using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace GameFrameX.Web.Api.Filter
{
    /// <summary>
    /// 全局请求过滤器
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
                ControllerActionDescriptor controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;

                Console.WriteLine(
                    $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}: req ==> {controllerActionDescriptor.ControllerName}/{controllerActionDescriptor.ActionName} {context.ActionDescriptor.Id}: {JsonConvert.SerializeObject(context.ActionArguments)}");
            }
        }
    }
}