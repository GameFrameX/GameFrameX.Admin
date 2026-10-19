namespace GameFrameX.Application.System.Plugin.Dto;

public class AddPluginRequest : SystemPluginEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "功能名称不能为空")]
    public override string Name { get; set; } = string.Empty;
}