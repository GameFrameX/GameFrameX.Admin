using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Penalty.GamePenalty;

/// <summary>
/// 处罚事实源登录阻断查询服务。Client API 登录鉴权的唯一封禁判定入口。
/// <para>铁律：仅命中 ActionType ∈ {BanAsync, RestrictLogin} 的 Active + 未过期处罚；Mute / RestrictTrade / RestrictMatch 永不命中。</para>
/// </summary>
public interface ITenantGamePenaltyEnforcementService
{
    /// <summary>
    /// 判断指定对象在当前应用 / 区服下是否存在生效中的登录阻断处罚（ActionType ∈ {BanAsync, RestrictLogin}）。
    /// </summary>
    /// <param name="targetType">处罚对象类型（Account / Role / Device / Ip）。</param>
    /// <param name="targetId">对象标识（AccountId / RoleId / 设备标识 / IP）。</param>
    /// <param name="appId">当前应用 Id；处罚 AppId=0（全应用）也会命中。</param>
    /// <param name="areaId">当前区服 Id；null 时仅命中 AreaId 为空的处罚。</param>
    /// <returns>命中返回处罚实体（含原因），未命中返回 null。</returns>
    Task<Entity.Tenant.Game.TenantGamePenaltyEntity> GetLoginBlockingPenaltyAsync(
        PenaltyTargetTypeEnum targetType, string targetId, long appId, long? areaId = null);
}