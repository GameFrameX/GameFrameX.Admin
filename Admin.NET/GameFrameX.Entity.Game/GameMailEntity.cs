// 麻省理工学院许可证
// 
// 版权所有 (c) 2021-2023 zuohuaijun，大名科技（天津）有限公司  联系电话/微信：18020030720  QQ：515096995
// 
// 特此免费授予获得本软件的任何人以处理本软件的权利，但须遵守以下条件：在所有副本或重要部分的软件中必须包括上述版权声明和本许可声明。
// 
// 软件按“原样”提供，不提供任何形式的明示或暗示的保证，包括但不限于对适销性、适用性和非侵权的保证。
// 在任何情况下，作者或版权持有人均不对任何索赔、损害或其他责任负责，无论是因合同、侵权或其他方式引起的，与软件或其使用或其他交易有关。

namespace GameFrameX.Entity.Game;

/// <summary>
/// 游戏邮件
/// </summary>
[SugarTable(null, "游戏邮件")]
[SysTable]
[IncreTable]
public class GameMailEntity : EntityBase
{
    /// <summary>
    /// 区服Id
    /// </summary>
    [SugarColumn(ColumnDescription = "区服Id")]
    public long AreaId { get; set; }

    /// <summary>
    /// 发送者的唯一标识符。
    /// </summary>
    [SugarColumn(ColumnDescription = "发送者的唯一标识符")]
    public long SenderId { get; set; }

    /// <summary>
    /// 接收者的唯一标识符。
    /// </summary>
    [SugarColumn(ColumnDescription = "接收者的唯一标识符")]
    public long RecipientId { get; set; }

    /// <summary>
    /// 邮件发送的日期和时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件发送的日期和时间")]
    public DateTime SendDate { get; set; }

    /// <summary>
    /// 邮件的标题或主题。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件的标题或主题")]
    public string Subject { get; set; }

    /// <summary>
    /// 邮件的详细信息内容。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件的详细信息内容", Length = 4096)]
    public string Body { get; set; }

    /// <summary>
    /// 附件列表
    /// </summary>
    [SugarColumn(IsIgnore = true, ColumnDescription = "附件列表")]
    [Navigate(NavigateType.OneToMany, nameof(Id), nameof(GameMailAttachmentEntity.MailId))]
    public List<GameMailAttachmentEntity> Attachments { get; set; }

    /// <summary>
    /// 邮件状态，包括已读、未读、删除等件。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件状态")]
    public GameMailStatus Status { get; set; }

    /// <summary>
    /// 邮件的过期日期。
    /// </summary>
    [SugarColumn(IsNullable = true, ColumnDescription = "邮件的过期日期")]
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// 邮件类型，例如个人邮件、系统邮件等。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件类型，例如个人邮件、系统邮件等。")]
    public GameMailType MailType { get; set; }
}