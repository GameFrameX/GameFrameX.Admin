using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.Advertisement.TenantGameAdvertisementRewardEntity.Dto;

/// <summary>
/// 广告奖励发放账本分页/导出查询输入参数（C114 / GFX-111 后端跟进）。
/// <para>过滤字段对齐前端检索表单：Provider/Status/RoleId 常用 + TransactionId/AdvertisementUnit/AreaId/时间范围 高级。</para>
/// </summary>
public class PageTenantGameAdvertisementRewardEntityRequest : BaseAreaPageRequest
{
    /// <summary>
    /// 应用Id。账本为事实数据应用作用域，分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 广告平台 Provider（等值匹配，标准化值 TopOn / GoogleAdMob / UnityAds）。
    /// </summary>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// 平台交易号（等值匹配）。
    /// </summary>
    public string TransactionId { get; set; } = null!;

    /// <summary>
    /// 角色 Id（等值匹配）。
    /// </summary>
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 用户 Id。
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// 广告位/单元（模糊匹配）。
    /// </summary>
    public string AdvertisementUnit { get; set; } = null!;

    /// <summary>
    /// 发放状态（Pending/Success/Failed）。
    /// </summary>
    public AdvertisementRewardStatusEnum? Status { get; set; }

    /// <summary>
    /// 创建时间范围（[0]=起始, [1]=结束）。
    /// </summary>
    public List<long?> CreateTimeRange { get; set; } = null!;
}
