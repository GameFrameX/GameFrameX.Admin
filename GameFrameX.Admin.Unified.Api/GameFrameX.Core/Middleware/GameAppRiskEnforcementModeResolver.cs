using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Business.ClientApiConfig;

namespace GameFrameX.Core.Middleware;

/// <summary>
/// 从主库最新启用的 Client API 运行策略读取签名与设备证明风险模式。
/// </summary>
public class GameAppRiskEnforcementModeResolver : IGameAppRiskEnforcementModeResolver
{
    private readonly ISqlSugarClient _database;

    public GameAppRiskEnforcementModeResolver(ISqlSugarClient database)
    {
        _database = database;
    }

    /// <inheritdoc />
    public RiskEnforcementModeEnum Resolve()
    {
        var policy = _database.Queryable<ClientApiPolicyEntity>()
                              .Where(entity => entity.IsEnabled == true)
                              .OrderByDescending(entity => entity.Id)
                              .First();
        if (policy == null)
        {
            return RiskEnforcementModeEnum.FailClosed;
        }

        return RiskEnforcementModeParser.Parse(policy.RiskPolicy, policy.SignatureRequired, policy.DeviceProofRequired);
    }
}
