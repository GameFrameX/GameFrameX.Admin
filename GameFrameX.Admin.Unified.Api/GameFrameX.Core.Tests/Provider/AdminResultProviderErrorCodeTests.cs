using System.Text.Json;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Filter;
using GameFrameX.Core.Provider;
using GameFrameX.Foundation.Http.Normalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace GameFrameX.Core.Tests.Provider;

/// <summary>
/// Admin 统一错误响应数字码契约最小测试（C41 立项；C265 / GFX-671 切换统一数字错误码出站）。
/// </summary>
public class AdminResultProviderErrorCodeTests
{
    [Fact]
    public void BusinessException_EnumConstructor_ExposesNumericCode()
    {
        var exception = BusinessError.Oh(SystemUserErrorCode.NotLoggedIn).StatusCode(401);

        Assert.Equal(401, exception.HttpStatusCode);
        Assert.Equal(30000006, exception.Code);
        Assert.Equal(SystemUserErrorCode.NotLoggedIn, exception.ErrorCode);
    }

    [Fact]
    public void BusinessException_TextConstructor_CodeIsZero()
    {
        var exception = BusinessError.Oh("纯文本错误");

        Assert.Equal(400, exception.HttpStatusCode);
        Assert.Equal(0, exception.Code);
        Assert.Null(exception.ErrorCode);
        Assert.Equal("纯文本错误", exception.Message);
    }

    [Fact]
    public void AdminResultFactory_CreateError_NumericCodeOverridesStatusCodeInEnvelope()
    {
        var result = AdminResultFactory.CreateError(400, "记录不存在", (int)CommonErrorCode.RecordNotFound);

        Assert.Equal(20000003, result.Code);
        Assert.Equal("记录不存在", result.Message);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public void AdminResultFactory_CreateError_WithoutCode_FallsBackToStatusCode()
    {
        var result = AdminResultFactory.CreateError(401, "登录已过期");

        Assert.Equal(401, result.Code);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public void CreateStatusCodeResult_CarriesNumericCode()
    {
        var result = AdminResultFilter.CreateStatusCodeResult(401, "401 登录已过期，请重新登录", (int)AuthErrorCode.TokenExpired);

        Assert.Equal(20100003, result.Code);
        Assert.Equal("401 登录已过期，请重新登录", result.Message);
    }

    [Fact]
    public void HttpJsonResultData_ErrorCode_SerializesAsCamelCaseField()
    {
        var result = new HttpJsonResultData<object>
        {
            Code = 409,
            Message = "版本冲突",
            ErrorCode = "CONCURRENCY.CONFLICT",
            Data = null!,
        };

        var json = JsonSerializer.Serialize(result);

        Assert.Contains("\"errorCode\":\"CONCURRENCY.CONFLICT\"", json);
        Assert.Contains("\"code\":409", json);

        var roundTripped = JsonSerializer.Deserialize<HttpJsonResultData<object>>(json);
        Assert.NotNull(roundTripped);
        Assert.Equal("CONCURRENCY.CONFLICT", roundTripped!.ErrorCode);
        Assert.Equal(409, roundTripped.Code);
    }

    [Fact]
    public void AdminResultFactory_Create_ProducesSuccessEnvelopeWithZeroCode()
    {
        var result = AdminResultFactory.Create(new SampleResponse { Name = "ok", });

        Assert.Equal(0, result.Code);
        Assert.Equal(string.Empty, result.Message);
        Assert.IsType<SampleResponse>(result.Data);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WrappedObjectResult_UpdatesDeclaredType()
    {
        var filter = new AdminResultFilter();
        var objectResult = new ObjectResult(new SampleResponse { Name = "ok", })
        {
            DeclaredType = typeof(SampleResponse),
        };
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();
        var executingContext = new ActionExecutingContext(actionContext, filters, new Dictionary<string, object?>(), new object());
        ActionExecutionDelegate next = () => Task.FromResult(new ActionExecutedContext(actionContext, filters, new object())
        {
            Result = objectResult,
        });

        await filter.OnActionExecutionAsync(executingContext, next);

        var value = Assert.IsType<HttpJsonResultData<object>>(objectResult.Value);
        Assert.Equal(typeof(HttpJsonResultData<object>), objectResult.DeclaredType);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        Assert.Equal(0, value.Code);
        Assert.IsType<SampleResponse>(value.Data);
    }

    /// <summary>
    /// action 已显式返回统一信封时必须透传，防止双重包装（C223 双重信封根因的回归防护）。
    /// </summary>
    [Fact]
    public async Task OnActionExecutionAsync_ExplicitHttpJsonResult_PassesThroughWithoutWrapping()
    {
        var filter = new AdminResultFilter();
        var envelope = HttpJsonResultData<SampleResponse>.Success(new SampleResponse { Name = "ok", });
        var objectResult = new ObjectResult(envelope)
        {
            DeclaredType = typeof(HttpJsonResultData<SampleResponse>),
        };
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();
        var executingContext = new ActionExecutingContext(actionContext, filters, new Dictionary<string, object?>(), new object());
        ActionExecutionDelegate next = () => Task.FromResult(new ActionExecutedContext(actionContext, filters, new object())
        {
            Result = objectResult,
        });

        await filter.OnActionExecutionAsync(executingContext, next);

        Assert.Same(envelope, objectResult.Value);
        Assert.Equal(typeof(HttpJsonResultData<SampleResponse>), objectResult.DeclaredType);
    }

    private sealed class SampleResponse
    {
        public string Name { get; set; } = string.Empty;
    }
}