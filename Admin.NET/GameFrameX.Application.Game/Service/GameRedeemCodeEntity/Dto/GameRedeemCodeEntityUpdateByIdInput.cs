using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game;

/// <summary>
/// 兑换码更新输入参数
/// </summary>
public class UpdateGameRedeemCodeEntityInput : BaseUpdateInput
{
    /// <summary>
    /// 是否激活
    /// </summary>
    [Required(ErrorMessage = "是否激活不能为空")]
    public bool IsActive { get; set; }
        
    /// <summary>
    /// 有效开始时间
    /// </summary>
    [Required(ErrorMessage = "有效开始时间不能为空")]
    public DateTime StartTime { get; set; }
        
    /// <summary>
    /// 失效时间
    /// </summary>
    [Required(ErrorMessage = "失效时间不能为空")]
    public DateTime EndTime { get; set; }
        
}

