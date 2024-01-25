using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏邮件增加输入参数
/// </summary>
public class AddGameMailEntityInput : BaseAddInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
        
    /// <summary>
    /// 区服列表
    /// </summary>
    [Required(ErrorMessage = "区服列表不能为空")]
    public List<long> AreaIdList { get; set; }
        
    /// <summary>
    /// 接收者
    /// </summary>
    [Required(ErrorMessage = "接收者不能为空")]
    public string RecipientId { get; set; }
        
    /// <summary>
    /// 发送的时间
    /// </summary>
    [Required(ErrorMessage = "发送的时间不能为空")]
    public DateTime SendDate { get; set; }
        
    /// <summary>
    /// 标题
    /// </summary>
    [Required(ErrorMessage = "标题不能为空")]
    public string Subject { get; set; }
        
    /// <summary>
    /// 详细内容
    /// </summary>
    [Required(ErrorMessage = "详细内容不能为空")]
    public string Body { get; set; }
        
    /// <summary>
    /// 附件列表
    /// </summary>
    [Required(ErrorMessage = "附件列表不能为空")]
    public string Attachments { get; set; }
        
    /// <summary>
    /// 邮件状态
    /// </summary>
    [Required(ErrorMessage = "邮件状态不能为空")]
    public GameMailStatus Status { get; set; }
        
    /// <summary>
    /// 邮件的过期日期
    /// </summary>
    [Required(ErrorMessage = "邮件的过期日期不能为空")]
    public DateTime? ExpirationDate { get; set; }
        
    /// <summary>
    /// 邮件类型，例如个人邮件、系统邮件等。
    /// </summary>
    [Required(ErrorMessage = "邮件类型，例如个人邮件、系统邮件等。不能为空")]
    public GameMailType MailType { get; set; }
        
}

