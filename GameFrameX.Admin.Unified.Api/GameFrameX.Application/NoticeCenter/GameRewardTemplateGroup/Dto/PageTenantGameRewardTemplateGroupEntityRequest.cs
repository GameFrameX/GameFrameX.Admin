namespace GameFrameX.Application.NoticeCenter.GameRewardTemplateGroup.Dto;

/// <summary>
/// 奖励模板分组分页查询输入参数。
/// </summary>
public class PageTenantGameRewardTemplateGroupEntityRequest : BasePageRequest
{
    /// <summary>
    /// 名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool? IsEnabled { get; set; }
}