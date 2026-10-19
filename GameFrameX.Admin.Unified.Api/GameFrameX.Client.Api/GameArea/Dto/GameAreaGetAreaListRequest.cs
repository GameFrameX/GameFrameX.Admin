namespace GameFrameX.Client.Api.GameArea.Dto;

/// <summary>
/// 
/// </summary>
public class GameAreaGetAreaListRequest
{
    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; } = null!;

    /// <summary>
    /// 包名
    /// </summary>
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 平台
    /// </summary>
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 客户端版本
    /// </summary>
    public string AppVersion { get; set; } = null!;

    /// <summary>
    /// 渠道名称
    /// </summary>
    public string Channel { get; set; } = null!;
}