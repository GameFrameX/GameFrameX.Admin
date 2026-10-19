namespace GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice.Dto;

/// <summary>
/// 机器码输出参数
/// </summary>
// 纯 POCO 投影（C228）：属性名与 TenantGameAppDeviceEntity（含 EntityBasicSelect 的 Name/Description）保持一致，
// 供 .Select&lt;TenantGameAppDeviceResponse&gt;() 自动投影与 Mapster 按名映射。
public class TenantGameAppDeviceResponse : IBasePageResponse<global::GameFrameX.Entity.Tenant.Device.TenantGameAppDeviceEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用 ID。机器码归属具体应用，必须 &gt; 0；同一租户下不同应用的机器码彼此独立，禁止 AppId=0 的租户默认机器码。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 机器码
    /// </summary>
    public string? DeviceUniqueIdentifier { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

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