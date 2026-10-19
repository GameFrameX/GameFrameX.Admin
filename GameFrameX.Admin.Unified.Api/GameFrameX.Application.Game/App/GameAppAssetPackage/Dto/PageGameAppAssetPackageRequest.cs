namespace GameFrameX.Application.Game.App.GameAppAssetPackage.Dto;

/// <summary>
/// 资源包分页查询输入参数
/// </summary>
public class PageGameAppAssetPackageRequest : BasePageRequest
{
    /// <summary>
    /// 游戏应用标识。资源包定义页面必须从应用工作台进入。
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }
}