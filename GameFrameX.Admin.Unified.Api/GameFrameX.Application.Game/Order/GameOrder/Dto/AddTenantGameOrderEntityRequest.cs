namespace GameFrameX.Application.Game.Order.GameOrder.Dto;

/// <summary>
/// 游戏支付订单增加输入参数
/// </summary>
public class AddTenantGameOrderEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
}