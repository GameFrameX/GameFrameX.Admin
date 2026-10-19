using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Routing;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.System;

/// <summary>
/// 租户菜单同步状态表（主库）。每租户一行，Id 即 TenantId（确定性主键，跨重启幂等 upsert）。
/// <para>状态行缺失视为待同步（首次部署全量入队的依据）；租户删除时同步清理状态行（见 <see cref="GameFrameX.Core.Service.Tenant.SystemTenantService"/>）。</para>
/// </summary>
[SugarTable(null, "租户菜单同步状态表")]
[PlatformTable]
[IncrementTable]
public class SystemTenantMenuSyncStateEntity : EntityBase
{
    /// <summary>
    /// 最近一次成功同步时的菜单目录版本号（MenuSeedCatalog.CatalogVersion）
    /// </summary>
    [SugarColumn(ColumnDescription = "最近目录版本号", Length = EntityStringLength.Length256)]
    public string LastCatalogVersion { get; set; } = string.Empty;

    /// <summary>
    /// 同步状态
    /// </summary>
    [SugarColumn(ColumnDescription = "同步状态", DefaultValue = "1")]
    public TenantMenuSyncStatusEnum Status { get; set; } = TenantMenuSyncStatusEnum.Pending;

    /// <summary>
    /// 最近一次同步时间（秒级 Unix 时间戳，含失败）
    /// </summary>
    [SugarColumn(ColumnDescription = "最近同步时间", IsNullable = true)]
    public long? LastSyncTime { get; set; }

    /// <summary>
    /// 最近一次失败原因（悬空套餐/租户库不可达等；成功时清空）
    /// </summary>
    [SugarColumn(ColumnDescription = "最近失败原因", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string? LastError { get; set; }

    /// <summary>
    /// 下次可重试时间（秒级 Unix 时间戳；失败退避重入队不占并发槽）
    /// </summary>
    [SugarColumn(ColumnDescription = "下次可重试时间", IsNullable = true)]
    public long? NextRetryTime { get; set; }
}
