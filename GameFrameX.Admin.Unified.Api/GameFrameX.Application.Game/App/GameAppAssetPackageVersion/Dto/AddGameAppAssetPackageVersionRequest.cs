namespace GameFrameX.Application.Game.App.GameAppAssetPackageVersion.Dto;

/// <summary>
/// 资源包版本增加输入参数
/// </summary>
public class AddGameAppAssetPackageVersionRequest : BaseAddRequest
{
    /// <summary>
    /// 应用作用域，0 / null 表示租户默认配置
    /// </summary>
    public virtual long? AppId { get; set; }

    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    public long Id { get; set; }

    /// <summary>
    /// 渠道名称
    /// </summary>
    [Required(ErrorMessage = "渠道名称不能为空")]
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// 平台
    /// </summary>
    [Required(ErrorMessage = "平台不能为空")]
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// 游戏版本
    /// </summary>
    [Required(ErrorMessage = "游戏版本不能为空")]
    public string AppVersion { get; set; } = string.Empty;

    /// <summary>
    /// 包名
    /// </summary>
    [Required(ErrorMessage = "包名不能为空")]
    public string Package { get; set; } = string.Empty;

    /// <summary>
    /// 语言
    /// </summary>
    [Required(ErrorMessage = "语言不能为空")]
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// 资源包名称
    /// </summary>
    [Required(ErrorMessage = "资源包名称不能为空")]
    public string AssetPackageName { get; set; } = string.Empty;

    /// <summary>
    /// 资源下载根路径
    /// </summary>
    [Required(ErrorMessage = "资源下载根路径")]
    public virtual string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// 资源版本
    /// </summary>
    [Required(ErrorMessage = "资源版本不能为空")]
    public string ResourceVersion { get; set; } = string.Empty;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 机器码不能为空
    /// </summary>
    [Required(ErrorMessage = "机器码不能为空")]
    public long DeviceGroupId { get; set; }
}