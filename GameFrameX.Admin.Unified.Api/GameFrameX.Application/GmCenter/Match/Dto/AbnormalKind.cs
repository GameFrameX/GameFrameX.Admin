namespace GameFrameX.Application.GmCenter.Match.Dto;

/// <summary>
/// 异常 Ticket 检索类别（Admin 侧自有检索枚举，vault 无此概念）。
/// <para>定义在 Application.Platform（非 Core.Enum），不触碰 EnumContractTests SHA-256 契约锁。</para>
/// </summary>
public enum AbnormalKind
{
    /// <summary>
    /// 长时间未分配（关联 vault 等待扩展）。
    /// </summary>
    LongWaiting = 1,

    /// <summary>
    /// 重复取消（关联 vault .7 取消竞态）。
    /// </summary>
    RepeatedCancel = 2,

    /// <summary>
    /// 状态机残留孤儿 Ticket（关联 vault 重启恢复）。
    /// </summary>
    Orphan = 3,
}
