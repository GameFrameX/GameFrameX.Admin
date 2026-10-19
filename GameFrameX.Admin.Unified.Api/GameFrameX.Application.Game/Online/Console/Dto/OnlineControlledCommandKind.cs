namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// Online 控制台受控操作命令枚举（严格四值，来源 change C344 / vault:C9 S8.1）。
/// <para>
/// 只承载本 change 新增的四条运行时干预命令；踢下线/封禁/禁言与发放/撤销资产等既有命令
/// 由 C350 <c>OnlinePenaltyLinkCommandService</c>、C340 <c>OnlineAssetGrantCommandService</c> 既有端点承载
/// （复用矩阵，禁平行实现——守护测试 <c>OnlineOverviewIsolationTests</c> 锁定枚举四值与 Console 域零既有命令 token）。
/// 新增命令必须回 vault 评审并同步守护测试。
/// </para>
/// </summary>
public enum OnlineControlledCommandKind
{
    /// <summary>
    /// 取消匹配 Ticket（目标标识 = TicketId）。
    /// </summary>
    CancelTicket = 1,

    /// <summary>
    /// 结束异常 Match（目标标识 = MatchId）。
    /// </summary>
    EndAbnormalMatch = 2,

    /// <summary>
    /// 暂停/恢复玩家（目标标识 = PlayerId；方向必填）。
    /// </summary>
    SuspendResumePlayer = 3,

    /// <summary>
    /// 匹配隔离/解除隔离（目标标识 = PlayerId；方向必填）。
    /// </summary>
    MatchIsolation = 4,
}
