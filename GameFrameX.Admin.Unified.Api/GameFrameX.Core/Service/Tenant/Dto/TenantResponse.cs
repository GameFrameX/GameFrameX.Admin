using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.Tenant.Dto;

/// <summary>
/// 系统租户输出参数
/// </summary>
// 纯 POCO 投影（C228）：属性名与 GameFrameX.Entity.System.SystemTenantEntity 保持一致，
// 供 .Select(u => new TenantResponse {...}) 投影与 Mapster 按名映射。
// Name/AdminAccount/Email/Phone 为非实体列的展示字段，由 HydrateTenantAdminAsync 从租户库补全。
public class TenantResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 机构Id
    /// </summary>
    public long OrganizationId { get; set; }

    /// <summary>
    /// 主机
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// 租户类型
    /// </summary>
    public TenantTypeEnum TenantType { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    public DbType DbType { get; set; }

    /// <summary>
    /// 数据库连接
    /// </summary>
    public string? Connection { get; set; }

    /// <summary>
    /// 数据库标识
    /// </summary>
    public string? ConfigId { get; set; }

    /// <summary>
    /// 套餐等级Id
    /// </summary>
    public long? PackageId { get; set; }

    /// <summary>
    /// 到期时间（秒级 Unix 时间戳）
    /// </summary>
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 过渡期天数（覆盖套餐默认值，null 使用套餐默认）
    /// </summary>
    public int? GracePeriodDays { get; set; }

    /// <summary>
    /// 租户名称
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// 管理员账号
    /// </summary>
    public virtual string AdminAccount { get; set; } = null!;

    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// 电话
    /// </summary>
    public string Phone { get; set; } = null!;

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
    public string? Remark { get; set; }

    /// <summary>
    /// 套餐菜单同步状态（主库 <c>system_tenant_menu_sync_state_entity</c> hydrate；状态行缺失视为待同步）。
    /// 单一事实源见 C353 状态表与三态枚举。
    /// </summary>
    public TenantMenuSyncStatusEnum MenuSyncStatus { get; set; } = TenantMenuSyncStatusEnum.Pending;

    /// <summary>
    /// 最近一次套餐菜单同步时间（秒级 Unix 时间戳，含失败；状态行缺失为 null）。
    /// </summary>
    public long? MenuSyncLastSyncTime { get; set; }

    /// <summary>
    /// 最近一次套餐菜单同步失败原因（成功时为空；状态行缺失为 null）。
    /// </summary>
    public string? MenuSyncLastError { get; set; }

    /// <summary>
    /// 下一次可重试时间（秒级 Unix 时间戳；状态行缺失为 null）。
    /// </summary>
    public long? MenuSyncNextRetryTime { get; set; }
}