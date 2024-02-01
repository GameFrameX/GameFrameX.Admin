using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game;

/// <summary>
/// 兑换码增加输入参数
/// </summary>
public class AddGameRedeemCodeEntityInput : BaseAddInput
{
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
    public string Code { get; set; }
        
    /// <summary>
    /// 描述
    /// </summary>
    [Required(ErrorMessage = "描述不能为空")]
    public string Description { get; set; }
        
    /// <summary>
    /// 是否激活
    /// </summary>
    [Required(ErrorMessage = "是否激活不能为空")]
    public bool IsActive { get; set; }
        
    /// <summary>
    /// 已使用次数
    /// </summary>
    [Required(ErrorMessage = "已使用次数不能为空")]
    public int UsedCount { get; set; }
        
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
        
    /// <summary>
    /// 礼包内容
    /// </summary>
    [Required(ErrorMessage = "礼包内容不能为空")]
    public string Contents { get; set; }
        
}

