namespace GameFrameX.Application.Game.Reward.GameRewardTemplate.Dto;

/// <summary>
/// 奖励模板快照输出。
/// </summary>
public class TenantGameRewardTemplateSnapshotResponse
{
    /// <summary>
    /// 模板Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用Id。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 模板名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 分组Id。
    /// </summary>
    public long GroupId { get; set; }

    /// <summary>
    /// 奖励明细 JSON。业务使用该字段复制快照，不运行时引用模板。
    /// </summary>
    public string RewardItemsJson { get; set; } = null!;

    /// <summary>
    /// 快照结构版本。
    /// </summary>
    public int SnapshotSchemaVersion { get; set; }
}