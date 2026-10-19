namespace GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty.Dto;

/// <summary>
/// 撤销处罚输入参数。
/// </summary>
public class RevokeTenantGamePenaltyEntityRequest
{
    /// <summary>
    /// 处罚 Id。
    /// </summary>
    [Required(ErrorMessage = "处罚Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "处罚Id必须大于0")]
    public long Id { get; set; }

    /// <summary>
    /// 撤销原因。
    /// </summary>
    [Required(ErrorMessage = "撤销原因不能为空")]
    [MaxLength(EntityStringLength.Length2048)]
    public string Reason { get; set; } = null!;
}