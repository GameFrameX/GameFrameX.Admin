namespace GameFrameX.Application.Game.Tenant.Integration.TenantSmsConfig.Dto;

/// <summary>
/// 租户 SMS 配置输出参数（敏感字段已脱敏）。
/// </summary>
public class TenantSmsConfigResponse : IBasePageResponse<GameFrameX.Entity.Tenant.ClientApiConfig.TenantSmsConfigEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用 ID。0 表示租户默认 SMS 配置；大于 0 表示应用级覆盖。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// Provider 类型。当前仅支持 Aliyun（阿里云短信）。
    /// </summary>
    public string Provider { get; set; } = "Aliyun";

    /// <summary>
    /// 阿里云 AccessKeyId（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// 阿里云 AccessKeySecret（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    public string AccessKeySecret { get; set; } = string.Empty;

    /// <summary>
    /// 短信签名（如 AdminNET平台）。
    /// </summary>
    public string SignName { get; set; } = string.Empty;

    /// <summary>
    /// 短信模板 Code（阿里云 SMS_ 开头）。可为空（测试发送时由调用方指定）。
    /// </summary>
    public string? TemplateCode { get; set; }

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