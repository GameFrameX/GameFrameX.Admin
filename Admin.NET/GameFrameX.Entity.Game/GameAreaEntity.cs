// 麻省理工学院许可证
// 
// 版权所有 (c) 2021-2023 zuohuaijun，大名科技（天津）有限公司  联系电话/微信：18020030720  QQ：515096995
// 
// 特此免费授予获得本软件的任何人以处理本软件的权利，但须遵守以下条件：在所有副本或重要部分的软件中必须包括上述版权声明和本许可声明。
// 
// 软件按“原样”提供，不提供任何形式的明示或暗示的保证，包括但不限于对适销性、适用性和非侵权的保证。
// 在任何情况下，作者或版权持有人均不对任何索赔、损害或其他责任负责，无论是因合同、侵权或其他方式引起的，与软件或其使用或其他交易有关。

using GameFrameX.Core.Enum;

namespace GameFrameX.Entity.Game;

/// <summary>
/// 游戏区服
/// </summary>
[SugarTable(null, "游戏区服")]
[SysTable]
[IncreTable]
public class GameAreaEntity : EntityBaseSelect
{
    /// <summary>
    /// 是否是默认
    /// </summary>
    [SugarColumn(ColumnDescription = "是否是默认")]
    public YesNoEnum IsDefault { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [SugarColumn(ColumnDescription = "是否启用")]
    public YesNoEnum IsEnable { get; set; }

    /// <summary>
    /// 表示服务器当前的状态。例如：Online, Offline, Maintenance, Full。
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public GameServerStatus ServerStatus { get; set; }

    /// <summary>
    /// 服务器所在地理位置或服务的区域。
    /// </summary>
    [SugarColumn(ColumnDescription = "区域")]
    public string Region { get; set; }

    /// <summary>
    /// 服务器的Internet协议地址。
    /// </summary>
    [SugarColumn(ColumnDescription = "地址")]
    public string IPAddress { get; set; }

    /// <summary>
    /// 服务器监听的网络端口号。
    /// </summary>
    [SugarColumn(ColumnDescription = "端口号")]
    public int Port { get; set; }

    /// <summary>
    /// 服务器能够承载的最大玩家数量。
    /// </summary>
    [SugarColumn(ColumnDescription = "最大玩家数量")]
    public int Capacity { get; set; }

    /// <summary>
    /// 目前服务器上的玩家数量。
    /// </summary>
    [SugarColumn(ColumnDescription = "目前的玩家数量")]
    public int CurrentLoad { get; set; }

    /// <summary>
    /// 服务器启用的日期。
    /// </summary>
    [SugarColumn(ColumnDescription = "启用的日期")]
    public DateTime CreationDate { get; set; }

    /// <summary>
    /// 服务器最后一次维护或更新的日期。
    /// </summary>
    [SugarColumn(ColumnDescription = "服务器最后一次维护或更新的日期")]
    public DateTime LastMaintenanceDate { get; set; }

    /// <summary>
    /// 服务器运行的游戏版本号。
    /// </summary>
    [SugarColumn(ColumnDescription = "游戏版本号")]
    public string Version { get; set; }

    /// <summary>
    /// 服务器的游戏模式类型，如：Normal, Newbie, Elite, PvP, PvE。
    /// </summary>
    [SugarColumn(ColumnDescription = "服务器的游戏模式类型")]
    public string ServerType { get; set; }

    /// <summary>
    /// 服务器支持的主要语言。
    /// </summary>
    [SugarColumn(ColumnDescription = "语言")]
    public string Language { get; set; }

    /// <summary>
    /// 服务器所在的时区。
    /// </summary>
    [SugarColumn(ColumnDescription = "时区")]
    public TimeZoneInfo TimeZone { get; set; }

    /// <summary>
    /// 指定可以加入此服务器的玩家类型，如：NewPlayer, VIP。
    /// </summary>
    [SugarColumn(ColumnDescription = "指定可以加入此服务器的玩家类型")]
    public string AllowedPlayerProfile { get; set; }

    /// <summary>
    /// 特定服务器举行的事件或活动信息。
    /// </summary>
    [SugarColumn(ColumnDescription = "特定服务器举行的事件或活动信息")]
    public string ServerEvents { get; set; }

    /// <summary>
    /// 访问权限设置，控制哪些玩家可以访问服务器。
    /// </summary>
    [SugarColumn(ColumnDescription = "访问权限设置，控制哪些玩家可以访问服务器")]
    public string AccessControls { get; set; }
}