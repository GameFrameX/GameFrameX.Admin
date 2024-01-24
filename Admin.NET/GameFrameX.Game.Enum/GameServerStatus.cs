using System.ComponentModel;

namespace GameFrameX.Game.Enum;

/// <summary>
/// 游戏区服状态
/// </summary>
[Description("游戏区服状态")]
public enum GameServerStatus
{
    /// <summary>
    /// 在线
    /// </summary>
    [Description("在线")] Online,

    /// <summary>
    /// 离线
    /// </summary>
    [Description("离线")] Offline,

    /// <summary>
    /// 维护中
    /// </summary>
    [Description("维护中")] Maintenance,

    /// <summary>
    /// 满员
    /// </summary>
    [Description("封禁")] Full,
}