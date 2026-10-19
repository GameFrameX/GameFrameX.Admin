namespace GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto;

/// <summary>
/// 租户微信公众号 / 小程序配置输出参数（敏感字段已脱敏）。
/// </summary>
public class TenantWechatConfigResponse : IBasePageResponse<GameFrameX.Entity.Tenant.ClientApiConfig.TenantWechatConfigEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用 ID。0 表示租户默认公众号/小程序配置；大于 0 表示应用级覆盖。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 微信公众号 AppId。
    /// </summary>
    public string WechatAppId { get; set; } = string.Empty;

    /// <summary>
    /// 微信公众号 AppSecret（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    public string WechatAppSecret { get; set; } = string.Empty;

    /// <summary>
    /// 微信小程序 AppId。
    /// </summary>
    public string WxOpenAppId { get; set; } = string.Empty;

    /// <summary>
    /// 微信小程序 AppSecret（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    public string WxOpenSecret { get; set; } = string.Empty;

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