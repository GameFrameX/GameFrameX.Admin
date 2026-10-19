using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.Report.TenantGameReport.Dto;

/// <summary>
/// 举报输出参数（含处理人 / 结果处罚关联，供后台只读展示）。
/// </summary>
public class TenantGameReportEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameReportEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用作用域。举报是应用级事实数据，必须大于 0。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 举报人 UserId。
    /// </summary>
    public long ReporterId { get; set; }

    /// <summary>
    /// 被举报对象类型（复用 PenaltyTargetTypeEnum）。
    /// </summary>
    public PenaltyTargetTypeEnum TargetType { get; set; }

    /// <summary>
    /// 被举报对象标识。
    /// </summary>
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// 举报类型（Cheat / Abuse / Illegal / Spam / Other）。
    /// </summary>
    public ReportTypeEnum ReportType { get; set; }

    /// <summary>
    /// 场景描述。
    /// </summary>
    public string Scene { get; set; } = null!;

    /// <summary>
    /// 证据 JSON。
    /// </summary>
    public string EvidenceJson { get; set; } = null!;

    /// <summary>
    /// 举报状态（Pending / Investigating / Confirmed / Dismissed）。默认 Pending。
    /// </summary>
    public ReportStatusEnum Status { get; set; }

    /// <summary>
    /// 处理人（CurrentUserContext.UserId）。
    /// </summary>
    public long? HandlerId { get; set; }

    /// <summary>
    /// 处理结果产生的处罚 Id。
    /// </summary>
    public long? ResultPenaltyId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}