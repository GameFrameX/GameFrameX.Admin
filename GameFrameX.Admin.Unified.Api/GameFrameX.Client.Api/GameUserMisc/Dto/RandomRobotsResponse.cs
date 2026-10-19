
using GameFrameX.Client.Api.GameUserMisc.Models;
namespace GameFrameX.Client.Api.GameUserMisc.Dto;

/// <summary>
/// 随机机器人响应（原 BLRandomRobots 出参）
/// </summary>
public class RandomRobotsResponse
{
    /// <summary>
    /// 业务结果码（0 = 成功；非 0 客户端不消费 robots）
    /// </summary>
    public long Rcode { get; set; }

    /// <summary>
    /// 机器人列表（userid/name/icon；badgeId/frameId/score 客户端本地随机生成）
    /// </summary>
    public List<GameUserRobotInfo> Robots { get; set; } = new();
}