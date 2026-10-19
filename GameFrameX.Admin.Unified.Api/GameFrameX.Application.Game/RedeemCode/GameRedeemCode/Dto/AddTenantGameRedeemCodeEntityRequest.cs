namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCode.Dto;

/// <summary>
/// 兑换码增加输入参数
/// </summary>
public class AddTenantGameRedeemCodeEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 应用Id。事实数据应用作用域，必须 &gt; 0。
    /// <para>过渡期：后台运营当前应用上下文尚未建立（前端「当前应用」选择器待统一应用上下文 change 落地），由前端手填。</para>
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 区服ID
    /// </summary>
    [Required(ErrorMessage = "区服ID不能为空")]
    public long AreaId { get; set; }

    /// <summary>
    /// 兑换码
    /// </summary>
    [Required(ErrorMessage = "兑换码不能为空")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    [Required(ErrorMessage = "描述不能为空")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// 是否激活
    /// </summary>
    [Required(ErrorMessage = "是否激活不能为空")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 可使用次数
    /// </summary>
    [Required(ErrorMessage = "可使用次数不能为空")]
    public int UsageLimit { get; set; }

    /// <summary>
    /// 已使用次数
    /// </summary>
    [Required(ErrorMessage = "已使用次数不能为空")]
    public int UsedCount { get; set; }

    /// <summary>
    /// 有效开始时间
    /// </summary>
    [Required(ErrorMessage = "有效开始时间不能为空")]
    public long StartTime { get; set; }

    /// <summary>
    /// 失效时间
    /// </summary>
    [Required(ErrorMessage = "失效时间不能为空")]
    public long EndTime { get; set; }

    /// <summary>
    /// 礼包内容
    /// </summary>
    [Required(ErrorMessage = "礼包内容不能为空")]
    public string Contents { get; set; } = null!;

    /// <summary>
    /// 兑换码渠道ID
    /// </summary>
    [Required(ErrorMessage = "兑换码渠道ID不能为空")]
    public long? ChannelId { get; set; }
}