namespace GameFrameX.Wechat.Open.Dto;

/// <summary>
/// 发送公众号模板消息（C23：必须显式传入 AppId）。
/// </summary>
public class WechatMessageTemplateSendRequest : MessageTemplateSendRequest
{
    /// <summary>
    /// 应用 ID。0/null 表示租户默认公众号配置；&gt; 0 表示应用级覆盖。
    /// </summary>
    public long? AppId { get; set; }
}