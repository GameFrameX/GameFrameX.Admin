using System.ComponentModel;

namespace GameFrameX.Game.Enum;

/// <summary>
/// 游戏邮件类型
/// </summary>
[Description("游戏邮件类型")]
public enum GameMailType
{
    /// <summary>
    /// 玩家之间相互发送的个人邮件。
    /// </summary>
    [Description("个人邮件")] PersonalMail = 1,

    /// <summary>
    /// 由游戏系统自动发送给玩家的邮件。
    /// </summary>
    [Description("系统邮件")] SystemMail = 2,

    /// <summary>
    /// 向所有玩家发送的公告信息。
    /// </summary>
    [Description("公告邮件")] AnnouncementMail = 3,

    /// <summary>
    /// 包含礼物或促销礼包的邮件。
    /// </summary>
    [Description("礼包邮件")] GiftMail = 4,

    /// <summary>
    /// 与游戏内任务或剧情相关的邮件。
    /// </summary>
    [Description("任务邮件")] QuestMail = 5,

    /// <summary>
    /// 游戏管理员（Game Master）发送的邮件。
    /// </summary>
    [Description("GM邮件")] GMMail = 6,

    /// <summary>
    /// 通过游戏内交易系统处理的邮件，包含交易物品和货币。
    /// </summary>
    [Description("交易邮件")] TradeMail = 7
}