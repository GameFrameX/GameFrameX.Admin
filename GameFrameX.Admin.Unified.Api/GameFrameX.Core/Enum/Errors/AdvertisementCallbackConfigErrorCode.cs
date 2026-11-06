namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 广告回调配置错误码（段 318）。
/// </summary>
public enum AdvertisementCallbackConfigErrorCode
{
    /// <summary>
    /// 同一分包下的广告平台配置已存在。
    /// </summary>
    [ErrorText("同一分包下的广告平台回调配置已存在", "An advertisement callback configuration already exists for this package and provider")]
    PackageProviderAlreadyExists = 31800001,
}
