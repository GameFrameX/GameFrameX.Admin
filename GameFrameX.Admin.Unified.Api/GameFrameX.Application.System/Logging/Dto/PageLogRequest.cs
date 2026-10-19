namespace GameFrameX.Application.System.Logging.Dto;

public class PageLogRequest : BasePageRequest
{
    /// <summary>
    /// 关键字查询（不区分大小写，按各日志服务声明的 <c>SearchableTextColumns</c> 多列 OR 模糊匹配）。
    /// 与时间范围条件 <c>AND</c> 叠加；为空/null 时行为与现状完全一致（不过滤）。
    /// 继承自 <see cref="BasePageRequest"/>，此处显式列出便于契约可读。
    /// </summary>
    public new string? SearchKey { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public long? EndTime { get; set; }
}