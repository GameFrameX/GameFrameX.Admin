namespace GameFrameX.Application.Game.App.GameAppAssetPackage.Dto;

/// <summary>
/// 资源包增加输入参数
/// </summary>
public class AddGameAppAssetPackageRequest : BaseAddRequest
{
    /// <summary>
    /// 游戏应用标识。资源包定义必须绑定具体应用，不能写入 0。
    /// </summary>
    [Required(ErrorMessage = "游戏应用不能为空")]
    [AdaptMember("GameAppId")]
    public long AppId { get; set; }

    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }

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