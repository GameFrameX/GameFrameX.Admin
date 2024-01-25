namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏区服参数对象
/// </summary>
public class GameAreaEntityDto
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
        
    /// <summary>
    /// 是否是默认
    /// </summary>
    public YesNoEnum IsDefault { get; set; }
        
    /// <summary>
    /// 是否启用
    /// </summary>
    public YesNoEnum IsEnable { get; set; }
        
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }
        
    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; }
        
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
        
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
        
    /// <summary>
    /// 状态
    /// </summary>
    public GameServerStatus ServerStatus { get; set; }
        
    /// <summary>
    /// 区域
    /// </summary>
    public string? Region { get; set; }
        
    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }
        
    /// <summary>
    /// 端口号
    /// </summary>
    public long? Port { get; set; }
        
    /// <summary>
    /// 最大玩家数量
    /// </summary>
    public long? Capacity { get; set; }
        
    /// <summary>
    /// 目前的玩家数量
    /// </summary>
    public long? CurrentLoad { get; set; }
        
    /// <summary>
    /// 启用的日期
    /// </summary>
    public DateTime? CreationDate { get; set; }
        
    /// <summary>
    /// 服务器最后一次维护或更新的日期
    /// </summary>
    public DateTime? LastMaintenanceDate { get; set; }
        
    /// <summary>
    /// 游戏版本号
    /// </summary>
    public string? Version { get; set; }
        
    /// <summary>
    /// 服务器的游戏模式类型
    /// </summary>
    public string? ServerType { get; set; }
        
    /// <summary>
    /// 语言
    /// </summary>
    public GameLanguage Language { get; set; }
        
    /// <summary>
    /// 时区
    /// </summary>
    public string? TimeZone { get; set; }
        
    /// <summary>
    /// 指定可以加入此服务器的玩家类型
    /// </summary>
    public string? AllowedPlayerProfile { get; set; }
        
    /// <summary>
    /// 特定服务器举行的事件或活动信息
    /// </summary>
    public string? ServerEvents { get; set; }
        
    /// <summary>
    /// 访问权限设置，控制哪些玩家可以访问服务器
    /// </summary>
    public string? AccessControls { get; set; }
        
}
