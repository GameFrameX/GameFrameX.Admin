namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 匹配失败原因聚合摘要行（按原因码计数，Online 原值透传）。
/// </summary>
public class OnlineMatchFailureReasonSummaryResponse
{
    /// <summary>
    /// 失败原因码（Online 原值）。
    /// </summary>
    public string ReasonCode { get; set; } = string.Empty;

    /// <summary>
    /// 时间窗内该原因的失败次数。
    /// </summary>
    public long Count { get; set; }
}
