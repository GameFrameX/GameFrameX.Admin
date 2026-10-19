using Microsoft.AspNetCore.Mvc.Filters;

namespace GameFrameX.Client.Api.Filter;

/// <summary>
/// 登录阻断异常过滤器：将 <see cref="PenaltyLoginBlockedException"/> 转为 <c>HttpJsonResultData<BaseOperationAckResponse>.Error</c>，
/// 保持登录漏斗与正常返回结构一致（Code != 0 表示失败）。
/// </summary>
public sealed class PenaltyLoginBlockedExceptionFilter : IExceptionFilter
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is PenaltyLoginBlockedException ex)
        {
            context.Result = new ObjectResult(HttpJsonResultData<BaseOperationAckResponse>.Error(ex.Code, ex.Message))
            {
                StatusCode = StatusCodes.Status200OK,
            };
            context.ExceptionHandled = true;
        }
    }
}