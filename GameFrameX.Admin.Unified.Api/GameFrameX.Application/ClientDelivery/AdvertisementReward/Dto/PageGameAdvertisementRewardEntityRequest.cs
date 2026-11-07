using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.ClientDelivery.AdvertisementReward.Dto;

/// <summary>
/// 广告奖励发放账本分页/导出查询输入参数（后端跟进）。
/// <para>过滤字段对齐前端检索表单：Provider/Status/RoleId 常用 + TransactionId/AdvertisementUnit/AreaId/时间范围 高级。</para>
/// </summary>
public class PageGameAdvertisementRewardEntityRequest : BaseAreaPageRequest
{
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
