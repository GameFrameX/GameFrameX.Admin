namespace GameFrameX.Application.NoticeCenter.GameRewardTemplateGroup.Dto;

/// <summary>
/// 奖励模板分组增加输入参数。
/// </summary>
public class AddGameRewardTemplateGroupEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 名称。
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述。
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 排序。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool? IsEnabled { get; set; }
}