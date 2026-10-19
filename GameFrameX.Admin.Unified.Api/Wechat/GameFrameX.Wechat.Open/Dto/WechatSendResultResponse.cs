namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 公众号模板消息发送结果
/// </summary>
public class WechatSendResultResponse
{
    /// <summary>
    /// 消息 ID（发送成功时返回的数字标识）
    /// </summary>
    public long? MessageId { get; set; }

    /// <summary>
    /// 微信接口返回的错误码（0 表示成功）
    /// </summary>
    public int? ErrorCode { get; set; }

    /// <summary>
    /// 微信接口返回的错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
}