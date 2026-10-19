namespace GameFrameX.Application.Game.Tenant.Report.TenantGameReport.Dto;

/// <summary>
/// 举报关联处罚输入参数。仅 Confirmed 举报可关联；不自动创建处罚（创建走 TenantGamePenaltyEntityService.AddAsync）。
/// </summary>
public class LinkPenaltyTenantGameReportEntityRequest
{
    /// <summary>
    /// 举报 Id。
    /// </summary>
    [Required(ErrorMessage = "举报Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "举报Id必须大于0")]
    public long Id { get; set; }

    /// <summary>
    /// 处理结果产生的处罚 Id。
    /// </summary>
    [Required(ErrorMessage = "处罚Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "处罚Id必须大于0")]
    public long PenaltyId { get; set; }
}