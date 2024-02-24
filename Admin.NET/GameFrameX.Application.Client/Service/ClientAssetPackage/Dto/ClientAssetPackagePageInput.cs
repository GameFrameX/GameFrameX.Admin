using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Client;

/// <summary>
/// 资源包分页查询输入参数
/// </summary>
public class ClientAssetPackageInput : BasePageInput
{
    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }
        
}
