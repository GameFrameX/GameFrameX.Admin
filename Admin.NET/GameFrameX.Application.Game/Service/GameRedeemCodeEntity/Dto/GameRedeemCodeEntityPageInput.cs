using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game;

/// <summary>
/// 兑换码分页查询输入参数
/// </summary>
public class GameRedeemCodeEntityInput : BasePageInput
{
    /// <summary>
    /// 区服ID
    /// </summary>
    public long? AreaId { get; set; }
        
    /// <summary>
    /// 兑换码
    /// </summary>
    public string? Code { get; set; }
        
    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }
        
    /// <summary>
    /// 已使用次数
    /// </summary>
    public int? UsedCount { get; set; }
        
    /// <summary>
    /// 有效开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }
        
    /// <summary>
     /// 有效开始时间范围
     /// </summary>
     public List<DateTime?> StartTimeRange { get; set; } 
    /// <summary>
    /// 失效时间
    /// </summary>
    public DateTime? EndTime { get; set; }
        
    /// <summary>
     /// 失效时间范围
     /// </summary>
     public List<DateTime?> EndTimeRange { get; set; } 
}
