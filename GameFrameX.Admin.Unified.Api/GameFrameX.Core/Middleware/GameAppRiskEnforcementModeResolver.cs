using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Middleware;

/// <summary>
/// <see cref="IGameAppRiskEnforcementModeResolver"/> 默认实现。
/// 读 <see cref="GameFrameX.Entity.Tenant.Game.TenantGameAppEntity"/> 的 RiskPolicy / SignatureRequired / DeviceProofRequired，
/// 委托 <see cref="RiskEnforcementModeParser"/> 解析为 <see cref="RiskEnforcementModeEnum"/>。
/// <para>本期不缓存（每次读应用行）；缓存优化属后续 change。</para>
/// </summary>
public class GameAppRiskEnforcementModeResolver : IGameAppRiskEnforcementModeResolver
{
    private readonly SqlSugarRepository<TenantGameAppEntity> _appRepository;

    public GameAppRiskEnforcementModeResolver(SqlSugarRepository<TenantGameAppEntity> appRepository)
    {
        _appRepository = appRepository;
    }

    /// <inheritdoc />
    public RiskEnforcementModeEnum Resolve(long appId)
    {
        var app = _appRepository.GetFirst(u => u.Id == appId);
        if (app == null)
        {
            // 应用不存在按默认安全处理：未知应用视为 FailClosed，避免放行未注册应用。
            return RiskEnforcementModeEnum.FailClosed;
        }

        return RiskEnforcementModeParser.Parse(app.RiskPolicy, app.SignatureRequired, app.DeviceProofRequired);
    }
}