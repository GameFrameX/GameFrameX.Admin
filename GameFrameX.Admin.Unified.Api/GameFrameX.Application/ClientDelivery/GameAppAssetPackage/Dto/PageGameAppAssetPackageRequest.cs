namespace GameFrameX.Application.ClientDelivery.GameAppAssetPackage.Dto;

/// <summary>
/// 资源包分页查询输入参数
/// </summary>
public class PageGameAppAssetPackageRequest : BasePageRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }
}