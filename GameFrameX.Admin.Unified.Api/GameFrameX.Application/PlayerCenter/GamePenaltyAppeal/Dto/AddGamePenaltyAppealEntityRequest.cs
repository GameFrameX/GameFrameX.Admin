namespace GameFrameX.Application.PlayerCenter.GamePenaltyAppeal.Dto;

/// <summary>
/// 申诉新增输入参数。关联处罚由服务端验证。
/// </summary>
public class AddGamePenaltyAppealEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 关联处罚 Id（必须为生效中处罚）。
    /// </summary>
    [Required(ErrorMessage = "关联处罚Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "关联处罚Id必须大于0")]
    public long PenaltyId { get; set; }

    /// <summary>
    /// 申诉人 = 被处罚账号 UserId。
    /// </summary>
    [Required(ErrorMessage = "申诉人不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "申诉人必须大于0")]
    public long AppellantId { get; set; }

    /// <summary>
    /// 申诉理由。
    /// </summary>
    [Required(ErrorMessage = "申诉理由不能为空")]
    [MaxLength(EntityStringLength.Length2048)]
    public string Reason { get; set; } = null!;
}