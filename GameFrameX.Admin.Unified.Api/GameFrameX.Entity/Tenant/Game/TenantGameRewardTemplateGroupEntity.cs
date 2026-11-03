using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 奖励模板分组。
/// <para>分组用于邮件、兑换码、活动等运营场景归类奖励模板；分组本身强制绑定具体应用，不存在租户默认分组。</para>
/// </summary>
[SugarTable(null, "奖励模板分组")]
[TenantTable]
[IncrementTable]
public class TenantGameRewardTemplateGroupEntity : EntityBasicSelect
{

    /// <summary>
    /// 排序。
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int Sort { get; set; }
}