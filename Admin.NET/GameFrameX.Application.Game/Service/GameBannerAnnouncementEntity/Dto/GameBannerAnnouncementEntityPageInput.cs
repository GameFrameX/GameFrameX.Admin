using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏内横幅公告分页查询输入参数
/// </summary>
public class GameBannerAnnouncementEntityInput : BasePageInput
{
    /// <summary>
    /// 区服ID
    /// </summary>
    public long? AreaId { get; set; }
        
    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }
        
    /// <summary>
    /// 优先级
    /// </summary>
    public int? Priority { get; set; }
        
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }
        
    /// <summary>
     /// 开始时间范围
     /// </summary>
     public List<DateTime?> StartTimeRange { get; set; } 
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
        
    /// <summary>
     /// 结束时间范围
     /// </summary>
     public List<DateTime?> EndTimeRange { get; set; } 
    /// <summary>
    /// 间隔时间
    /// </summary>
    public int? Interval { get; set; }
        
    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }
        
}
