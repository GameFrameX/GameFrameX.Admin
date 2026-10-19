namespace GameFrameX.Application.Game.Home.GameHome.Dto;

/// <summary>
/// 全服在线人数结果
/// </summary>
public class GameHomeOnlineResponse
{
    /// <summary>
    /// 全服在线人数（启用区服在线人数之和，区服不可达或未配置管理地址时按 0 计）
    /// </summary>
    public int OnlineCount { get; set; }
}