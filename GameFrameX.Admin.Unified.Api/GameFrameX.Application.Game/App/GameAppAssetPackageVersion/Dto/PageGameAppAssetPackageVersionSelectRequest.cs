namespace GameFrameX.Application.Game.App.GameAppAssetPackageVersion.Dto;

/// <summary>
/// 资源包版本选择器输入参数
/// </summary>
public class PageGameAppAssetPackageVersionSelectRequest : BaseSelectPageRequest
{
    /// <summary>
    /// 分包Id
    /// </summary>
    public long? AppPackageId { get; set; }
}