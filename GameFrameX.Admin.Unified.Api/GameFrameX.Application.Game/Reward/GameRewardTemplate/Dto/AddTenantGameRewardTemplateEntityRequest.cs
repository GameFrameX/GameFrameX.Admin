namespace GameFrameX.Application.Game.Reward.GameRewardTemplate.Dto;

/// <summary>
/// 奖励模板增加输入参数。
/// </summary>
public class AddTenantGameRewardTemplateEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 应用Id。奖励模板必须绑定具体应用，禁止 AppId=0。
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
    /// 分组Id。
    /// </summary>
    [Required(ErrorMessage = "分组不能为空")]
    public long GroupId { get; set; }

    /// <summary>
    /// 奖励明细 JSON。
    /// </summary>
    [Required(ErrorMessage = "奖励明细不能为空")]
    public string RewardItemsJson { get; set; } = null!;

    /// <summary>
    /// 快照结构版本。
    /// </summary>
    public int SnapshotSchemaVersion { get; set; } = 1;

    /// <summary>
    /// 排序。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool? IsEnabled { get; set; }
}