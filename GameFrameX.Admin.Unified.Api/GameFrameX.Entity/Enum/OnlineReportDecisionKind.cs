namespace GameFrameX.Entity.Enum;

/// <summary>
/// Online 举报案件处置决策（vault:C7 / C342 处置流决策枚举）。
/// </summary>
[Description("Online 举报处置决策")]
[AdminEnum]
public enum OnlineReportDecisionKind
{
    /// <summary>
    /// 驳回：举报不成立，不写处罚事实源。
    /// </summary>
    [Description("驳回")] Dismiss = 1,

    /// <summary>
    /// 警告：不写处罚事实源，仅通知当事人。
    /// </summary>
    [Description("警告")] Warn = 2,

    /// <summary>
    /// 禁言：写处罚事实源（Mute）并联动 Online 运行时；需 DurationMinutes。
    /// </summary>
    [Description("禁言")] Mute = 3,

    /// <summary>
    /// 封禁：写处罚事实源（Ban）并联动 Online 运行时；需 DurationMinutes。
    /// </summary>
    [Description("封禁")] Ban = 4,

    /// <summary>
    /// 转人工处罚流：不自动处置，转 gamePenaltyEntity 页由人工建处罚。
    /// </summary>
    [Description("转人工处罚")] ForwardPenalty = 5,
}
