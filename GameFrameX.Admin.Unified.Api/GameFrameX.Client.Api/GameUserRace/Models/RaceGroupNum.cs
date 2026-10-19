namespace GameFrameX.Client.Api.GameUserRace.Models;

/// <summary>
/// 比赛机器人档位数量（原 BL 比赛 groups_num 数组元素）
/// </summary>
public class RaceGroupNum
{
    /// <summary>
    /// 组类型（难度档位标识，透传返回）
    /// </summary>
    public string GroupType { get; set; } = string.Empty;

    /// <summary>
    /// 该档位需要的机器人数量
    /// </summary>
    public long GroupNum { get; set; }
}