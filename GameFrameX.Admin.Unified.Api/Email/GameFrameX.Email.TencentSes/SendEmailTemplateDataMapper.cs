namespace GameFrameX.Email.TencentSes;

/// <summary>
/// 腾讯云 SES 模板变量映射器。
/// <para>将业务层提交的 <see cref="IDictionary{TKey,TValue}"/> 映射为腾讯云 SES 当前支持的模板数据对象；
/// 未知键会被忽略，保证运行发送与测试发送共享同一字段语义。</para>
/// </summary>
public static class SendEmailTemplateDataMapper
{
    /// <summary>
    /// 将前端或业务层提交的模板变量字典映射为腾讯云 SES 当前支持的模板数据对象。
    /// <para>当前模板数据对象只暴露 <c>username</c>、<c>verification_code</c>、<c>expiry_minutes</c> 字段；未知键会被忽略。</para>
    /// </summary>
    public static SendEmailTemplateData FromDictionary(IDictionary<string, string> templateData)
    {
        var data = new SendEmailTemplateData();
        if (templateData == null || templateData.Count == 0)
        {
            return data;
        }

        if (templateData.TryGetValue("username", out var username))
        {
            data.Username = username ?? string.Empty;
        }

        if (templateData.TryGetValue("verification_code", out var code))
        {
            data.VerificationCode = code ?? string.Empty;
        }

        if (templateData.TryGetValue("expiry_minutes", out var minutes))
        {
            data.ExpiryMinutes = minutes ?? string.Empty;
        }

        return data;
    }
}