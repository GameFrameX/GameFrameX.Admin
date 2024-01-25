using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game;

/// <summary>
/// 游戏邮件分页查询输入参数
/// </summary>
public class GameMailEntityInput : BasePageInput
{
    /// <summary>
    /// 发送的时间
    /// </summary>
    public DateTime? SendDate { get; set; }
        
    /// <summary>
     /// 发送的时间范围
     /// </summary>
     public List<DateTime?> SendDateRange { get; set; } 
    /// <summary>
    /// 标题
    /// </summary>
    public string? Subject { get; set; }
        
    /// <summary>
    /// 详细内容
    /// </summary>
    public string? Body { get; set; }
        
    /// <summary>
    /// 邮件状态
    /// </summary>
    public GameMailStatus? Status { get; set; }
        
    /// <summary>
    /// 邮件类型，例如个人邮件、系统邮件等。
    /// </summary>
    public GameMailType? MailType { get; set; }
        
}
