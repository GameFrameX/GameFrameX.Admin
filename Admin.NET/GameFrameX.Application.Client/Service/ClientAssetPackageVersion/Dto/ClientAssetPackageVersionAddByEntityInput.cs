using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Client;

/// <summary>
/// 资源包版本增加输入参数
/// </summary>
public class AddClientAssetPackageVersionInput : BaseAddInput
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }
        
    /// <summary>
    /// 渠道名称
    /// </summary>
    [Required(ErrorMessage = "渠道名称不能为空")]
    public string Channel { get; set; }
        
    /// <summary>
    /// 平台
    /// </summary>
    [Required(ErrorMessage = "平台不能为空")]
    public string Platform { get; set; }
        
    /// <summary>
    /// 游戏版本
    /// </summary>
    [Required(ErrorMessage = "游戏版本不能为空")]
    public string AppVersion { get; set; }
        
    /// <summary>
    /// 包名
    /// </summary>
    [Required(ErrorMessage = "包名不能为空")]
    public string Package { get; set; }
        
    /// <summary>
    /// 语言
    /// </summary>
    [Required(ErrorMessage = "语言不能为空")]
    public string Language { get; set; }
        
    /// <summary>
    /// 资源包名称
    /// </summary>
    [Required(ErrorMessage = "资源包名称不能为空")]
    public string AssetPackageName { get; set; }
        
    /// <summary>
    /// 资源版本
    /// </summary>
    [Required(ErrorMessage = "资源版本不能为空")]
    public string Version { get; set; }
        
    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; }
        
}

