namespace GameFrameX.Application.GmCenter.Match.Dto;

/// <summary>
/// 匹配队列总览查询输入参数，ServerId 为调用前置条件。
/// </summary>
public class OnlineMatchOverviewRequest
{

    /// <summary>
    /// 区服作用域（必填，经 OnlineScopeContextFactory 校验区服归属）。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long ServerId { get; set; }

    /// <summary>
    /// 匹配模式过滤（null = 全部模式）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? Mode { get; set; }

    /// <summary>
    /// 地区过滤（null = 全部地区）。
    /// </summary>
    [MaxLength(EntityStringLength.Length256)]
    public string? Region { get; set; }
}
