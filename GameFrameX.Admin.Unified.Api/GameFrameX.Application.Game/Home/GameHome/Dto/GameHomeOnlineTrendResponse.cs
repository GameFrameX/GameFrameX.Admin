namespace GameFrameX.Application.Game.Home.GameHome.Dto;

/// <summary>
/// 首页 24h 在线人数时序（C321 / GFX-730）
/// <para>最近 24 个整点桶（含当前进行中桶），桶内取该小时最后一次采样值；无采样数据的桶 OnlineCount 为 null（前端降级画 —）。</para>
/// </summary>
public class GameHomeOnlineTrendResponse
{
    /// <summary>
    /// 趋势点列表（按桶时间升序，共 24 个点）
    /// </summary>
    public List<GameHomeOnlineTrendPointResponse> Points { get; set; } = new();
}
