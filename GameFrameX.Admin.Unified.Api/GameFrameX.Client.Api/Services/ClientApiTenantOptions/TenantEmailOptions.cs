namespace GameFrameX.Client.Api.Services.ClientApiTenantOptions;

/// <summary>
/// 租户业务邮件配置快照。null 引用表示该作用域未配置邮件凭据，调用方必须返回明确错误，不得 fallback 到系统邮件。
/// </summary>
public sealed class TenantEmailOptions
{
    /// <summary>
    /// 发件人邮箱地址。
    /// </summary>
    public string? FromEmailAddress { get; init; }

    /// <summary>
    /// 邮件服务商 SecretId。
    /// </summary>
    public string? FromEmailSecretId { get; init; }

    /// <summary>
    /// 邮件服务商 SecretKey。
    /// </summary>
    public string? FromEmailSecretKey { get; init; }
}