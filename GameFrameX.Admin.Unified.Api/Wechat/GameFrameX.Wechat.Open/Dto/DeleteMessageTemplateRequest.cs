namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 删除消息模板
/// </summary>
public class DeleteMessageTemplateRequest
{
    /// <summary>
    /// 订阅模板Id
    /// </summary>
    [Required(ErrorMessage = "订阅模板Id不能为空")]
    public string TemplateId { get; set; } = string.Empty;
}