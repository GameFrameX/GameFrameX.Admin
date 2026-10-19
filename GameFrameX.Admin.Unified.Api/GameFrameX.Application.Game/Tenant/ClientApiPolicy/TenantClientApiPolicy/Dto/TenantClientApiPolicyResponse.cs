namespace GameFrameX.Application.Game.Tenant.ClientApiPolicy.TenantClientApiPolicy.Dto;

/// <summary>
/// 租户 Client API 运行策略输出参数
/// </summary>
public class TenantClientApiPolicyResponse : IBasePageResponse<GameFrameX.Entity.Tenant.ClientApiConfig.TenantClientApiPolicyEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用 ID。0 表示租户默认行；大于 0 表示应用级覆盖。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 访问令牌签名密钥（输出恒为脱敏占位，仅 AppId=0 行被运行时消费）
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// 访问令牌有效期（分钟）。
    /// </summary>
    public int LifetimeMinutes { get; set; }

    /// <summary>
    /// 票据固定到期时间（Unix 秒）。0 表示永不过期。
    /// </summary>
    public long TicketExpireTime { get; set; }

    /// <summary>
    /// 是否允许首次签发时自动创建租户应用级 RSA key。
    /// </summary>
    public bool EnableKeyAutoCreate { get; set; } = true;

    /// <summary>
    /// 区服列表是否返回 HttpManagerUrl。
    /// </summary>
    public bool EnableReturnHttpManager { get; set; }

    /// <summary>
    /// 订单是否启用沙箱直发（测试充值）。
    /// </summary>
    public bool EnableSandBoxRecharge { get; set; }

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
