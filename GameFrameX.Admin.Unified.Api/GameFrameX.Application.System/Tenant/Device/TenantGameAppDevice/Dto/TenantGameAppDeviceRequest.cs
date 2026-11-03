namespace GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice.Dto;

/// <summary>
/// 机器码分页查询输入参数
/// </summary>
public class TenantGameAppDeviceRequest : BasePageRequest
{
    /// <summary>
    /// 设备分组 Id（可选过滤）：按设备-分组关系过滤属于该分组的设备。
    /// </summary>
    public long? GroupId { get; set; }
}