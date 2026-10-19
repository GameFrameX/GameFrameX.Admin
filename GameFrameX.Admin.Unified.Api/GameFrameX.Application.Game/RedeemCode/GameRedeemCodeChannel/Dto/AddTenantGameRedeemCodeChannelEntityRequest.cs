namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCodeChannel.Dto;

/// <summary>
/// 兑换码渠道增加输入参数
/// </summary>
public class AddTenantGameRedeemCodeChannelEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 应用Id。事实数据应用作用域，必须 &gt; 0。
    /// <para>过渡期：后台运营当前应用上下文尚未建立，由前端手填。</para>
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// 每角色可用次数（兑换码分组角色限制）。0 = 不限制，1 = 每角色一次，N = 每角色最多 N 次。
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "每角色可用次数必须大于等于0")]
    public int RoleUsageLimit { get; set; }
}