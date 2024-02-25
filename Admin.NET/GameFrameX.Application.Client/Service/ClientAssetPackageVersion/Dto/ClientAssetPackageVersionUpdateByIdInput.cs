using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Client;

/// <summary>
/// 资源包版本更新输入参数
/// </summary>
public class UpdateClientAssetPackageVersionInput : BaseUpdateInput
{
    /// <summary>
    /// 资源版本
    /// </summary>
    [Required(ErrorMessage = "资源版本不能为空")]
    public string Version { get; set; }

    /// <summary>
    /// 资源下载根路径
    /// </summary>
    [Required(ErrorMessage = "资源下载根路径")]
    public virtual string RootPath { get; set; }

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; }
}