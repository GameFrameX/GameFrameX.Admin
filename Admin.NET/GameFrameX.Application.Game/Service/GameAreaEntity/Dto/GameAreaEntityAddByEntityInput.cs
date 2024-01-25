using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏区服增加输入参数
/// </summary>
public class AddGameAreaEntityInput : BaseAddInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
        
    /// <summary>
    /// 是否是默认
    /// </summary>
    [Required(ErrorMessage = "是否是默认不能为空")]
    public YesNoEnum IsDefault { get; set; }
        
    /// <summary>
    /// 是否启用
    /// </summary>
    [Required(ErrorMessage = "是否启用不能为空")]
    public YesNoEnum IsEnable { get; set; }
        
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; }
        
    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; }
        
    /// <summary>
    /// 状态
    /// </summary>
    [Required(ErrorMessage = "状态不能为空")]
    public GameServerStatus ServerStatus { get; set; }
        
    /// <summary>
    /// 区域
    /// </summary>
    [Required(ErrorMessage = "区域不能为空")]
    public string? Region { get; set; }
        
    /// <summary>
    /// 地址
    /// </summary>
    [Required(ErrorMessage = "地址不能为空")]
    public string? Address { get; set; }
        
    /// <summary>
    /// 端口号
    /// </summary>
    [Required(ErrorMessage = "端口号不能为空")]
    public long? Port { get; set; }
        
    /// <summary>
    /// 最大玩家数量
    /// </summary>
    [Required(ErrorMessage = "最大玩家数量不能为空")]
    public long? Capacity { get; set; }
        
    /// <summary>
    /// 启用的日期
    /// </summary>
    [Required(ErrorMessage = "启用的日期不能为空")]
    public DateTime? CreationDate { get; set; }
        
    /// <summary>
    /// 服务器最后一次维护或更新的日期
    /// </summary>
    [Required(ErrorMessage = "服务器最后一次维护或更新的日期不能为空")]
    public DateTime? LastMaintenanceDate { get; set; }
        
    /// <summary>
    /// 游戏版本号
    /// </summary>
    [Required(ErrorMessage = "游戏版本号不能为空")]
    public string? Version { get; set; }
        
    /// <summary>
    /// 服务器的游戏模式类型
    /// </summary>
    [Required(ErrorMessage = "服务器的游戏模式类型不能为空")]
    public string? ServerType { get; set; }
        
    /// <summary>
    /// 语言
    /// </summary>
    [Required(ErrorMessage = "语言不能为空")]
    public GameLanguage Language { get; set; }
        
    /// <summary>
    /// 时区
    /// </summary>
    [Required(ErrorMessage = "时区不能为空")]
    public string? TimeZone { get; set; }
        
    /// <summary>
    /// 指定可以加入此服务器的玩家类型
    /// </summary>
    [Required(ErrorMessage = "指定可以加入此服务器的玩家类型不能为空")]
    public string? AllowedPlayerProfile { get; set; }
        
    /// <summary>
    /// 特定服务器举行的事件或活动信息
    /// </summary>
    [Required(ErrorMessage = "特定服务器举行的事件或活动信息不能为空")]
    public string? ServerEvents { get; set; }
        
    /// <summary>
    /// 访问权限设置，控制哪些玩家可以访问服务器
    /// </summary>
    [Required(ErrorMessage = "访问权限设置，控制哪些玩家可以访问服务器不能为空")]
    public string? AccessControls { get; set; }
        
}

