namespace GameFrameX.Application.Game.DockerTag.GameDockerTag.Dto;

/// <summary>
/// 游戏大区分类分页查询输入参数
/// </summary>
public class PageTenantGameDockerTagEntityRequest : BasePageRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 应用Id（可选过滤）
    /// </summary>
    public long? AppId { get; set; }
}