using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 举报事实源。管理端录入的玩家举报记录：举报人、被举报对象、类型、场景、证据、处理状态与结果处罚关联。
/// <para>本表是举报事实源；本期由 Admin 后台录入，玩家侧提交端点属后续 change。</para>
/// </summary>
[SugarTable(null, "举报事实源")]
[SugarIndex("idx_report_tenant_app", "TenantId", OrderByType.Asc, "AppId", OrderByType.Asc)]
[SugarIndex("idx_report_target", nameof(TargetType), OrderByType.Asc, nameof(TargetId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameReportEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用作用域。举报是应用级事实数据，必须大于 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 举报人 UserId。
    /// </summary>
    [SugarColumn(ColumnDescription = "举报人", DefaultValue = "0")]
    public long ReporterId { get; set; }

    /// <summary>
    /// 被举报对象类型（复用 PenaltyTargetTypeEnum）。
    /// </summary>
    [SugarColumn(ColumnDescription = "被举报对象类型", DefaultValue = "0")]
    public PenaltyTargetTypeEnum TargetType { get; set; }

    /// <summary>
    /// 被举报对象标识。
    /// </summary>
    [SugarColumn(ColumnDescription = "被举报对象标识", Length = EntityStringLength.Length256)]
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// 举报类型（Cheat / Abuse / Illegal / Spam / Other）。
    /// </summary>
    [SugarColumn(ColumnDescription = "举报类型", DefaultValue = "0")]
    public ReportTypeEnum ReportType { get; set; }

    /// <summary>
    /// 场景描述。
    /// </summary>
    [SugarColumn(ColumnDescription = "场景描述", Length = EntityStringLength.Length512, IsNullable = true)]
    public string Scene { get; set; } = null!;

    /// <summary>
    /// 证据 JSON。
    /// </summary>
    [SugarColumn(ColumnDescription = "证据JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string EvidenceJson { get; set; } = null!;

    /// <summary>
    /// 举报状态（Pending / Investigating / Confirmed / Dismissed）。默认 Pending。
    /// </summary>
    [SugarColumn(ColumnDescription = "举报状态", DefaultValue = "0")]
    public ReportStatusEnum Status { get; set; }

    /// <summary>
    /// 处理人（CurrentUserContext.UserId）。
    /// </summary>
    [SugarColumn(ColumnDescription = "处理人", IsNullable = true)]
    public long? HandlerId { get; set; }

    /// <summary>
    /// 处理结果产生的处罚 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "结果处罚Id", IsNullable = true)]
    public long? ResultPenaltyId { get; set; }
}