using GameFrameX.Entity.Base;
using GameFrameX.Foundation.Orm.Entity.Filter;

namespace GameFrameX.Entity.Tenant.Device;

/// <summary>
/// 客户机器码分组（应用级设备资源）。
/// <para>作用域：租户库内应用级资源，由 TenantId + AppId 界定；分组唯一性 = TenantId + AppId + Name。
/// 分组只能绑定同一应用下的设备，跨应用设备加入分组由 <c>TenantGameAppDeviceGroupService</c> 拒绝。</para>
/// <para>命名遵循 C210 应用中心实体家族 <c>TenantGameApp*Entity</c>；物理表名由类名下划线生成（<c>tenant_game_app_device_group_entity</c>）。</para>
/// </summary>
[SugarTable(null, "客户机器码分组")]
[SugarIndex("idx_tenant_game_app_device_group_tenant_app_name", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc, nameof(Name), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public sealed class TenantGameAppDeviceGroupEntity : EntityBasic, ISelectFilter, IAppScoped
{
    /// <summary>
    /// 应用 ID。分组归属具体应用，必须 &gt; 0；同一租户下不同应用的分组彼此独立。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [SugarColumn(ColumnDescription = "详细描述", Length = EntityStringLength.Length1024)]
    public string? Description { get; set; }
}