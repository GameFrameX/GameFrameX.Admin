namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 小程序订阅消息模板关键词枚举值
/// </summary>
public class WxOpenKeywordEnumValueResponse
{
    /// <summary>
    /// 关键词代码
    /// </summary>
    public string? KeywordCode { get; set; }

    /// <summary>
    /// 枚举值列表
    /// </summary>
    public List<string>? EnumValueList { get; set; }
}