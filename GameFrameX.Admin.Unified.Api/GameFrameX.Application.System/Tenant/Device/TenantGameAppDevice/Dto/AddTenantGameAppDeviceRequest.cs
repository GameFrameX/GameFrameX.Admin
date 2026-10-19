namespace GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice.Dto;

/// <summary>
/// 机器码增加输入参数
/// </summary>
public class AddTenantGameAppDeviceRequest : BaseNamedDescriptionRequest
{
    /// <summary>
    /// 应用 Id。机器码归属具体应用，必须 &gt; 0；同一租户下不同应用的机器码彼此独立。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

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