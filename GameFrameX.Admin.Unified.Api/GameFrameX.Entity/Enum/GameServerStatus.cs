namespace GameFrameX.Entity.Enum;

/// <summary>
/// 游戏区服状态
/// </summary>
[Description("游戏区服状态")]
[AdminEnum]
public enum GameServerStatus
{
    /// <summary>
    /// 未知
    /// </summary>
    [Description("未知")] Unknown = 0,

    /// <summary>
    /// 在线,正常
    /// </summary>
    [Description("在线")] Online = 1,

    /// <summary>
    /// 离线，不可用
    /// </summary>
    [Description("离线")] Offline = 2,

    /// <summary>
    /// 维护中
    /// </summary>
    [Description("维护中")] Maintenance = 3,

    /// <summary>
    /// 满员
    /// </summary>
    [Description("满员")] Full = 4,
}