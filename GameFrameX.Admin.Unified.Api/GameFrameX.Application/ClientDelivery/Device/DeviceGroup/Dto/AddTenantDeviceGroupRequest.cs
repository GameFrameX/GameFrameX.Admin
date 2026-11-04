namespace GameFrameX.Application.ClientDelivery.Device.DeviceGroup.Dto;

/// <summary>
/// 机器码分组增加输入参数
/// </summary>
public class AddTenantDeviceGroupRequest : BaseNamedDescriptionRequest
{
    /// <summary>
    /// 机器码列表（必须与本分组同应用，跨应用设备由服务拒绝）
    /// </summary>
    public List<long>? DeviceIds { get; set; }
}