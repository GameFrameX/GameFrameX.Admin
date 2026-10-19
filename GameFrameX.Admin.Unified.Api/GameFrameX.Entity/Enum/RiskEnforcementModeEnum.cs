namespace GameFrameX.Entity.Enum;

/// <summary>
/// 风险策略生效模式（解析 TenantGameAppEntity.RiskPolicy 字符串后的结构化决策）。
/// <para>严格锁定 3 值：直接对齐 C11-D 验收"风险策略 fail closed / fail open 必须按应用配置明确"。
/// 不引入 Disabled——AuditOnly 已表达"不阻断但记录"。</para>
/// <para>解析默认安全：开启 SignatureRequired / DeviceProofRequired 且未显式配置时，回退 FailClosed（见 RiskEnforcementModeParser）。</para>
/// </summary>
[Description("风险策略生效模式")]
[AdminEnum]
public enum RiskEnforcementModeEnum
{
    /// <summary>
    /// 仅审计：风险 / 校验失败只记录，不阻断。
    /// </summary>
    [Description("仅审计")] AuditOnly = 1,

    /// <summary>
    /// 失败放开：校验不可用时放行，仅记录。
    /// </summary>
    [Description("失败放开")] FailOpen = 2,

    /// <summary>
    /// 失败关闭：校验不可用时拒绝（默认安全）。
    /// </summary>
    [Description("失败关闭")] FailClosed = 3,
}