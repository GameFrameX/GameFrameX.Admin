
using GameFrameX.Client.Api.GameUserRace.Models;
namespace GameFrameX.Client.Api.GameUserRace.Dto;

/// <summary>
/// 比赛报名响应（原 BL 比赛 6 op 出参：即时生成的机器人列表，不落库）
/// </summary>
public class GameUserRaceEntryResponse
{
    /// <summary>
    /// 业务结果码（0 = 成功）
    /// </summary>
    public long Rcode { get; set; }

    /// <summary>
    /// 机器人列表（数量严格 == 请求 RobotCount）
    /// </summary>
    public List<GameRaceRobotInfo> Robots { get; set; } = new();
}