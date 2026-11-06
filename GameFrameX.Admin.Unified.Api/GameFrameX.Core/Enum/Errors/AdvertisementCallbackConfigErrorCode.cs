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

    /// <summary>
    /// 分包 Id 必填且必须为正数。
    /// </summary>
    [ErrorText("分包 Id 必须为正数", "Package id must be a positive number")]
    PackageIdRequired = 31800002,

    /// <summary>
    /// 指定的游戏应用分包不存在或未启用。
    /// </summary>
    [ErrorText("分包不存在或未启用", "Package does not exist or is not enabled")]
    PackageNotFound = 31800003,
}
