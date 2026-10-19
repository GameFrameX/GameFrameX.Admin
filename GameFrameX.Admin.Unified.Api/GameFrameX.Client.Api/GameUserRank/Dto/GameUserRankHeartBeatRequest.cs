namespace GameFrameX.Client.Api.GameUserRank.Dto;

/// <summary>
/// 排行心跳请求（原 RSHeartBeat HTTP 分支入参）
/// </summary>
public class GameUserRankHeartBeatRequest
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 旧通道会话（Game.Api 无会话源，透传不校验）
    /// </summary>
    public string Session { get; set; } = string.Empty;

    /// <summary>
    /// BakeOut 榜当前分数（活动代币数）
    /// </summary>
    public long BakeoutToken { get; set; }

    /// <summary>
    /// Multi 榜当前分数
    /// </summary>
    public long MultiToken { get; set; }

    /// <summary>
    /// Weekly1 榜当前分数
    /// </summary>
    public long Weekly1Token { get; set; }

    /// <summary>
    /// Weekly2 榜当前分数
    /// </summary>
    public long Weekly2Token { get; set; }
}