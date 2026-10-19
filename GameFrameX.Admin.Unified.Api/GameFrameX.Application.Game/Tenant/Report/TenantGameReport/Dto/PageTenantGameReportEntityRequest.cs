using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.Report.TenantGameReport.Dto;

/// <summary>
/// 举报分页查询输入参数。
/// </summary>
public class PageTenantGameReportEntityRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id。应用级举报分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 被举报对象类型（可选过滤）。
    /// </summary>
    public PenaltyTargetTypeEnum? TargetType { get; set; }

    /// <summary>
    /// 被举报对象标识（可选过滤，精确匹配）。
    /// </summary>
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// 举报类型（可选过滤）。
    /// </summary>
    public ReportTypeEnum? ReportType { get; set; }

    /// <summary>
    /// 举报状态（可选过滤）。
    /// </summary>
    public ReportStatusEnum? Status { get; set; }
}