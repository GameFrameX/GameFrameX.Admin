namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 增加小程序订阅消息模板（C23：必须显式传入 AppId）。
/// </summary>
public class WxOpenAddSubscribeMessageTemplateRequest : AddSubscribeMessageTemplateRequest
{
    /// <summary>
    /// 应用 ID。0/null 表示租户默认小程序配置；&gt; 0 表示应用级覆盖。
    /// </summary>
    public long? AppId { get; set; }
}