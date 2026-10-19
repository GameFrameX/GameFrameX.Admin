using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 申诉事实源。玩家对处罚提交的申诉及审核记录：关联处罚、申诉人、理由、状态、审核人审计。
/// <para>审核通过（Approved）即同步撤销关联处罚（写 Revoke* + SourceAppealId 审计链）。</para>
/// </summary>
[SugarTable(null, "申诉事实源")]
[SugarIndex("idx_appeal_tenant_app", "TenantId", OrderByType.Asc, "AppId", OrderByType.Asc)]
[SugarIndex("idx_appeal_penalty", nameof(PenaltyId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGamePenaltyAppealEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用作用域，跟随关联处罚 AppId。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 关联处罚 Id（TenantGamePenaltyEntity.Id）。
    /// </summary>
    [SugarColumn(ColumnDescription = "关联处罚Id", DefaultValue = "0")]
    public long PenaltyId { get; set; }

    /// <summary>
    /// 申诉人 = 被处罚账号 UserId。
    /// </summary>
    [SugarColumn(ColumnDescription = "申诉人", DefaultValue = "0")]
    public long AppellantId { get; set; }

    /// <summary>
    /// 申诉理由。
    /// </summary>
    [SugarColumn(ColumnDescription = "申诉理由", Length = EntityStringLength.Length2048)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 申诉状态（Pending / Approved / Rejected）。默认 Pending。
    /// </summary>
    [SugarColumn(ColumnDescription = "申诉状态", DefaultValue = "0")]
    public AppealStatusEnum Status { get; set; }

    /// <summary>
    /// 审核人（CurrentUserContext.UserId）。
    /// </summary>
    [SugarColumn(ColumnDescription = "审核人", IsNullable = true)]
    public long? ReviewerId { get; set; }

    /// <summary>
    /// 审核备注。
    /// </summary>
    [SugarColumn(ColumnDescription = "审核备注", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string ReviewRemark { get; set; } = null!;

    /// <summary>
    /// 审核时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "审核时间", IsNullable = true)]
    public long? ReviewTime { get; set; }
}