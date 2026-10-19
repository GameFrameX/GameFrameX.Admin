namespace GameFrameX.Application.Game.Order.GamePaymentConfig.Dto;

/// <summary>
/// 游戏支付配置输出参数
/// </summary>
public class GamePaymentConfigResponse : IBasePageResponse<Entity.Tenant.Game.TenantGamePaymentConfigEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 租户 Id。wire 功能字段特例保留（非审计泄漏）：前端 buildNotifyUrl 以其为路径段拼接支付回调地址
    /// （C65），SuperAdmin 视角每行租户不同，无法在客户端侧补全。
    /// </summary>
    public long TenantId { get; set; }

    /// <summary>
    /// 应用ID。0表示租户默认配置。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 游戏应用ID。空表示租户默认配置。
    /// </summary>
    public long? GameAppId { get; set; }

    /// <summary>
    /// 分包ID。空表示应用默认配置。
    /// </summary>
    public long? PackageId { get; set; }

    /// <summary>
    /// 游戏应用ID归一化值，空按0参与唯一性判断。
    /// </summary>
    public long GameAppIdNormalized { get; set; }

    /// <summary>
    /// 分包ID归一化值，空按0参与唯一性判断。
    /// </summary>
    public long PackageIdNormalized { get; set; }

    /// <summary>
    /// 支付渠道
    /// </summary>
    public string PayChannel { get; set; } = string.Empty;

    /// <summary>
    /// 是否沙盒配置
    /// </summary>
    public bool IsSandbox { get; set; }

    /// <summary>
    /// 支付配置JSON
    /// </summary>
    public string ConfigJson { get; set; } = string.Empty;

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