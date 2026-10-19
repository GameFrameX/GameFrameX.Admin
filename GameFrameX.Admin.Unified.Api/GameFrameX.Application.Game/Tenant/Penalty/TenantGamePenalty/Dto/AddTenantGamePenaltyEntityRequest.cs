using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty.Dto;

/// <summary>
/// 处罚新增输入参数。审计字段（Status / OperatorId / RevokeAsync*）由服务端强制写入，不在此暴露。
/// </summary>
public class AddTenantGamePenaltyEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 应用作用域。处罚是应用级事实数据，必须大于 0。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服作用域。null = 全区服生效。
    /// </summary>
    public long? AreaId { get; set; }

    /// <summary>
    /// 处罚对象类型（Account / Role / Device / Ip）。
    /// </summary>
    [Required(ErrorMessage = "处罚对象类型不能为空")]
    public PenaltyTargetTypeEnum TargetType { get; set; }

    /// <summary>
    /// 对象标识（AccountId / RoleId / 设备标识 / IP）。
    /// </summary>
    [Required(ErrorMessage = "对象标识不能为空")]
    [MaxLength(EntityStringLength.Length256)]
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// 处罚动作（BanAsync / RestrictLogin / Mute / RestrictTrade / RestrictMatch）。
    /// </summary>
    [Required(ErrorMessage = "处罚动作不能为空")]
    public PenaltyActionTypeEnum ActionType { get; set; }

    /// <summary>
    /// 处罚原因。
    /// </summary>
    [Required(ErrorMessage = "处罚原因不能为空")]
    [MaxLength(EntityStringLength.Length2048)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 过期时间。null = 永久。
    /// </summary>
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 来源举报 Id（由举报触发时写）。
    /// </summary>
    public long? SourceReportId { get; set; }
}