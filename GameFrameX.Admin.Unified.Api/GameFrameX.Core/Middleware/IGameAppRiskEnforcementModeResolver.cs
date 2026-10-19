using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Middleware;

/// <summary>
/// 应用风险策略生效模式解析器。把 <see cref="GameFrameX.Entity.Tenant.Game.TenantGameAppEntity"/> 的
/// <c>RiskPolicy</c> 字符串解析为结构化的 <see cref="RiskEnforcementModeEnum"/>，
/// 供 C11-D-followup 的 admission / 票据签发逻辑消费。
/// </summary>
public interface IGameAppRiskEnforcementModeResolver
{
    /// <summary>
    /// 解析指定应用的风险策略生效模式。
    /// </summary>
    /// <param name="appId">应用 Id。</param>
    /// <returns>结构化生效模式；解析规则见 <see cref="RiskEnforcementModeParser"/>。</returns>
    RiskEnforcementModeEnum Resolve(long appId);
}