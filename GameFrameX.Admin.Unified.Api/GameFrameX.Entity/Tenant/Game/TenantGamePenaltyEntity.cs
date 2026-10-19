using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 处罚事实源。记录账号 / 角色 / 设备 / IP 维度的封禁 / 限制登录 / 禁言 / 限制交易 / 限制匹配处罚生命周期：
/// 对象、动作、状态、原因、有效期、来源（举报 / 申诉）、操作人与撤销审计。
/// <para>应用 / 区服作为作用域字段（AppId / AreaId），不引入"封禁整个应用"对象类型。AppId 必须大于 0。</para>
/// <para>登录阻断铁律：Enforcement 命中查询仅命中 ActionType ∈ {Ban, RestrictLogin}；Mute / RestrictTrade / RestrictMatch 永不阻断登录。</para>
/// </summary>
[SugarTable(null, "处罚事实源")]
[SugarIndex("idx_penalty_tenant_app", "TenantId", OrderByType.Asc, "AppId", OrderByType.Asc)]
[SugarIndex("idx_penalty_target", nameof(TargetType), OrderByType.Asc, nameof(TargetId), OrderByType.Asc, nameof(Status), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGamePenaltyEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用作用域。处罚是应用级事实数据，必须大于 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服作用域。null = 全区服生效。
    /// </summary>
    [SugarColumn(ColumnDescription = "区服Id", IsNullable = true)]
    public long? AreaId { get; set; }

    /// <summary>
    /// 处罚对象类型（Account / Role / Device / Ip）。
    /// </summary>
    [SugarColumn(ColumnDescription = "处罚对象类型", DefaultValue = "0")]
    public PenaltyTargetTypeEnum TargetType { get; set; }

    /// <summary>
    /// 对象标识（AccountId / RoleId / 设备标识 / IP）。
    /// </summary>
    [SugarColumn(ColumnDescription = "对象标识", Length = EntityStringLength.Length256)]
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// 处罚动作（Ban / RestrictLogin / Mute / RestrictTrade / RestrictMatch）。
    /// </summary>
    [SugarColumn(ColumnDescription = "处罚动作", DefaultValue = "0")]
    public PenaltyActionTypeEnum ActionType { get; set; }

    /// <summary>
    /// 处罚状态（Active / Revoked / Expired）。默认 Active。
    /// </summary>
    [SugarColumn(ColumnDescription = "处罚状态", DefaultValue = "0")]
    public PenaltyStatusEnum Status { get; set; }

    /// <summary>
    /// 处罚原因。
    /// </summary>
    [SugarColumn(ColumnDescription = "处罚原因", Length = EntityStringLength.Length2048)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 过期时间。null = 永久。
    /// </summary>
    [SugarColumn(ColumnDescription = "过期时间", IsNullable = true)]
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 创建操作人（CurrentUserContext.UserId），&gt;0。
    /// </summary>
    [SugarColumn(ColumnDescription = "创建操作人", DefaultValue = "0")]
    public long OperatorId { get; set; }

    /// <summary>
    /// 撤销操作人。
    /// </summary>
    [SugarColumn(ColumnDescription = "撤销操作人", IsNullable = true)]
    public long? RevokeOperatorId { get; set; }

    /// <summary>
    /// 撤销原因。
    /// </summary>
    [SugarColumn(ColumnDescription = "撤销原因", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string RevokeReason { get; set; } = null!;

    /// <summary>
    /// 撤销时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "撤销时间", IsNullable = true)]
    public long? RevokeTime { get; set; }

    /// <summary>
    /// 来源举报 Id（由举报触发时写）。
    /// </summary>
    [SugarColumn(ColumnDescription = "来源举报Id", IsNullable = true)]
    public long? SourceReportId { get; set; }

    /// <summary>
    /// 由申诉解除时写（关联申诉 Id）。
    /// </summary>
    [SugarColumn(ColumnDescription = "来源申诉Id", IsNullable = true)]
    public long? SourceAppealId { get; set; }
}