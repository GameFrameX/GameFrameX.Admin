namespace GameFrameX.Application.Game.App.GameAppRemoteConfigKey.Dto;

/// <summary>
/// 远程配置内容键定义增加输入参数
/// </summary>
public class AddGameAppRemoteConfigKeyRequest : GameAppRemoteConfigKeyBaseRequest
{
    /// <summary>
    /// JSON key 名称
    /// </summary>
    [Required(ErrorMessage = "KeyName不能为空")]
    public override string? KeyName { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    [Required(ErrorMessage = "DisplayName不能为空")]
    public override string? DisplayName { get; set; }
}