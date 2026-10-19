using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Device;

/// <summary>
/// 客户机器码（应用级设备资源）。
/// <para>作用域：租户库内应用级资源，由 <see cref="EntityBasic"/> 的 TenantId 与 <see cref="IAppScoped.AppId"/> 共同界定；
/// 同一租户下不同应用的机器码彼此独立，机器码唯一性 = TenantId + AppId + DeviceUniqueIdentifier。</para>
/// <para>命名遵循 C210 应用中心实体家族 <c>TenantGameApp*Entity</c>；物理表名由类名下划线生成（<c>tenant_game_app_device_entity</c>），与 <c>TenantGameAppPackageEntity</c> 等同模式（<c>SugarTable(null)</c>）。</para>
/// </summary>
[SugarTable(null, "客户机器码")]
[SugarIndex("idx_tenant_game_app_device_tenant_app_uid", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc, nameof(DeviceUniqueIdentifier), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameAppDeviceEntity : EntityBasicSelect, IAppScoped
{
    /// <summary>
    /// 应用 ID。机器码归属具体应用，必须 &gt; 0；同一租户下不同应用的机器码彼此独立，禁止 AppId=0 的租户默认机器码。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 机器码
    /// </summary>
    [SugarColumn(ColumnDescription = "机器码")]
    [MaxLength(EntityStringLength.Length1024)]
    public string? DeviceUniqueIdentifier { get; set; }
}