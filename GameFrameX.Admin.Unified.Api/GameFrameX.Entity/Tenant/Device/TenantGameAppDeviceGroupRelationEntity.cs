using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Device;

/// <summary>
/// 客户机器码对机器码分组关系（应用级）。
/// 跨应用设备加入分组直接拒绝。</para>
/// <para>命名遵循 C210 应用中心实体家族 <c>TenantGameApp*Entity</c>；物理表名由类名下划线生成（<c>tenant_game_app_device_group_relation_entity</c>）。</para>
/// </summary>
[SugarTable(null, "客户机器码对机器码分组")]
[TenantTable]
[IncrementTable]
public class TenantGameAppDeviceGroupRelationEntity : EntityBasic
{

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