
using GameFrameX.Client.Api.GameUserRace.Models;
namespace GameFrameX.Client.Api.GameUserRace.Dto;

/// <summary>
/// 比赛报名请求（原 BL 比赛 6 op 入参，形状全同；DayRace 例外无 Round）
/// </summary>
public class GameUserRaceEntryRequest
{
    /// <summary>
    /// 比赛类型（1=Coin / 2=Pk / 3=Pk1 / 4=Crash / 5=Upgrade / 6=Day）
    /// </summary>
    public int RaceType { get; set; }

    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 需排除的 userid 列表（旧协议恒空，防御性保留）
    /// </summary>
    public List<long> ExcludeUserIds { get; set; } = new();

    /// <summary>
    /// 比赛轮次（难度曲线乘数；Day 比赛无此字段）
    /// </summary>
    public long? Round { get; set; }

    /// <summary>
    /// 需要的机器人总数（客户端严格断言返回数量==此值）
    /// </summary>
    public long RobotCount { get; set; }

    /// <summary>
    /// 各难度档位的机器人数量
    /// </summary>
    public List<RaceGroupNum> GroupsNum { get; set; } = new();
}