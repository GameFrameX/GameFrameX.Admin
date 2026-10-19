namespace GameFrameX.Application.Game.Reward.GameRewardTemplateGroup.Dto;

/// <summary>
/// 奖励模板分组增加输入参数。
/// </summary>
public class AddTenantGameRewardTemplateGroupEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 应用Id。奖励模板分组必须绑定具体应用，禁止 AppId=0。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

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