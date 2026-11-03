using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Middleware;

/// <summary>
/// 主库 Client API 风险策略生效模式解析器。
/// </summary>
public interface IGameAppRiskEnforcementModeResolver
{
    /// <summary>
    /// 解析最新启用策略的风险模式；无策略时默认拒绝。
    /// </summary>
    RiskEnforcementModeEnum Resolve();
}
