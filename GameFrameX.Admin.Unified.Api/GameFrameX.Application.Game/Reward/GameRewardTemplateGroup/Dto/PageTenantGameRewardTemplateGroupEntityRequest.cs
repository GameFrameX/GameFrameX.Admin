namespace GameFrameX.Application.Game.Reward.GameRewardTemplateGroup.Dto;

/// <summary>
/// 奖励模板分组分页查询输入参数。
/// </summary>
public class PageTenantGameRewardTemplateGroupEntityRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id。
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool? IsEnabled { get; set; }
}