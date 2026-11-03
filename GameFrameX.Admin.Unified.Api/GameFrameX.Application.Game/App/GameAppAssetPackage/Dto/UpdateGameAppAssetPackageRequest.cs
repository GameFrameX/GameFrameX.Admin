namespace GameFrameX.Application.Game.App.GameAppAssetPackage.Dto;

/// <summary>
/// 资源包更新输入参数
/// </summary>
public class UpdateGameAppAssetPackageRequest : BaseUpdateRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; } = string.Empty;
}