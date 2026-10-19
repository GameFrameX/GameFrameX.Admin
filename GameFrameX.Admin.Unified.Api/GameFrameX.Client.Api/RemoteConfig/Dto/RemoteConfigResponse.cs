namespace GameFrameX.Client.Api.RemoteConfig.Dto;

/// <summary>
/// 
/// </summary>
public sealed class RemoteConfigResponse
{
    /// <summary>
    /// 检测程序版本地址
    /// </summary>
    public string CheckAppVersionUrl { get; set; } = null!;

    /// <summary>
    /// 检测资源版本地址
    /// </summary>
    public string CheckResourceVersionUrl { get; set; } = null!;

    /// <summary>
    /// Aot 代码列表
    /// </summary>
    public string AotCodeList { get; set; } = null!;

    /// <summary>
    /// 附加内容。JSON对象
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 位置信息
    /// </summary>
    public RemoteConfigLocation Location { get; set; } = null!;
}