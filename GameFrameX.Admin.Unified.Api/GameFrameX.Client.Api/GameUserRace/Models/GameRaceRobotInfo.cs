namespace GameFrameX.Client.Api.GameUserRace.Models;

/// <summary>
/// 比赛机器人条目（原 BL 比赛 RobotInfo；嵌套行结构）
/// <para>计分制（Coin/Pk/Day）填 Score 十进制字符串；计时制（Pk1/Crash/Upgrade）填 Second 秒数；两者互斥。</para>
/// </summary>
public class GameRaceRobotInfo
{
    /// <summary>
    /// 机器人 userid（真实玩家档抽样或合成）
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 展示名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 头像 id
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 组类型（难度档位标识，透传客户端）
    /// </summary>
    public string GroupType { get; set; } = string.Empty;

    /// <summary>
    /// 比赛成绩-计分制（十进制字符串；计时制为 null）
    /// </summary>
    public string? Score { get; set; }

    /// <summary>
    /// 比赛成绩-计时制（秒；计分制为 null）
    /// </summary>
    public long? Second { get; set; }

    /// <summary>
    /// 能力值（难度基准 × 轮次）
    /// </summary>
    public long Ability { get; set; }
}