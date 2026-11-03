namespace GameFrameX.Client.Api.Services.PackageVersion;

/// <summary>
/// 包请求标记接口，标识具有包相关属性的请求
/// </summary>
public interface IPackageRequest
{
    /// <summary>
    /// 平台标识（iOS/Android）
    /// </summary>
    string Platform { get; set; }

    /// <summary>
    /// 包名
    /// </summary>
    string PackageName { get; set; }

    /// <summary>
    /// 应用版本
    /// </summary>
    string AppVersion { get; set; }

    /// <summary>
    /// 语言代码
    /// </summary>
    string Language { get; set; }

    /// <summary>
    /// 渠道标识
    /// </summary>
    string Channel { get; set; }

}
