namespace GameFrameX.Application.Game.App.GameAppRemoteConfigMode.Dto;

/// <summary>
/// 远程配置模式定义增加输入参数
/// </summary>
public class AddGameAppRemoteConfigModeRequest : GameAppRemoteConfigModeBaseRequest
{
    /// <summary>
    /// 模式编码
    /// </summary>
    [Required(ErrorMessage = "ModeCode不能为空")]
    public override string? ModeCode { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    [Required(ErrorMessage = "DisplayName不能为空")]
    public override string? DisplayName { get; set; }
}