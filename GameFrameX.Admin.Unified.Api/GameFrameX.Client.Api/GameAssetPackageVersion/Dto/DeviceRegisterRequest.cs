namespace GameFrameX.Client.Api.GameAssetPackageVersion.Dto;

/// <summary>
/// 设备显式注册请求（GFX-706：从 GetInfo 隐式副作用拆出为独立端点）
/// </summary>
public class DeviceRegisterRequest
{
    /// <summary>
    /// 设备唯一标识（机器码）
    /// </summary>
    public string DeviceUniqueIdentifier { get; set; } = string.Empty;
}
