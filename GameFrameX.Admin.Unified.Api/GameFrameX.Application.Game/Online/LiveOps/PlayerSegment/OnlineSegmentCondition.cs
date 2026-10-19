// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。

namespace GameFrameX.Application.Game.Online.LiveOps.PlayerSegment;

/// <summary>
/// 玩家分群单个条件项（ConditionsJson 数组元素；来源 change C343 / vault:C8 S7.5）。
/// <para><c>Field</c> / <c>Op</c> 受 <c>OnlineRolloutSemanticMap</c> 白名单校验
/// （首版字段族 activeDays / retentionDays / totalPaymentAmount；操作符 gte / lte / eq）；
/// <c>Value</c> 为字符串承载（数值语义由 Online 侧按字段族解释，Admin 不解释、不复算命中）。</para>
/// </summary>
public sealed class OnlineSegmentCondition
{
    /// <summary>
    /// 条件字段名（必须在 SegmentConditionFields 白名单内）。
    /// </summary>
    public string Field { get; set; } = null!;

    /// <summary>
    /// 条件操作符（必须在 SegmentConditionOperators 白名单内：gte / lte / eq）。
    /// </summary>
    public string Op { get; set; } = null!;

    /// <summary>
    /// 条件值（字符串承载；空值视为非法条件）。
    /// </summary>
    public string Value { get; set; } = null!;
}
