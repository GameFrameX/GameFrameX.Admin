namespace GameFrameX.Application.System.Tenant.Device.TenantDeviceGroup.Dto;

/// <summary>
/// 机器码分组增加输入参数
/// </summary>
public class AddTenantDeviceGroupRequest : BaseNamedDescriptionRequest
{
    /// <summary>
    /// 应用 Id。分组归属具体应用，必须 &gt; 0。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 机器码列表（必须与本分组同应用，跨应用设备由服务拒绝）
    /// </summary>
    public List<long>? DeviceIds { get; set; }
}