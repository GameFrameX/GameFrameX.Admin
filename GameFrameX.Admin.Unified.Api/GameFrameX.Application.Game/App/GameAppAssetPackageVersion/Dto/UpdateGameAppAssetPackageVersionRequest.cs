namespace GameFrameX.Application.Game.App.GameAppAssetPackageVersion.Dto;

/// <summary>
/// 资源包版本更新输入参数
/// </summary>
public class UpdateGameAppAssetPackageVersionRequest : BaseUpdateRequest
{
    /// <summary>
    /// 资源版本
    /// </summary>
    [Required(ErrorMessage = "资源版本不能为空")]
    public string ResourceVersion { get; set; } = string.Empty;

    /// <summary>
    /// 资源下载根路径
    /// </summary>
    [Required(ErrorMessage = "资源下载根路径")]
    public virtual string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 机器码分组id
    /// </summary>
    [Required(ErrorMessage = "机器码分组不能为空")]
    public long DeviceGroupId { get; set; }
}