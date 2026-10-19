using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Mail.GameMail.Dto;

/// <summary>
/// 游戏邮件输出参数
/// </summary>
public class TenantGameMailEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameMailEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 区服ID
    /// </summary>
    public long AreaId { get; set; }

    /// <summary>
    /// 接收者的唯一标识符。
    /// </summary>
    public string PlayerIdListJson { get; set; } = null!;

    /// <summary>
    /// 是否发全部用户。
    /// </summary>
    public bool IsAllPost { get; set; } = true;

    /// <summary>
    /// 邮件的标题或主题。
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 副标题。
    /// </summary>
    public string SubTitle { get; set; } = null!;

    /// <summary>
    /// 邮件的详细信息内容。
    /// </summary>
    public string Body { get; set; } = null!;

    /// <summary>
    /// 附件列表
    /// </summary>
    public string AttachmentsJson { get; set; } = null!;

    /// <summary>
    /// 邮件状态，包括已读、未读、删除等件。
    /// </summary>
    public GameMailStatus Status { get; set; }

    /// <summary>
    /// 邮件的过期日期。
    /// </summary>
    public long ExpirationTime { get; set; }

    /// <summary>
    /// 邮件类型。
    /// </summary>
    public GameMailType MailType { get; set; }

    /// <summary>
    /// 邮件是否本地化
    /// </summary>
    public bool? IsTranslate { get; set; }

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