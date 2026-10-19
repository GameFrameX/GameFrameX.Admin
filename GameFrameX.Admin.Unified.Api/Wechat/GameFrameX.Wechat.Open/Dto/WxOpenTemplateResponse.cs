namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 小程序订阅消息模板信息
/// </summary>
public class WxOpenTemplateResponse
{
    /// <summary>
    /// 模板 ID（新增模板接口返回的私有模板 ID）
    /// </summary>
    public string? PrivateTemplateId { get; set; }

    /// <summary>
    /// 模板标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 模板内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 模板类型（2 为一次性订阅，3 为长期订阅）
    /// </summary>
    public int? Type { get; set; }

    /// <summary>
    /// 模板内容示例
    /// </summary>
    public string? Example { get; set; }

    /// <summary>
    /// 模板关键词库枚举值列表
    /// </summary>
    public List<WxOpenKeywordEnumValueResponse>? KeywordEnumValueList { get; set; }
}