namespace GameFrameX.Application.Game.App.GameAppAssetPackageVersion.Dto;

/// <summary>
/// 资源包版本分页查询输入参数
/// </summary>
public class PageGameAppAssetPackageVersionRequest : BaseAppFilterPageRequest
{
    /// <summary>
    /// 资源下载根路径
    /// </summary>
    public string? RootPath { get; set; }

    /// <summary>
    /// 资源包名称
    /// </summary>
    public string? AssetPackageName { get; set; }
}