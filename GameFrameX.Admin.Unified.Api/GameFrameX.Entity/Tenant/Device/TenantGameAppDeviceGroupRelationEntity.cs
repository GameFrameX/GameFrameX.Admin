using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Device;

/// <summary>
/// 客户机器码对机器码分组关系（应用级）。
/// <para>关系两端的 DeviceId 与 DeviceGroupId 必须属于同一 AppId；写入由 <c>TenantGameAppDeviceGroupService</c> 校验同应用一致性，
/// 跨应用设备加入分组直接拒绝。</para>
/// <para>命名遵循 C210 应用中心实体家族 <c>TenantGameApp*Entity</c>；物理表名由类名下划线生成（<c>tenant_game_app_device_group_relation_entity</c>）。</para>
/// </summary>
[SugarTable(null, "客户机器码对机器码分组")]
[SugarIndex("idx_tenant_game_app_device_group_relation_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameAppDeviceGroupRelationEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用 ID。冗余自设备与分组的 AppId，用于关系级应用隔离校验；必须与两端 DeviceId/DeviceGroupId 所属应用一致。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 设备 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "设备Id", DefaultValue = "0")]
    public long DeviceId { get; set; }

    /// <summary>
    /// 设备分组 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "设备分组Id", DefaultValue = "0")]
    public long DeviceGroupId { get; set; }
}