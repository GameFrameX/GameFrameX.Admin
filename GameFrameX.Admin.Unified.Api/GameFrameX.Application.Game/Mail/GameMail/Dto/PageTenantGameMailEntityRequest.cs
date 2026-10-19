using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Mail.GameMail.Dto;

/// <summary>
/// 游戏邮件分页查询输入参数
/// </summary>
public class PageTenantGameMailEntityRequest : BaseAreaPageRequest
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 是否发全部用户
    /// </summary>
    public bool? IsAllPost { get; set; }

    /// <summary>
    /// 邮件状态
    /// </summary>
    public GameMailStatus? Status { get; set; }

    /// <summary>
    /// 邮件类型
    /// </summary>
    public GameMailType? MailType { get; set; }

    /// <summary>
    /// 应用Id。应用级邮件分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。
    /// </summary>
    public long? AppId { get; set; }
}