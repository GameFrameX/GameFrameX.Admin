namespace GameFrameX.Application.System.Tenant.Device.TenantDeviceGroup.Dto;

/// <summary>
/// 机器码分组分页查询输入参数
/// </summary>
public class TenantDeviceGroupRequest : BasePageRequest
{
    /// <summary>
    /// 应用 Id（必填过滤）。设备分组为应用级资源，PageAsync 必须按当前应用收口。
    /// </summary>
    public long? AppId { get; set; }
}