namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCodeUseRecord.Dto;

/// <summary>
/// 兑换记录增加输入参数
/// </summary>
public class AddTenantGameRedeemCodeUseRecordEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}