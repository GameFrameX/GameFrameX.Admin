namespace GameFrameX.Application.Game.Tenant.Integration.TenantEmailConfig.Dto;

/// <summary>
/// 租户业务邮件配置输出参数（敏感字段已脱敏）
/// </summary>
public class TenantEmailConfigResponse : IBasePageResponse<GameFrameX.Entity.Tenant.ClientApiConfig.TenantEmailConfigEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用 ID。0 表示租户默认邮件配置；大于 0 表示应用级覆盖。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 租户业务邮件用途（TenantEmailPurpose）。0 表示通配该租户 / 应用下所有租户业务邮件。
    /// </summary>
    public int Purpose { get; set; }

    /// <summary>
    /// Provider 类型。当前仅支持 TencentCloudSes。
    /// </summary>
    public string Provider { get; set; } = "TencentCloudSes";

    /// <summary>
    /// Provider 区域（如腾讯云 SES 区域 ap-hongkong）。
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// 腾讯云邮件模板组 / 模板集标识；用于按业务组维护模板。
    /// </summary>
    public string? TemplateSet { get; set; }

    /// <summary>
    /// 发件人邮箱地址。
    /// </summary>
    public string FromEmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// 默认发件人显示名称。
    /// </summary>
    public string? DefaultFromName { get; set; }

    /// <summary>
    /// 默认邮件主题前缀（例如 [GameName]）。
    /// </summary>
    public string? DefaultSubjectPrefix { get; set; }

    /// <summary>
    /// 邮件服务商 SecretId（敏感字段，API 输出需脱敏，日志禁止明文）。
    /// </summary>
    public string FromEmailSecretId { get; set; } = string.Empty;

    /// <summary>
    /// 邮件服务商 SecretKey（敏感字段，API 输出需脱敏，日志禁止明文）。
    /// </summary>
    public string FromEmailSecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 配置选择优先级。同一匹配层中按此值降序选择；数值越大越优先。
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;
}