using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.System.Tenant.Notice.Dto;

/// <summary>
/// 系统通知公告用户输出参数
/// </summary>
public class TenantNoticeUserResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 通知公告Id
    /// </summary>
    public long NoticeId { get; set; }

    /// <summary>
    /// 通知公告
    /// </summary>
    public TenantNoticeResponse TenantNoticeEntity { get; set; } = null!;

    /// <summary>
    /// 用户Id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 阅读时间
    /// </summary>
    public long? ReadTime { get; set; }

    /// <summary>
    /// 状态（0未读 1已读）
    /// </summary>
    public NoticeUserStatusEnum ReadStatus { get; set; } = NoticeUserStatusEnum.UNREAD;

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