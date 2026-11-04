namespace GameFrameX.Application.CommerceCenter.GameRedeemCode.Dto;

/// <summary>
/// 兑换码更新输入参数
/// </summary>
public class UpdateTenantGameRedeemCodeEntityRequest : BaseUpdateRequest
{

    /// <summary>
    /// 描述
    /// </summary>
    [Required(ErrorMessage = "描述不能为空")]
    public string Description { get; set; } = null!;
}