namespace GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice.Dto;

/// <summary>
/// 机器码更新输入参数
/// </summary>
public class UpdateTenantGameAppDeviceRequest : BaseNamedDescriptionRequest, ICrudUpdateRequest
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 客户端读取记录时的版本号，用于阻止过期数据覆盖较新的修改。
    /// </summary>
    [Required(ErrorMessage = "版本号不能为空")]
    [Range(0, long.MaxValue, ErrorMessage = "版本号不能小于0")]
    public long? RowVersion { get; set; }

    /// <summary>
    /// 应用 Id。不可变（服务端从既有实体回填）；声明于 DTO 仅为契约对称，前端无需传入。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 设备唯一标识
    /// </summary>
    [Required(ErrorMessage = "机器码不能为空")]
    public string? DeviceUniqueIdentifier { get; set; }
}