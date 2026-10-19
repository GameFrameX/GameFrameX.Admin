namespace GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice.Dto;

/// <summary>
/// 机器码分页查询输入参数
/// </summary>
public class TenantGameAppDeviceRequest : BasePageRequest
{
    /// <summary>
    /// 应用 Id（必填过滤）。设备码为应用级资源，PageAsync 必须按当前应用收口。
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 设备分组 Id（可选过滤）：按设备-分组关系过滤属于该分组的设备。
    /// </summary>
    public long? GroupId { get; set; }
}