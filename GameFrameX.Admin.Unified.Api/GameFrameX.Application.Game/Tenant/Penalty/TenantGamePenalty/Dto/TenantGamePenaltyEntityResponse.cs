using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty.Dto;

/// <summary>
/// 处罚输出参数（包含全部审计字段，供后台只读展示）。
/// </summary>
public class TenantGamePenaltyEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGamePenaltyEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用作用域。处罚是应用级事实数据，必须大于 0。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 区服作用域。null = 全区服生效。
    /// </summary>
    public long? AreaId { get; set; }

    /// <summary>
    /// 处罚对象类型（Account / Role / Device / Ip）。
    /// </summary>
    public PenaltyTargetTypeEnum TargetType { get; set; }

    /// <summary>
    /// 对象标识（AccountId / RoleId / 设备标识 / IP）。
    /// </summary>
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// 处罚动作（BanAsync / RestrictLogin / Mute / RestrictTrade / RestrictMatch）。
    /// </summary>
    public PenaltyActionTypeEnum ActionType { get; set; }

    /// <summary>
    /// 处罚状态（Active / Revoked / Expired）。默认 Active。
    /// </summary>
    public PenaltyStatusEnum Status { get; set; }

    /// <summary>
    /// 处罚原因。
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 过期时间。null = 永久。
    /// </summary>
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 创建操作人（CurrentUserContext.UserId），&gt;0。
    /// </summary>
    public long OperatorId { get; set; }

    /// <summary>
    /// 撤销操作人。
    /// </summary>
    public long? RevokeOperatorId { get; set; }

    /// <summary>
    /// 撤销原因。
    /// </summary>
    public string RevokeReason { get; set; } = null!;

    /// <summary>
    /// 撤销时间。
    /// </summary>
    public long? RevokeTime { get; set; }

    /// <summary>
    /// 来源举报 Id（由举报触发时写）。
    /// </summary>
    public long? SourceReportId { get; set; }

    /// <summary>
    /// 由申诉解除时写（关联申诉 Id）。
    /// </summary>
    public long? SourceAppealId { get; set; }

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