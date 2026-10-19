namespace GameFrameX.Core.Logging;

/// <summary>
/// IP 地址解析结果（地理位置文本与经纬度）。
/// </summary>
internal sealed class IpAddressInfo
{
    /// <summary>地理位置文本。</summary>
    public string IpLocation { get; }

    /// <summary>经度。</summary>
    public double? Longitude { get; }

    /// <summary>纬度。</summary>
    public double? Latitude { get; }

    public IpAddressInfo(string ipLocation, double? longitude, double? latitude)
    {
        IpLocation = ipLocation;
        Longitude = longitude;
        Latitude = latitude;
    }
}