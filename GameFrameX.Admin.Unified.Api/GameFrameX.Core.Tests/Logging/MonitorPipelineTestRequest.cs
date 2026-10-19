using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Core.Tests.Logging;

/// <summary>
/// Monitor 管线回归测试（T5）请求体：Keyword 必填用于触发 [ApiController] 自动 400 校验短路场景；
/// 正常路径下作为复杂参数落入 <c>parameters[0].value</c>，供操作日志行 RequestParam 三要素断言。
/// </summary>
public sealed class MonitorPipelineTestRequest
{
    /// <summary>
    /// 查询关键字（必填：缺失时触发数据校验失败，Monitor validation 路径不落库）。
    /// </summary>
    [Required]
    public string? Keyword { get; set; }

    /// <summary>
    /// 页码。
    /// </summary>
    public int Page { get; set; } = 1;
}
