namespace GameFrameX.Client.Api.GameUserMisc.Dto;

/// <summary>
/// 随机机器人请求（原 BLRandomRobots 入参；直线赛/组队赛报名用）
/// </summary>
public class RandomRobotsRequest
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 需排除的 userid 列表（已邀请/已有队友/建议列表）
    /// </summary>
    public List<long> ExcludeUserIds { get; set; } = new();

    /// <summary>
    /// 需要的机器人数量（客户端严格断言返回数量==请求数量，不足宁可凑满）
    /// </summary>
    public long RobotCount { get; set; }
}