namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 匹配队列总览查询输入参数（change C341 §3；AppId/ServerId 为 C339 作用域三元组硬前置）。
/// </summary>
public class OnlineMatchOverviewRequest
{
    /// <summary>
    /// 应用作用域（必填，经 OnlineScopeContextFactory 校验租户归属）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id必须大于0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服作用域（必填，经 OnlineScopeContextFactory 校验租户与应用归属）。
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
