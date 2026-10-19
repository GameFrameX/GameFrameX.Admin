namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 开关型受控命令的方向（来源 change C344 A0 补充锁定）。
/// <para>SuspendResumePlayer（暂停/恢复）与 MatchIsolation（隔离/解除）是开关型命令，必须携带方向；
/// CancelTicket / EndAbnormalMatch 是单向动作，禁止携带方向（携带即参数校验拒绝）。
/// 方向参与幂等键——同一目标「暂停」与「恢复」是两次独立命令意图，各自幂等。</para>
/// </summary>
public enum ControlledCommandDirection
{
    /// <summary>
    /// 施加：暂停玩家 / 加入匹配隔离。
    /// </summary>
    Apply = 1,

    /// <summary>
    /// 解除：恢复玩家 / 解除匹配隔离。
    /// </summary>
    Lift = 2,
}
