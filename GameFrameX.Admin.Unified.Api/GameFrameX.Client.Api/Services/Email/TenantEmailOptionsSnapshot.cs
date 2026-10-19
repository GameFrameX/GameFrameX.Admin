namespace GameFrameX.Client.Api.Services.Email;

/// <summary>
/// 租户业务邮件配置快照（运行期只读视图）。
/// <para>封装 <see cref="GameFrameX.Entity.Tenant.ClientApiConfig.TenantEmailConfigEntity"/> 的关键字段；调用方按需读取，禁止修改。</para>
/// </summary>
public sealed class TenantEmailOptionsSnapshot
{
    /// <summary>
    /// 配置 ID。
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 命中的用途（解析结果）。0 表示通配命中。
    /// </summary>
    public int Purpose { get; init; }

    /// <summary>
    /// Provider 类型。
    /// </summary>
    public string Provider { get; init; } = "TencentCloudSes";

    /// <summary>
    /// Provider 区域。
    /// </summary>
    public string? Region { get; init; }

    /// <summary>
    /// 模板组 / 模板集标识。
    /// </summary>
    public string? TemplateSet { get; init; }

    /// <summary>
    /// 发件人邮箱。
    /// </summary>
    public string FromEmailAddress { get; init; } = string.Empty;

    /// <summary>
    /// 默认发件人显示名。
    /// </summary>
    public string? DefaultFromName { get; init; }

    /// <summary>
    /// 默认邮件主题前缀。
    /// </summary>
    public string? DefaultSubjectPrefix { get; init; }

    /// <summary>
    /// SecretId。
    /// </summary>
    public string FromEmailSecretId { get; init; } = string.Empty;

    /// <summary>
    /// SecretKey。
    /// </summary>
    public string FromEmailSecretKey { get; init; } = string.Empty;
}