namespace GameFrameX.Application.Game.Online.Match.Dto;

/// <summary>
/// 异常 Ticket 检索类别（Admin 侧自有检索枚举，vault 无此概念；change C341 §3 A0）。
/// <para>定义在 Application.Game（非 Core.Enum），不触碰 EnumContractTests SHA-256 契约锁。</para>
/// </summary>
public enum AbnormalKind
{
    /// <summary>
    /// 长时间未分配（关联 vault VC-4.5 等待扩展）。
    /// </summary>
    LongWaiting = 1,

    /// <summary>
    /// 重复取消（关联 vault S4.7 取消竞态）。
    /// </summary>
    RepeatedCancel = 2,

    /// <summary>
    /// 状态机残留孤儿 Ticket（关联 vault VC-4.8 重启恢复）。
    /// </summary>
    Orphan = 3,
}
