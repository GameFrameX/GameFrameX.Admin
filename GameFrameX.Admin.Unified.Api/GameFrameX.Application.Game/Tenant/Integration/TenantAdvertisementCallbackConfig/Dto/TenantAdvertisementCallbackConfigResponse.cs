using GameFrameX.Entity.Tenant.ClientApiConfig;

namespace GameFrameX.Application.Game.Tenant.Integration.TenantAdvertisementCallbackConfig.Dto;

/// <summary>
/// 租户广告回调配置输出参数（敏感字段已脱敏）
/// </summary>
public class TenantAdvertisementCallbackConfigResponse : IBasePageResponse<TenantAdvertisementCallbackConfigEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用 ID。0 表示租户默认配置；大于 0 表示应用级覆盖。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 广告平台 Provider。当前首批配置支持 TopOn、GoogleAdMob、Pangle、UnityAds。
    /// </summary>
    public string Provider { get; set; } = "TopOn";

    /// <summary>
    /// 广告平台后台配置的回调 URL。
    /// </summary>
    public string CallbackUrl { get; set; } = string.Empty;

    /// <summary>
    /// 签名密钥或共享密钥（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    public string SigningSecret { get; set; } = string.Empty;

    /// <summary>
    /// 验签公钥或验证材料（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    public string VerificationKeys { get; set; } = string.Empty;

    /// <summary>
    /// 广告回调动态配置 JSON。字段由 Provider schema 决定。
    /// </summary>
    public string ConfigJson { get; set; } = string.Empty;

    /// <summary>
    /// 参数作用说明 JSON。记录该 Provider 的核心参数如何映射到用户、交易、奖励、签名、审计等用途。
    /// </summary>
    public string ParameterPurposeJson { get; set; } = string.Empty;

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