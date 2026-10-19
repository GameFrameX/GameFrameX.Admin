using System.ComponentModel;
using GameFrameX.Core.Logging;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.Logging;

/// <summary>
/// Monitor 管线回归测试控制器（T5）：路由模板模拟 AdminApiRouteRules 产物
/// <c>api/{controller}/{action}</c>（字面路由段如 userInfo/logout），命中
/// <see cref="DatabaseLoggingWriter"/> 三表分流与 Monitor 过滤器豁免/校验路径。
/// </summary>
[Route("api/monitorPipeline")]
[ApiController]
public sealed class MonitorPipelineTestController : ControllerBase
{
    /// <summary>
    /// 操作日志路径：复杂参数 → 操作日志行 RequestParam 断言。
    /// </summary>
    [HttpPost("page")]
    [DisplayName("Monitor管线分页")]
    public Task<object> GetPageAsync([FromBody] MonitorPipelineTestRequest request)
    {
        return Task.FromResult<object>(new { Total = 1, Keyword = request.Keyword, });
    }

    /// <summary>
    /// 访问日志路径：路由段 userInfo → 仅落 visit 表。
    /// </summary>
    [HttpPost("userInfo")]
    [DisplayName("Monitor管线用户信息")]
    public Task<object> GetUserInfoAsync()
    {
        return Task.FromResult<object>(new { UserName = "monitor-pipeline", });
    }

    /// <summary>
    /// 访问日志路径：路由段 logout → 仅落 visit 表。
    /// </summary>
    [HttpPost("logout")]
    [DisplayName("Monitor管线登出")]
    public Task LogoutAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 异常日志路径：抛错 → Error 级 + exception 表 + Exception 字段非空。
    /// </summary>
    [HttpPost("fail")]
    [DisplayName("Monitor管线失败")]
    public Task FailAsync()
    {
        throw new InvalidOperationException("monitor-pipeline-fail");
    }

    /// <summary>
    /// 豁免路径：方法级 [SuppressMonitor] → 全链路零落库。
    /// </summary>
    [HttpPost("suppressed")]
    [SuppressMonitor]
    [DisplayName("Monitor管线豁免")]
    public Task SuppressedAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 校验短路路径：Keyword 缺失 → 自动 400 + validation → 不落库。
    /// </summary>
    [HttpPost("invalid")]
    [DisplayName("Monitor管线校验失败")]
    public Task<object> InvalidAsync([FromBody] MonitorPipelineTestRequest request)
    {
        return Task.FromResult<object>(new { Keyword = request.Keyword, });
    }
}
