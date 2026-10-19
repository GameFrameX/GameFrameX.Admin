using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 发送订阅消息
/// </summary>
public class SendSubscribeMessageRequest
{
    /// <summary>
    /// 订阅模板Id
    /// </summary>
    [Required(ErrorMessage = "订阅模板Id不能为空")]
    public string TemplateId { get; set; } = string.Empty;

    /// <summary>
    /// 接收者的OpenId
    /// </summary>
    [Required(ErrorMessage = "接收者的OpenId不能为空")]
    public string ToUserOpenId { get; set; } = string.Empty;

    /// <summary>
    /// 模板内容，格式形如 { "key1": { "value": any }, "key2": { "value": any } }
    /// </summary>
    [Required(ErrorMessage = "模板内容不能为空")]
    public Dictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem> Data { get; set; } = new();

    /// <summary>
    /// 跳转小程序类型
    /// </summary>
    public string MiniprogramState { get; set; } = string.Empty;

    /// <summary>
    /// 语言类型
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// 点击模板卡片后的跳转页面（仅限本小程序内的页面），支持带参数（示例pages/app/index?foo=bar）
    /// </summary>
    public string MiniProgramPagePath { get; set; } = string.Empty;
}