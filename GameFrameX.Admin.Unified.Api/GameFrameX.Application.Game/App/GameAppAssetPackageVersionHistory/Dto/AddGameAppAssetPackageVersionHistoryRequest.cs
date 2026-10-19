namespace GameFrameX.Application.Game.App.GameAppAssetPackageVersionHistory.Dto;

/// <summary>
/// 资源包版本记录增加输入参数
/// </summary>
public class AddGameAppAssetPackageVersionHistoryRequest : BaseAddRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 资源包版本Id
    /// </summary>
    [Required(ErrorMessage = "资源包版本Id不能为空")]
    public long VersionId { get; set; }

    /// <summary>
    /// 旧资源版本
    /// </summary>
    [Required(ErrorMessage = "旧资源版本不能为空")]
    public string OldVersion { get; set; } = string.Empty;

    /// <summary>
    /// 新资源版本
    /// </summary>
    [Required(ErrorMessage = "新资源版本不能为空")]
    public string ResourceVersion { get; set; } = string.Empty;
}