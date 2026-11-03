namespace GameFrameX.Application.Game.DockerTag.GameDockerTag.Dto;

/// <summary>
/// 游戏Docker标签实体基类 - 包含 AddInput 和 UpdateInput 的公共字段
/// </summary>
public abstract class TenantGameDockerTagEntityBaseRequest : BaseUpdateRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// tag
    /// </summary>
    public string ServerVersion { get; set; } = null!;

}