namespace GameFrameX.Application.System.Server.Dto;

/// <summary>
/// 框架主要程序集信息
/// </summary>
public class AssemblyInfoResponse
{
    /// <summary>
    /// 程序集名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 程序集版本号
    /// </summary>
    public Version? Version { get; set; }
}