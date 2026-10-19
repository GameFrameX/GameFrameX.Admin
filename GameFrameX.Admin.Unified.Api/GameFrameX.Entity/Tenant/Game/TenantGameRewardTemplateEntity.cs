using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 奖励模板。
/// <para>模板用于运营配置复用，业务使用时复制奖励快照；模板修改不影响已生成邮件、兑换码或活动。</para>
/// </summary>
[SugarTable(null, "奖励模板")]
[SugarIndex("idx_tenant_game_reward_template_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[SugarIndex("idx_tenant_game_reward_template_group", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc, nameof(GroupId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameRewardTemplateEntity : EntityBasicSelect, IAppScoped
{
    /// <summary>
    /// 应用Id。奖励模板属于应用内容作用域（TenantId + AppId），必须 &gt; 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 分组Id。必须属于同一应用下的奖励模板分组。
    /// </summary>
    [SugarColumn(ColumnDescription = "分组Id", DefaultValue = "0")]
    public long GroupId { get; set; }

    /// <summary>
    /// 奖励明细 JSON。当前版本为 <c>List&lt;GamePropItem&gt;</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "奖励明细", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string RewardItemsJson { get; set; } = null!;

    /// <summary>
    /// 快照结构版本。当前固定为 1，用于后续扩展奖励结构。
    /// </summary>
    [SugarColumn(ColumnDescription = "快照结构版本", DefaultValue = "0")]
    public int SnapshotSchemaVersion { get; set; } = 1;

    /// <summary>
    /// 排序。
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int Sort { get; set; }
}