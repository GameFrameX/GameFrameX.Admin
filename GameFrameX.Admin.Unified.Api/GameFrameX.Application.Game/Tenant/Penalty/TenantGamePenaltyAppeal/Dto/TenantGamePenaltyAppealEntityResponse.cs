using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenaltyAppeal.Dto;

/// <summary>
/// 申诉输出参数（含审核审计字段，供后台只读展示）。
/// </summary>
public class TenantGamePenaltyAppealEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGamePenaltyAppealEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用作用域，跟随关联处罚 AppId。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 关联处罚 Id（TenantGamePenaltyEntity.Id）。
    /// </summary>
    public long PenaltyId { get; set; }

    /// <summary>
    /// 申诉人 = 被处罚账号 UserId。
    /// </summary>
    public long AppellantId { get; set; }

    /// <summary>
    /// 申诉理由。
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 申诉状态（Pending / Approved / Rejected）。默认 Pending。
    /// </summary>
    public AppealStatusEnum Status { get; set; }

    /// <summary>
    /// 审核人（CurrentUserContext.UserId）。
    /// </summary>
    public long? ReviewerId { get; set; }

    /// <summary>
    /// 审核备注。
    /// </summary>
    public string ReviewRemark { get; set; } = null!;

    /// <summary>
    /// 审核时间。
    /// </summary>
    public long? ReviewTime { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

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