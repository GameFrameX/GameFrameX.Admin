using GameFrameX.Core;
using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Client;

/// <summary>
/// 资源包版本记录分页查询输入参数
/// </summary>
public class ClientAssetPackageVersionHistoryInput : BasePageInput
{
    /// <summary>
    /// 资源包版本Id
    /// </summary>
    public long? VersionId { get; set; }
        
}
