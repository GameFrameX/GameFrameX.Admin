namespace GameFrameX.Application.Game.Home.GameHome.Dto;

/// <summary>
/// 首页在线人数时序单点（C321 / GFX-730）
/// </summary>
public class GameHomeOnlineTrendPointResponse
{
    /// <summary>
    /// 整点桶时间（Unix 秒）
    /// </summary>
    public long BucketTime { get; set; }

    /// <summary>
    /// 该小时内最后一次采样的在线人数；无采样数据时为 null
    /// </summary>
    public int? OnlineCount { get; set; }
}
