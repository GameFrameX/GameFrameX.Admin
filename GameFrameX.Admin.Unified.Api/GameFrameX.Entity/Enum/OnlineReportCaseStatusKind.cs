namespace GameFrameX.Entity.Enum;

/// <summary>
/// Online 举报案件状态（案件数据经 Online 查询 API 透传，Admin 不落镜像）。
/// <para>枚举值 mock 先行（2026-09-13 C350 A0 对齐评审），联调时以 vault:C7 详表为准回写。</para>
/// </summary>
[Description("Online 举报案件状态")]
[AdminEnum]
public enum OnlineReportCaseStatusKind
{
    /// <summary>
    /// 待处理。
    /// </summary>
    [Description("待处理")] Pending = 1,

    /// <summary>
    /// 处理中。
    /// </summary>
    [Description("处理中")] Handling = 2,

    /// <summary>
    /// 已处理（处置生效）。
    /// </summary>
    [Description("已处理")] Resolved = 3,

    /// <summary>
    /// 已驳回。
    /// </summary>
    [Description("已驳回")] Rejected = 4,
}
