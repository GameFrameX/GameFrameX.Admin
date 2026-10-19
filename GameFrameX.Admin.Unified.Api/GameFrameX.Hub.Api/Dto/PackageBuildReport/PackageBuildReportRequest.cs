namespace GameFrameX.Hub.Api.Dto.PackageBuildReport;

/// <summary>
/// 打包上报输入（机器凭证驱动，分包 Id 解析应用 / 包名 / 平台 / 渠道）。
/// </summary>
public class PackageBuildReportRequest
{
    /// <summary>
    /// 上报类型：<c>asset</c>（资源包版本）/ <c>release</c>（整包版本）。
    /// </summary>
    public string ReportType { get; set; } = null!;

    /// <summary>
    /// 分包 Id（驱动应用 / 包名 / 平台 / 渠道解析，须属凭证绑定 App）。
    /// </summary>
    public long PackageId { get; set; }

    /// <summary>
    /// 程序版本（打包机显式传，不从 OSS 路径解析）。
    /// </summary>
    public string AppVersion { get; set; } = null!;

    /// <summary>
    /// 资源包名称（asset 必填，须命中资源包定义）。
    /// </summary>
    public string AssetPackageName { get; set; } = null!;

    /// <summary>
    /// 资源版本号（asset 必填）。
    /// </summary>
    public string ResourceVersion { get; set; } = null!;

    /// <summary>
    /// OSS 根路径（asset 必填，实体核心字段，客户端下载依据）。
    /// </summary>
    public string RootPath { get; set; } = null!;

    /// <summary>
    /// 整包下载地址（release 必填，实体核心字段）。
    /// </summary>
    public string DownloadUrl { get; set; } = null!;

    /// <summary>
    /// 语言（为空走 <c>default</c>）。
    /// </summary>
    public string Language { get; set; } = null!;
}