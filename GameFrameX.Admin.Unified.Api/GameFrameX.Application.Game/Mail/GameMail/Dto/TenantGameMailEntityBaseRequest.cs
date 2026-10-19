using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Mail.GameMail.Dto;

/// <summary>
/// 游戏邮件实体基类 - 包含 AddInput 和 UpdateInput 的公共字段
/// </summary>
public abstract class TenantGameMailEntityBaseRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用Id。事实数据应用作用域，必须 &gt; 0。
    /// <para>过渡期：后台运营当前应用上下文尚未建立（前端「当前应用」选择器待统一应用上下文 change 落地），由前端手填。</para>
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Required(ErrorMessage = "标题不能为空")]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 副标题
    /// </summary>
    [Required(ErrorMessage = "副标题不能为空")]
    public string SubTitle { get; set; } = null!;

    /// <summary>
    /// 详细内容
    /// </summary>
    [Required(ErrorMessage = "详细内容不能为空")]
    public string Body { get; set; } = null!;

    /// <summary>
    /// 玩家ID列表
    /// </summary>
    [Required(ErrorMessage = "玩家ID列表不能为空")]
    public string PlayerIdListJson { get; set; } = null!;

    /// <summary>
    /// 是否发全部用户
    /// </summary>
    [Required(ErrorMessage = "是否发全部用户不能为空")]
    public bool IsAllPost { get; set; }

    /// <summary>
    /// 是否本地化
    /// </summary>
    public bool IsTranslate { get; set; }

    /// <summary>
    /// 区服ID。此前仅 AddRequest 声明：update 提交的 areaId 被绑定丢弃，Adapt 后实体 0 值穿透
    /// IgnoreColumns(忽略 null) 不拦 0，导致编辑一次就把区服/类型/过期时间清零。
    /// </summary>
    [Required(ErrorMessage = "区服ID不能为空")]
    public string AreaId { get; set; } = null!;

    /// <summary>
    /// 附件列表
    /// </summary>
    [Required(ErrorMessage = "附件列表不能为空")]
    public string AttachmentsJson { get; set; } = null!;

    /// <summary>
    /// 邮件的过期日期
    /// </summary>
    [Required(ErrorMessage = "邮件的过期日期不能为空")]
    public long? ExpirationTime { get; set; }

    /// <summary>
    /// 邮件类型
    /// </summary>
    [Required(ErrorMessage = "邮件类型不能为空")]
    public GameMailType MailType { get; set; }
}
