namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 公众号消息模板信息
/// </summary>
public class WechatTemplateResponse
{
    /// <summary>
    /// 模板 ID
    /// </summary>
    public string? TemplateId { get; set; }

    /// <summary>
    /// 模板标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 模板所属一级行业
    /// </summary>
    public string? PrimaryIndustry { get; set; }

    /// <summary>
    /// 模板所属二级行业
    /// </summary>
    public string? SecondaryIndustry { get; set; }

    /// <summary>
    /// 模板内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 模板内容示例
    /// </summary>
    public string? Example { get; set; }
}