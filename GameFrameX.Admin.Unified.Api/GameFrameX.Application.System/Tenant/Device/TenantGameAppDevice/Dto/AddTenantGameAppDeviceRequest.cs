namespace GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice.Dto;

/// <summary>
/// 机器码增加输入参数
/// </summary>
public class AddTenantGameAppDeviceRequest : BaseNamedDescriptionRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public override string? Name { get; set; }

    /// <summary>
    /// 设备唯一标识
    /// </summary>
    [Required(ErrorMessage = "机器码不能为空")]
    public string? DeviceUniqueIdentifier { get; set; }
}