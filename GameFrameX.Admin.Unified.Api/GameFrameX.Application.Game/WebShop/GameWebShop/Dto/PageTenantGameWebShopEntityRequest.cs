using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.WebShop.GameWebShop.Dto;

/// <summary>
/// 游戏邮件分页查询输入参数
/// </summary>
public class PageTenantGameWebShopEntityRequest : BaseAreaPageRequest
{
    /// <summary>
    /// 应用Id（可选过滤，按当前应用筛选网页商城）
    /// </summary>
    public long? AppId { get; set; }

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
}