namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 小程序订阅消息发送结果
/// </summary>
public class WxOpenSendResultResponse
{
    /// <summary>
    /// 微信接口返回的错误码（0 表示成功）
    /// </summary>
    public int? ErrorCode { get; set; }

    /// <summary>
    /// 微信接口返回的错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
}