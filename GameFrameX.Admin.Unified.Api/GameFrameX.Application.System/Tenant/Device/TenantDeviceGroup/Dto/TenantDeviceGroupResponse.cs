namespace GameFrameX.Application.System.Tenant.Device.TenantDeviceGroup.Dto;

/// <summary>
/// 机器码分组输出参数
/// </summary>
// 纯 POCO 投影（C228）：属性名与 TenantGameAppDeviceGroupEntity 保持一致，
// 供 .Select&lt;TenantDeviceGroupResponse&gt;() 自动投影与 Mapster 按名映射。
public class TenantDeviceGroupResponse : IBasePageResponse<global::GameFrameX.Entity.Tenant.Device.TenantGameAppDeviceGroupEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用 ID。分组归属具体应用，必须 &gt; 0；同一租户下不同应用的分组彼此独立。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// DeviceIds绑定的设备id
    /// </summary>
    public List<long>? DeviceIds { get; set; }

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
}