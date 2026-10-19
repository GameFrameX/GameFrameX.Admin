using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.ServerManager.Online;

/// <summary>
/// <see cref="IOnlineServerEndpointResolver"/> 默认实现（来源 change C339 / vault:C2 S1.7）。
/// <para>内部经 <see cref="GameServerEndpointUrlBuilder.Build(TenantGameAreaEntity, GameServerEndpointKindEnum, string)"/>
/// 按 <see cref="GameServerEndpointKindEnum.OnlineAdmin"/> 能力构造：root 校验、前缀空值拒绝（同 Gm/Control 守卫）、
/// 斜杠归一化均复用既有构造器规则，不另起第二套拼接逻辑。</para>
/// </summary>
public sealed class OnlineServerEndpointResolver : IOnlineServerEndpointResolver
{
    /// <inheritdoc />
    public string Resolve(TenantGameAreaEntity area, string action)
    {
        return GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.OnlineAdmin, action);
    }
}
