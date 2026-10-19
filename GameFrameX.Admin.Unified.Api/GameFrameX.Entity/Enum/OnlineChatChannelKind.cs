namespace GameFrameX.Entity.Enum;

/// <summary>
/// Online 聊天频道类型（vault:C7 Chat 四类频道：Direct/Party/Group/Global）。
/// </summary>
[Description("Online 聊天频道")]
[AdminEnum]
public enum OnlineChatChannelKind
{
    /// <summary>
    /// 私聊：双方关系与 Block 规则约束。
    /// </summary>
    [Description("私聊")] Direct = 1,

    /// <summary>
    /// 队伍频道：Party 成员可见。
    /// </summary>
    [Description("队伍频道")] Party = 2,

    /// <summary>
    /// 群组频道：Group 成员与角色可见。
    /// </summary>
    [Description("群组频道")] Group = 3,

    /// <summary>
    /// 世界频道：App/Server 级策略与频控。
    /// </summary>
    [Description("世界频道")] Global = 4,
}
