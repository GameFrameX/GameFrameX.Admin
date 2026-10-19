using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Provider;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Web.Api.ApiRouting;

namespace GameFrameX.Core.Tests.ApiRouting;

/// <summary>
/// C391（GFX-923 / Api#291）：模型绑定失败显式 400 防线。
/// 动态控制器无 [ApiController]，Startup 配置的 InvalidModelStateResponseFactory 原本对其失活，
/// 绑定失败静默产出 null 参数 → 500 NRE；本过滤器把工厂接到全量 Admin MVC action。
/// 仅拦绑定失败；校验特性违规（[Range]/[Required]）维持历史放行语义（前端 pageSize=1000 等既定用法）。
/// </summary>
public class AdminInvalidModelStateFilterTests
{
    private static ActionExecutingContext CreateContext()
    {
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());
    }

    private static ApiBehaviorOptions CreateBehaviorOptionsWithFactory()
    {
        return new ApiBehaviorOptions
        {
            InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Where(item => item.Value?.Errors.Count > 0)
                                    .ToDictionary(item => item.Key,
                                                  item => item.Value!.Errors.Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? error.Exception?.Message : error.ErrorMessage).ToArray());
                return new JsonResult(AdminResultFactory.CreateError(StatusCodes.Status400BadRequest, errors, (int)CommonErrorCode.ParameterValidationFailed))
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                };
            },
        };
    }

    private static AdminInvalidModelStateFilter CreateFilter(ApiBehaviorOptions? behaviorOptions)
    {
        return new AdminInvalidModelStateFilter(Options.Create(behaviorOptions ?? new ApiBehaviorOptions()));
    }

    [Fact]
    public void BindingFailure_ReturnsFactoryResult_With400AndValidationErrorCode()
    {
        var context = CreateContext();
        context.ModelState.TryAddModelException("number", new JsonException("The JSON value could not be converted to System.Int64."));
        var filter = CreateFilter(CreateBehaviorOptionsWithFactory());

        filter.OnActionExecuting(context);

        var result = Assert.IsType<JsonResult>(context.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        var envelope = Assert.IsType<HttpJsonResultData<object>>(result.Value);
        Assert.Equal((int)CommonErrorCode.ParameterValidationFailed, envelope.Code);
    }

    [Fact]
    public void PureValidationViolation_DoesNotShortCircuit()
    {
        // 历史管线从不消费动态控制器的 ModelState：前端 RewardItemsEditor 以 pageSize=1000 调分页
        // （超出 BasePageRequest.PageSize [Range(1,100)]）长期可用，防线不得把此类校验违规改成 400
        var context = CreateContext();
        context.ModelState.AddModelError("PageSize", "页码容量超过最大限制");
        var filter = CreateFilter(CreateBehaviorOptionsWithFactory());

        filter.OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void ValidModelState_DoesNotShortCircuit()
    {
        var context = CreateContext();
        var filter = CreateFilter(CreateBehaviorOptionsWithFactory());

        filter.OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void BindingFailure_WithoutConfiguredFactory_DoesNotThrow()
    {
        // 孤立宿主/测试未配置工厂时保持放行，不在防线自身引入 NRE
        var context = CreateContext();
        context.ModelState.TryAddModelException("number", new JsonException("binding failed"));
        var filter = CreateFilter(null);

        filter.OnActionExecuting(context);

        Assert.Null(context.Result);
    }
}
