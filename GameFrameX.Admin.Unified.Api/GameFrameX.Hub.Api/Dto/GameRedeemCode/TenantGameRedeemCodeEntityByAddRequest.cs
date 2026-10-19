using System.ComponentModel.DataAnnotations;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Hub.Api.Dto.GameRedeemCode;

/// <summary>
///     兑换码增加输入参数
/// </summary>
public class TenantGameRedeemCodeEntityByAddRequest : BaseAddRequest
{
    /// <summary>
    /// 区服ID
    /// </summary>
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
    /// 可使用次数
    /// </summary>
    [Required(ErrorMessage = "可使用次数不能为空")]
    public int UsageLimit { get; set; }

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
    /// 兑换码渠道ID
    /// </summary>
    [Required(ErrorMessage = "兑换码渠道ID不能为空")]
    public long ChannelId { get; set; }

    /// <summary>
    /// 兑换码物品列表
    /// </summary>
    [Required(ErrorMessage = "兑换码物品列表不能为空")]
    public List<GamePropItem> Items { get; set; } = null!;
}