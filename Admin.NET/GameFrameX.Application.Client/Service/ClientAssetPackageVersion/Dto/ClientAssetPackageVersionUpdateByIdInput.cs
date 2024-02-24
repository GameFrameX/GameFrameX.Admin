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
        
}

