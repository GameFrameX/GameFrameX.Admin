namespace GameFrameX.Client.Api.RemoteConfig.Dto;

public sealed class RemoteConfigLocation
{
    /// <summary>
    /// IP
    /// </summary>
    public string Ip { get; set; } = null!;

    /// <summary>
    /// 国家
    /// </summary>
    public string Country { get; set; } = null!;

    /// <summary>
    /// 区域
    /// </summary>
    public string Region { get; set; } = null!;

    /// <summary>
    /// 城市
    /// </summary>
    public string City { get; set; } = null!;

    /// <summary>
    /// 时区
    /// </summary>
    public string TimeZone { get; set; } = null!;

    /// <summary>
    /// 国家
    /// </summary>
    public string CountryLong { get; set; } = null!;

    /// <summary>
    /// 国家
    /// </summary>
    public string CountryShort { get; set; } = null!;

    /// <summary>
    /// 货币简码，例如: "中文-人民币-CNY"
    /// https://zh.wikipedia.org/wiki/ISO_4217
    /// https://baike.baidu.com/item/ISO%204217/11018231
    /// </summary>
    public string Currency { get; set; } = null!;
}