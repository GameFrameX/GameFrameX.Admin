using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 系统通知公告用户表
/// </summary>
[SugarTable(null, "租户库通知公告用户表")]
[TenantTable]
[IncrementTable]
public class TenantNoticeUserEntity : EntityBase
{
    /// <summary>
    /// 通知公告Id
    /// </summary>
    [SugarColumn(ColumnDescription = "通知公告Id", DefaultValue = "0")]
    public long NoticeId { get; set; }

    /// <summary>
    /// 通知公告
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(NoticeId))]
    public TenantNoticeEntity TenantNoticeEntity { get; set; } = null!;

    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id", DefaultValue = "0")]
    public long UserId { get; set; }

    /// <summary>
    /// 阅读时间
    /// </summary>
    [SugarColumn(ColumnDescription = "阅读时间")]
    public long? ReadTime { get; set; }

    /// <summary>
    /// 状态（0未读 1已读）
    /// </summary>
    [SugarColumn(ColumnDescription = "状态（0未读 1已读）", DefaultValue = "0")]
    public NoticeUserStatusEnum ReadStatus { get; set; } = NoticeUserStatusEnum.UNREAD;
}