using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace GameFrameX.Web.Api.ApiRouting;

/// <summary>
/// 动态 API 控制器的模型绑定失败显式 400 防线（C391 / GFX-923）。
/// <para>
/// 动态服务类无 [ApiController]，Startup 配置的 ApiBehaviorOptions.InvalidModelStateResponseFactory
/// 只被 [ApiController] 注入的 ModelStateInvalidFilter 消费，对动态控制器完全失活——body 反序列化
/// 失败时复杂参数静默为 null，首次解引用即 500 NRE（Api#291）。本过滤器把同一工厂接到全量
/// Admin MVC action 上，消灭「契约错位 → 静默 null → 500 NRE」这一类问题。
/// </para>
/// <para>
/// 仅拦截绑定失败（JsonException 等携带 Exception 或空 ErrorMessage 的错误）；纯校验特性违规
/// （[Range]/[Required] 等，ErrorMessage 非空且无 Exception）维持旧语义放行——历史管线对动态
/// 控制器从不消费 ModelState，前端存在大量既定用法依赖这一点（如 RewardItemsEditor 以
/// pageSize=1000 调分页接口，超出 BasePageRequest.PageSize 的 [Range(1,100)]，校验被忽略、
/// 查询照常执行）。校验违规的拦截属于行为变更，须连同契约治理另行立项，不在本防线内夹带。
/// </para>
/// </summary>
public sealed class AdminInvalidModelStateFilter : IActionFilter
{
    private readonly ApiBehaviorOptions _options;

    public AdminInvalidModelStateFilter(IOptions<ApiBehaviorOptions> options)
    {
        _options = options.Value;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!HasBindingFailure(context))
        {
            return;
        }

        // 孤立宿主/测试未配置工厂时保持既有放行行为，不在防线自身引入 NRE
        if (_options.InvalidModelStateResponseFactory == null)
        {
            return;
        }

        context.Result = _options.InvalidModelStateResponseFactory(context);
    }

    /// <summary>
    /// 绑定失败的判据：错误携带 Exception（TryAddModelException）或 ErrorMessage 为空——
    /// 校验特性违规走 TryAddModelError，ErrorMessage 恒非空且无 Exception，二者以此区分。
    /// </summary>
    private static bool HasBindingFailure(ActionExecutingContext context)
    {
        foreach (var entry in context.ModelState.Values)
        {
            foreach (var error in entry.Errors)
            {
                if (error.Exception != null || string.IsNullOrEmpty(error.ErrorMessage))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
