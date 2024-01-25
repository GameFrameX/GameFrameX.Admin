using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏区服分页查询输入参数
/// </summary>
public class GameAreaEntityInput : BasePageInput
{
    /// <summary>
    /// 是否是默认
    /// </summary>
    public YesNoEnum? IsDefault { get; set; }
        
    /// <summary>
    /// 是否启用
    /// </summary>
    public YesNoEnum? IsEnable { get; set; }
        
    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }
        
    /// <summary>
    /// 状态
    /// </summary>
    public GameServerStatus? ServerStatus { get; set; }
        
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
    /// 启用的日期
    /// </summary>
    public DateTime? CreationDate { get; set; }
        
    /// <summary>
     /// 启用的日期范围
     /// </summary>
     public List<DateTime?> CreationDateRange { get; set; } 
    /// <summary>
    /// 服务器最后一次维护或更新的日期
    /// </summary>
    public DateTime? LastMaintenanceDate { get; set; }
        
    /// <summary>
     /// 服务器最后一次维护或更新的日期范围
     /// </summary>
     public List<DateTime?> LastMaintenanceDateRange { get; set; } 
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
    public GameLanguage? Language { get; set; }
        
}
