namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 小程序订阅消息模板新增结果
/// </summary>
public class WxOpenTemplateAddResponse
{
    /// <summary>
    /// 新增模板的私有模板 ID
    /// </summary>
    public string? PrivateTemplateId { get; set; }

    /// <summary>
    /// 微信接口返回的错误码（0 表示成功）
    /// </summary>
    public int? ErrorCode { get; set; }

    /// <summary>
    /// 微信接口返回的错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
}