using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Client;

/// <summary>
/// 资源包版本分页查询输入参数
/// </summary>
public class ClientAssetPackageVersionInput : BasePageInput
{
    /// <summary>
    /// 渠道名称
    /// </summary>
    public string? Channel { get; set; }
        
    /// <summary>
    /// 平台
    /// </summary>
    public string? Platform { get; set; }
        
    /// <summary>
    /// 游戏版本
    /// </summary>
    public string? AppVersion { get; set; }
        
    /// <summary>
    /// 包名
    /// </summary>
    public string? Package { get; set; }
        
    /// <summary>
    /// 语言
    /// </summary>
    public string? Language { get; set; }
        
    /// <summary>
    /// 资源包名称
    /// </summary>
    public string? AssetPackageName { get; set; }
        
}
