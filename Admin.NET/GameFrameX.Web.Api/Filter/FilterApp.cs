using Microsoft.AspNetCore.Mvc.Filters;

namespace GameFrameX.Web.Api.Filter
{
    /// <summary>
    /// 
    /// </summary>
    public static class FilterApp
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterCollection"></param>
        public static void Init(FilterCollection filterCollection)
        {
            filterCollection.Add<GlobalRequestHandlerFilter>();
            // 异常
            // filterCollection.Add<GlobalExceptionHandlerFilter>();
            // 全局数据格式统一处理
            filterCollection.Add<GlobalResponseHandlerFilter>();
        }
    }
}