namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏邮件参数对象
/// </summary>
public class GameMailEntityDto
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }
        
    /// <summary>
    /// 区服列表
    /// </summary>
    public string AreaIdList { get; set; }
        
    /// <summary>
    /// 接收者
    /// </summary>
    public string RecipientId { get; set; }
        
    /// <summary>
    /// 发送的时间
    /// </summary>
    public DateTime SendDate { get; set; }
        
    /// <summary>
    /// 标题
    /// </summary>
    public string Subject { get; set; }
        
    /// <summary>
    /// 详细内容
    /// </summary>
    public string Body { get; set; }
        
    /// <summary>
    /// 附件列表
    /// </summary>
    public string Attachments { get; set; }
        
    /// <summary>
    /// 邮件状态
    /// </summary>
    public GameMailStatus Status { get; set; }
        
    /// <summary>
    /// 邮件的过期日期
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
        
    /// <summary>
    /// 邮件类型，例如个人邮件、系统邮件等。
    /// </summary>
    public GameMailType MailType { get; set; }
        
    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreateUserName { get; set; }
        
    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? UpdateUserName { get; set; }
        
}
