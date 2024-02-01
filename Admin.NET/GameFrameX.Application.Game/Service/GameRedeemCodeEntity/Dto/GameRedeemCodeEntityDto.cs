namespace GameFrameX.Application.Game;

/// <summary>
/// 兑换码参数对象
/// </summary>
public class GameRedeemCodeEntityDto
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
    /// 兑换码
    /// </summary>
    public string Code { get; set; }
        
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
        
    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; }
        
    /// <summary>
    /// 可使用次数
    /// </summary>
    public int UsageLimit { get; set; }
        
    /// <summary>
    /// 已使用次数
    /// </summary>
    public int UsedCount { get; set; }
        
    /// <summary>
    /// 有效开始时间
    /// </summary>
    public DateTime StartTime { get; set; }
        
    /// <summary>
    /// 失效时间
    /// </summary>
    public DateTime EndTime { get; set; }
        
    /// <summary>
    /// 礼包内容
    /// </summary>
    public string Contents { get; set; }
        
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
        
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
        
}
