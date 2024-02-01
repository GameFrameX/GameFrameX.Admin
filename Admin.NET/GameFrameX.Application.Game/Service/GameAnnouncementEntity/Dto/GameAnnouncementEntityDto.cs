namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏公告参数对象
/// </summary>
public class GameAnnouncementEntityDto
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
        
    /// <summary>
    /// 区服ID
    /// </summary>
    public long AreaId { get; set; }
        
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }
        
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }
        
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }
        
    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; }
        
    /// <summary>
    /// 优先级
    /// </summary>
    public int Priority { get; set; }
        
    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; }
        
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
        
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
        
}
