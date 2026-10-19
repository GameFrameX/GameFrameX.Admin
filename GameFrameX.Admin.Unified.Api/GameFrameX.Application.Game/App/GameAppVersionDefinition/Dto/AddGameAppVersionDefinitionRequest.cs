namespace GameFrameX.Application.Game.App.GameAppVersionDefinition.Dto;

/// <summary>
/// 版本管理增加输入参数
/// </summary>
public class AddGameAppVersionDefinitionRequest : BaseNamedDescriptionRequest
{
    /// <summary>
    /// 游戏应用Id
    /// </summary>
    [Required(ErrorMessage = "游戏应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "游戏应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public override string? Name { get; set; }
}