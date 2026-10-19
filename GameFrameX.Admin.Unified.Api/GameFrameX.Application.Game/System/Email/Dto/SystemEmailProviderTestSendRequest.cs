namespace GameFrameX.Application.Game.System.Email.Dto;

/// <summary>
/// 测试发送输入参数。
/// <para>用于受权限控制的测试发送接口；调用方必须显式传入用途与目标邮箱；敏感字段不输出到 API/日志/前端。</para>
/// </summary>
public class SystemEmailProviderTestSendRequest
{
    /// <summary>
    /// 配置 ID。测试发送必须指定当前要验证的 Provider 配置。
    /// </summary>
    [Required(ErrorMessage = "配置ID不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 目标收件人邮箱。
    /// </summary>
    [Required(ErrorMessage = "收件人邮箱不能为空")]
    [EmailAddress(ErrorMessage = "收件人邮箱格式不正确")]
    public string ReceiverEmail { get; set; } = null!;

    /// <summary>
    /// 测试主题。
    /// </summary>
    [Required(ErrorMessage = "主题不能为空")]
    public string Subject { get; set; } = "平台系统邮件测试";

    /// <summary>
    /// 测试正文。
    /// </summary>
    [Required(ErrorMessage = "正文不能为空")]
    public string Body { get; set; } = "这是一封来自平台系统邮件 Provider 的测试邮件。";
}