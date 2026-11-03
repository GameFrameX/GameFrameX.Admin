namespace GameFrameX.Application.Game.App.GameAppVersionDefinition.Dto;

/// <summary>
/// 版本管理增加输入参数
/// </summary>
public class AddGameAppVersionDefinitionRequest : BaseNamedDescriptionRequest
{

    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public override string? Name { get; set; }
}