namespace GameFrameX.Application.Game.App.GameAppRemoteConfigMode.Dto;

/// <summary>
/// 远程配置模式定义分页查询输入参数
/// </summary>
public class PageGameAppRemoteConfigModeRequest : BasePageRequest
{
    /// <summary>
    /// 模式编码
    /// </summary>
    public string? ModeCode { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}