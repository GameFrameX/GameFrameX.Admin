using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.Advertisement.TenantGameAdvertisementRewardEntity.Dto;

/// <summary>
/// 广告奖励发放账本输出参数（C114 / GFX-111 后端跟进）。投影暴露实体全字段：
/// Provider/TransactionId/AdvertisementUnit/RoleId/UserId/AreaId/RewardItem/RewardAmount/Status/IdempotencyKey/FailureReason/IsSandbox + 审计列。
/// </summary>
public class TenantGameAdvertisementRewardEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameAdvertisementRewardEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），跟随角色 AppId。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 广告平台 Provider（TopOn / GoogleAdMob / UnityAds）。
    /// </summary>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// 平台交易号。TopOn trans_id / AdMob transaction_id / UnityAds oid。
    /// </summary>
    public string TransactionId { get; set; } = null!;

    /// <summary>
    /// 广告位/单元。UnityAds 无则空。
    /// </summary>
    public string AdvertisementUnit { get; set; } = null!;

    /// <summary>
    /// 角色 Id。反查自 TenantGameRoleEntity。
    /// </summary>
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 用户 Id。来自平台 userId 参数。
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 区服 Id。来自角色 AreaId。
    /// </summary>
    public long AreaId { get; set; }

    /// <summary>
    /// 奖励项名。
    /// </summary>
    public string RewardItem { get; set; } = null!;

    /// <summary>
    /// 奖励数量原文（字符串保留，平台可能传小数或字符串）。
    /// </summary>
    public string RewardAmount { get; set; } = null!;

    /// <summary>
    /// 发放状态（Pending/Success/Failed）。
    /// </summary>
    public AdvertisementRewardStatusEnum Status { get; set; }

    /// <summary>
    /// 幂等键。adreward:{NormalizedProvider}:{TransactionId}。
    /// </summary>
    public string IdempotencyKey { get; set; } = null!;

    /// <summary>
    /// 失败原因（仅 Failed 状态写）。
    /// </summary>
    public string FailureReason { get; set; } = null!;

    /// <summary>
    /// 是否沙箱/测试回调。
    /// </summary>
    public bool IsSandbox { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}
