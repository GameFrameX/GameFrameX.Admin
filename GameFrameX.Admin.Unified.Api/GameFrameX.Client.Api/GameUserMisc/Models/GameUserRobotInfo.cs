namespace GameFrameX.Client.Api.GameUserMisc.Models;

/// <summary>
/// 机器人条目（原 BL RobotInfo；嵌套行结构，非端点 IO——badgeId/frameId/score 客户端本地随机生成不来自服务端）
/// </summary>
public class GameUserRobotInfo
{
    /// <summary>
    /// 机器人 userid（队友数据主键/排除列表匹配）
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
}