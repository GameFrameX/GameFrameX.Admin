namespace GameFrameX.Application.Game.Reward.GameRewardTemplate.Dto;

/// <summary>
/// 奖励模板输出参数。
/// </summary>
public class TenantGameRewardTemplateEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameRewardTemplateEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 应用Id。奖励模板属于应用内容作用域（TenantId + AppId），必须 &gt; 0。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 分组Id。必须属于同一应用下的奖励模板分组。
    /// </summary>
    public long GroupId { get; set; }

    /// <summary>
    /// 奖励明细 JSON。当前版本为 <c>ListAsync&lt;GamePropItem&gt;</c>。
    /// </summary>
    public string RewardItemsJson { get; set; } = null!;

    /// <summary>
    /// 快照结构版本。当前固定为 1，用于后续扩展奖励结构。
    /// </summary>
    public int SnapshotSchemaVersion { get; set; } = 1;

    /// <summary>
    /// 排序。
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }

    /// <summary>
    /// 分组名称。
    /// </summary>
    public string GroupName { get; set; } = null!;
}