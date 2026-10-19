namespace GameFrameX.Application.Game.App.GameAppRemoteConfigMode.Dto;

/// <summary>
/// 远程配置模式定义更新输入参数
/// </summary>
public class UpdateGameAppRemoteConfigModeRequest : BaseUpdateRequest
{
    /// <summary>
    /// 模式编码
    /// </summary>
    [Required(ErrorMessage = "ModeCode不能为空")]
    public string? ModeCode { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    [Required(ErrorMessage = "DisplayName不能为空")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序序号
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; } = true;
}