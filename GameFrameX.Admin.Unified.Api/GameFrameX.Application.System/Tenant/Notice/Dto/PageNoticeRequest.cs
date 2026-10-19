using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.System.Tenant.Notice.Dto;

public sealed class PageNoticeRequest : BasePageRequest
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 类型（1通知 2公告）
    /// </summary>
    public NoticeTypeEnum? Type { get; set; }
}