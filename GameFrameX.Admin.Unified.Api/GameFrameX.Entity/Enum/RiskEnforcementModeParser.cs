namespace GameFrameX.Entity.Enum;

/// <summary>
/// 风险策略生效模式解析器（纯函数，无 IO，便于单元测试）。
/// 把 <c>TenantGameAppEntity.RiskPolicy</c> 字符串解析为结构化 <see cref="RiskEnforcementModeEnum"/>。
/// <para>解析优先级（默认安全）：</para>
/// <list type="number">
///   <item>字符串非空且能 <c>Enum.TryParse</c>（忽略大小写）成功 → 返回解析值。</item>
///   <item>字符串非空但解析失败 → 开了 SignatureRequired / DeviceProofRequired → FailClosed；否则 AuditOnly。</item>
///   <item>字符串为空/null → 同规则 2 回退。</item>
/// </list>
/// <para>该函数是 C11-D 验收"风险策略 fail closed / fail open 必须按应用配置明确"的核心保证。</para>
/// </summary>
public static class RiskEnforcementModeParser
{
    /// <summary>
    /// 按 §优先级解析风险策略生效模式。
    /// </summary>
    /// <param name="riskPolicy">应用配置的 RiskPolicy 字符串（可空 / 空白 / 非法）。</param>
    /// <param name="signatureRequired">是否要求请求签名（C9 字段）。</param>
    /// <param name="deviceProofRequired">是否要求设备证明（C9 字段）。</param>
    /// <returns>结构化生效模式。</returns>
    public static RiskEnforcementModeEnum Parse(string riskPolicy, bool signatureRequired, bool deviceProofRequired)
    {
        if (!string.IsNullOrWhiteSpace(riskPolicy))
        {
            if (global::System.Enum.TryParse(riskPolicy.Trim(), true, out RiskEnforcementModeEnum parsed))
            {
                return parsed;
            }
        }

        return signatureRequired || deviceProofRequired
                   ? RiskEnforcementModeEnum.FailClosed
                   : RiskEnforcementModeEnum.AuditOnly;
    }
}