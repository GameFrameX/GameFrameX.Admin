namespace GameFrameX.Application.Game.Tenant.Integration.TenantEmailConfig.Dto;

/// <summary>
/// 租户业务邮件测试发送输入参数。
/// </summary>
public class TenantEmailConfigTestSendRequest
{
    /// <summary>
    /// 配置 ID。空表示按当前租户 + 应用上下文自动选择。
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 目标收件人邮箱。
    /// </summary>
    [Required(ErrorMessage = "收件人邮箱不能为空")]
    [EmailAddress(ErrorMessage = "收件人邮箱格式不正确")]
    public string ReceiverEmail { get; set; } = null!;

    /// <summary>
    /// 模板 ID。
    /// </summary>
    [Required(ErrorMessage = "模板ID不能为空")]
    public ulong TemplateId { get; set; }

    /// <summary>
    /// 邮件主题。
    /// </summary>
    [Required(ErrorMessage = "主题不能为空")]
    public string Subject { get; set; } = "租户业务邮件测试";

    /// <summary>
    /// 模板变量。
    /// </summary>
    public Dictionary<string, string> TemplateData { get; set; } = new();
}