using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏内横幅公告增加输入参数
/// </summary>
public class AddGameBannerAnnouncementEntityInput : BaseAddInput
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
    /// 内容
    /// </summary>
    [Required(ErrorMessage = "内容不能为空")]
    public string Content { get; set; }
        
    /// <summary>
    /// 优先级
    /// </summary>
    [Required(ErrorMessage = "优先级不能为空")]
    public int Priority { get; set; }
        
    /// <summary>
    /// 开始时间
    /// </summary>
    [Required(ErrorMessage = "开始时间不能为空")]
    public DateTime StartTime { get; set; }
        
    /// <summary>
    /// 结束时间
    /// </summary>
    [Required(ErrorMessage = "结束时间不能为空")]
    public DateTime EndTime { get; set; }
        
    /// <summary>
    /// 间隔时间
    /// </summary>
    [Required(ErrorMessage = "间隔时间不能为空")]
    public int Interval { get; set; }
        
    /// <summary>
    /// 是否激活
    /// </summary>
    [Required(ErrorMessage = "是否激活不能为空")]
    public bool IsActive { get; set; }
        
}

