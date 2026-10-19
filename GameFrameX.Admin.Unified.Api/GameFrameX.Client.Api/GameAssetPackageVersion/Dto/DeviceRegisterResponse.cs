namespace GameFrameX.Client.Api.GameAssetPackageVersion.Dto;

/// <summary>
/// 设备注册响应（幂等：重复注册返回既有设备 Id，不重复插入）
/// </summary>
public class DeviceRegisterResponse
{
    /// <summary>
    /// 设备 Id（AppId + DeviceUniqueIdentifier 已存在时返回既有行 Id）
    /// </summary>
    public long DeviceId { get; set; }
}
