namespace GameFrameX.Application.NoticeCenter.GameRewardTemplate.Dto;

/// <summary>
/// 奖励模板分页查询输入参数。
/// </summary>
public class PageGameRewardTemplateEntityRequest : BasePageRequest
{
    /// <summary>
    /// 名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 分组Id。
    /// </summary>
    public long? GroupId { get; set; }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool? IsEnabled { get; set; }
}