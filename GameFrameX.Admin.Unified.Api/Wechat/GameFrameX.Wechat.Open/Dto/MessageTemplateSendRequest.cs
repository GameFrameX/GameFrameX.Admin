using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 获取消息模板列表
/// </summary>
public class MessageTemplateSendRequest
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
    /// 模板数据，格式形如 { "key1": { "value": any }, "key2": { "value": any } }
    /// </summary>
    [Required(ErrorMessage = "模板数据不能为空")]
    public Dictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem> Data { get; set; } = new();

    /// <summary>
    /// 模板跳转链接
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 所需跳转到小程序的具体页面路径，支持带参数,（示例index?foo=bar）
    /// </summary>
    public string MiniProgramPagePath { get; set; } = string.Empty;
}