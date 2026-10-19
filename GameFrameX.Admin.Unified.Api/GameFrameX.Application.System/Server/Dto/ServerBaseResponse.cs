namespace GameFrameX.Application.System.Server.Dto;

/// <summary>
/// 服务器配置信息
/// </summary>
public class ServerBaseResponse
{
    /// <summary>
    /// 主机名称
    /// </summary>
    public string HostName { get; set; } = null!;

    /// <summary>
    /// 操作系统
    /// </summary>
    public string SystemOs { get; set; } = null!;

    /// <summary>
    /// 系统架构
    /// </summary>
    public string OsArchitecture { get; set; } = null!;

    /// <summary>
    /// CPU 核心数
    /// </summary>
    public string ProcessorCount { get; set; } = null!;

    /// <summary>
    /// 系统运行时间
    /// </summary>
    public string SystemRunTime { get; set; } = null!;

    /// <summary>
    /// 外网地址（不可用时为“未知”）
    /// </summary>
    public string RemoteIp { get; set; } = null!;

    /// <summary>
    /// 本地地址（无法获取时为 null）
    /// </summary>
    public string? LocalIp { get; set; }

    /// <summary>
    /// .NET 框架描述
    /// </summary>
    public string FrameworkDescription { get; set; } = null!;

    /// <summary>
    /// 运行环境名称（Development / Production）
    /// </summary>
    public string Environment { get; set; } = null!;

    /// <summary>
    /// 网站根目录（未映射时为 null）
    /// </summary>
    public string? Wwwroot { get; set; }

    /// <summary>
    /// 是否 Stage 环境（Stage环境 / 非Stage环境）
    /// </summary>
    public string Stage { get; set; } = null!;
}