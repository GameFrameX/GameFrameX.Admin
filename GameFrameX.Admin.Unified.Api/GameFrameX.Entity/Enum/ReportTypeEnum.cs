namespace GameFrameX.Entity.Enum;

/// <summary>
/// 举报类型。举报事实源（TenantGameReportEntity）的分类。
/// <para>严格锁定 5 值。</para>
/// </summary>
[Description("举报类型")]
[AdminEnum]
public enum ReportTypeEnum
{
    /// <summary>
    /// 作弊 / 外挂。
    /// </summary>
    [Description("作弊外挂")] Cheat = 1,

    /// <summary>
    /// 辱骂 / 骚扰。
    /// </summary>
    [Description("辱骂骚扰")] Abuse = 2,

    /// <summary>
    /// 违规 / 违法内容。
    /// </summary>
    [Description("违规违法")] Illegal = 3,

    /// <summary>
    /// 刷屏 / 广告。
    /// </summary>
    [Description("刷屏广告")] Spam = 4,

    /// <summary>
    /// 其他。
    /// </summary>
    [Description("其他")] Other = 5,
}