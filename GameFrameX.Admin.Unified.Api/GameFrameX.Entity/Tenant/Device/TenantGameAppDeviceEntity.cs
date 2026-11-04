using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Device;

/// <summary>
/// 客户机器码（应用级设备资源）。
/// <para>命名遵循 C210 应用中心实体家族 <c>TenantGameApp*Entity</c>；物理表名由类名下划线生成（<c>tenant_game_app_device_entity</c>），与 <c>TenantGameAppPackageEntity</c> 等同模式（<c>SugarTable(null)</c>）。</para>
/// </summary>
[SugarTable(null, "客户机器码")]
[SugarIndex("idx_device_unique_identifier", nameof(DeviceUniqueIdentifier), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameAppDeviceEntity : EntityBasicSelect
{

    /// <summary>
    /// 机器码
    /// </summary>
    [SugarColumn(ColumnDescription = "机器码")]
    [MaxLength(EntityStringLength.Length1024)]
    public string? DeviceUniqueIdentifier { get; set; }
}