namespace GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenaltyAppeal.Dto;

/// <summary>
/// 申诉审核输入参数。Approve=true 通过（同步撤销关联处罚）；Approve=false 驳回。
/// </summary>
public class ReviewTenantGamePenaltyAppealEntityRequest
{
    /// <summary>
    /// 申诉 Id。
    /// </summary>
    [Required(ErrorMessage = "申诉Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "申诉Id必须大于0")]
    public long Id { get; set; }

    /// <summary>
    /// 是否通过。true=审核通过（撤销关联处罚）；false=审核驳回。
    /// </summary>
    [Required(ErrorMessage = "审核结果不能为空")]
    public bool Approve { get; set; }

    /// <summary>
    /// 审核备注。
    /// </summary>
    [MaxLength(EntityStringLength.Length2048)]
    public string Remark { get; set; } = null!;
}