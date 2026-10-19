namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 增加订阅消息模板
/// </summary>
public class AddSubscribeMessageTemplateRequest
{
    /// <summary>
    /// 模板标题Id
    /// </summary>
    [Required(ErrorMessage = "模板标题Id不能为空")]
    public string TemplateTitleId { get; set; } = string.Empty;

    /// <summary>
    /// 模板关键词列表,例如 [3,5,4]
    /// </summary>
    [Required(ErrorMessage = "模板关键词列表不能为空")]
    public List<int> KeyworkIdList { get; set; } = new();

    /// <summary>
    /// 服务场景描述，15个字以内
    /// </summary>
    [Required(ErrorMessage = "服务场景描述不能为空")]
    public string SceneDescription { get; set; } = string.Empty;
}