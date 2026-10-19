using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.Message.Dto;

public class MessageRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户ID列表
    /// </summary>
    public List<long> UserIds { get; set; } = null!;

    /// <summary>
    /// 消息标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageTypeEnum MessageType { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Message { get; set; } = null!;
}