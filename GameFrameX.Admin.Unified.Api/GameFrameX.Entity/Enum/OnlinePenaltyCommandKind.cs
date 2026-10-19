namespace GameFrameX.Entity.Enum;

/// <summary>
/// Online 处罚联动命令类型（vault:C2 S1.4 踢下线/吊销 Token/禁言契约的 Admin 侧枚举）。
/// <para>严格锁定三值（X3 红线）：Kick=踢下线、Ban=封禁（吊销全部 Token 并阻断后续登录）、Mute=禁言。</para>
/// <para>新增成员必须回 vault 评审后三端同步；守护测试 OnlineSocialCommandWhitelistTests 锁定成员集。</para>
/// </summary>
[Description("Online 处罚联动命令")]
[AdminEnum]
public enum OnlinePenaltyCommandKind
{
    /// <summary>
    /// 踢下线：断开当前连接，不改变后续登录资格。无事实源对应动作，仅能经 execute 显式指定。
    /// </summary>
    [Description("踢下线")] Kick = 1,

    /// <summary>
    /// 封禁：吊销全部 Session Token 并阻断后续登录（事实源对应 PenaltyActionTypeEnum.Ban / RestrictLogin）。
    /// </summary>
    [Description("封禁")] Ban = 2,

    /// <summary>
    /// 禁言：限制发言（事实源对应 PenaltyActionTypeEnum.Mute），不阻断登录与支付。
    /// </summary>
    [Description("禁言")] Mute = 3,
}
