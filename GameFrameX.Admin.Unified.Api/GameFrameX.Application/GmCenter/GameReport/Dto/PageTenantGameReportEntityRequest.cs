using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.GmCenter.GameReport.Dto;

/// <summary>
/// 举报分页查询输入参数。
/// </summary>
public class PageTenantGameReportEntityRequest : BasePageRequest
{
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