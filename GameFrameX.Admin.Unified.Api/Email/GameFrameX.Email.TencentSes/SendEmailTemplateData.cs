using System.Text.Json.Serialization;

namespace GameFrameX.Email.TencentSes;

/// <summary>
/// 腾讯云 SES 模板变量数据。
/// <para>当前通用模板仅暴露 <c>username</c> / <c>verification_code</c> / <c>expiry_minutes</c> 三字段；
/// 新模板扩展时按需新增字段并同步更新 <see cref="SendEmailTemplateDataMapper"/>。</para>
/// </summary>
public sealed class SendEmailTemplateData
{
    /// <summary>
    /// 用户名（兜底字段，正文通过该字段携带）。
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// 验证码，6 位数字。
    /// </summary>
    [JsonPropertyName("verification_code")]
    public string VerificationCode { get; set; } = null!;

    /// <summary>
    /// 过期时间（分钟），默认 5。
    /// </summary>
    [JsonPropertyName("expiry_minutes")]
    public string ExpiryMinutes { get; set; } = 5.ToString();
}