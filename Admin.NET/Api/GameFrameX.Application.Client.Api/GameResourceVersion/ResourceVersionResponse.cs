namespace GameFrameX.Application.Api.GameApplicationVersion;

/// <summary>
/// 
/// </summary>
public class ResourceVersionResponse
{
    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// 资源版本
    /// </summary>
    public string ResourceVersion { get; set; }

    /// <summary>
    /// 模块名称
    /// </summary>
    public string ModuleName { get; set; }

    /// <summary>
    /// 平台
    /// </summary>
    public string Platform { get; set; }

    /// <summary>
    /// 根路径
    /// </summary>
    public string RootPath { get; set; }

    public string PackageName { get; set; }
    public string AppVersion { get; set; }
    public string Channel { get; set; }
    public string SubChannel { get; set; }
}