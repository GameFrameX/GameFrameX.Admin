using GameFrameX.Entity.Base;
using GameFrameX.Foundation.Orm.Entity.Filter;

namespace GameFrameX.Entity.Business.Device;

/// <summary>
/// 客户机器码分组（应用级设备资源）。
/// 分组只能绑定同一应用下的设备，跨应用设备加入分组由 <c>GameAppDeviceGroupService</c> 拒绝。</para>
/// <para>命名遵循 C210 应用中心实体家族 <c>GameApp*Entity</c>；物理表名由类名下划线生成（<c>game_app_device_group_entity</c>）。</para>
/// </summary>
[SugarTable(null, "客户机器码分组")]
[SugarIndex("idx_device_group_name", nameof(Name), OrderByType.Asc)]
[BusinessTable]
[IncrementTable]
public sealed class GameAppDeviceGroupEntity : EntityBasic, ISelectFilter
{

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