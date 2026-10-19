namespace GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto;

/// <summary>
/// 租户微信公众号 / 小程序配置测试发送输入参数。
/// <para>公众号 / 小程序授权登录已从 Admin 登录移除，本接口仅服务租户上下文内的模板消息测试发送。</para>
/// </summary>
public class TenantWechatConfigTestSendRequest
{
    /// <summary>
    /// 配置 ID。必填；测试发送必须显式指定目标租户配置。
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 接收模板消息的目标用户 OpenId。
    /// </summary>
    [Required(ErrorMessage = "接收者OpenId不能为空")]
    public string ReceiverOpenId { get; set; } = null!;

    /// <summary>
    /// 微信模板消息 ID。
    /// </summary>
    [Required(ErrorMessage = "模板ID不能为空")]
    public string TemplateId { get; set; } = null!;

    /// <summary>
    /// 模板消息点击跳转链接。
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// 小程序页面路径。
    /// </summary>
    public string MiniProgramPagePath { get; set; } = null!;

    /// <summary>
    /// 模板变量。Key 与微信模板占位符一致，Value 为字符串内容。
    /// </summary>
    public Dictionary<string, string> TestData { get; set; } = new();
}