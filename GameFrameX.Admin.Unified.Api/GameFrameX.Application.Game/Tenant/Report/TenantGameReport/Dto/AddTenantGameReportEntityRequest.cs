using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.Report.TenantGameReport.Dto;

/// <summary>
/// 举报新增输入参数。Status / HandlerId 服务端强制（Status=Pending，HandlerId=null）。
/// </summary>
public class AddTenantGameReportEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 应用作用域。举报是应用级事实数据，必须大于 0。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 举报人 UserId。
    /// </summary>
    [Required(ErrorMessage = "举报人不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "举报人必须大于0")]
    public long ReporterId { get; set; }

    /// <summary>
    /// 被举报对象类型。
    /// </summary>
    [Required(ErrorMessage = "被举报对象类型不能为空")]
    public PenaltyTargetTypeEnum TargetType { get; set; }

    /// <summary>
    /// 被举报对象标识。
    /// </summary>
    [Required(ErrorMessage = "被举报对象标识不能为空")]
    [MaxLength(EntityStringLength.Length256)]
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// 举报类型。
    /// </summary>
    [Required(ErrorMessage = "举报类型不能为空")]
    public ReportTypeEnum ReportType { get; set; }

    /// <summary>
    /// 场景描述。
    /// </summary>
    [MaxLength(EntityStringLength.Length512)]
    public string Scene { get; set; } = null!;

    /// <summary>
    /// 证据 JSON。
    /// </summary>
    [Required(ErrorMessage = "证据JSON不能为空")]
    [MaxLength(EntityStringLength.Length8192)]
    public string EvidenceJson { get; set; } = null!;
}