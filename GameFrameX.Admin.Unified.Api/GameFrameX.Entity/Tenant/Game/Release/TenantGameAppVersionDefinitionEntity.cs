using GameFrameX.Entity.Base;
using GameFrameX.Foundation.Orm.Entity.Filter;

namespace GameFrameX.Entity.Tenant.Game.Release;

/// <summary>
/// 客户端程序版本号字典（应用作用域事实数据）。
/// <para>同一租户下按 <c>AppId</c> 隔离版本定义；同一应用内版本名称唯一，不同应用允许复用相同版本名称。</para>
/// </summary>
[SugarTable(null, "客户端程序版本配置表")]
[SugarIndex("idx_tenant_game_app_version_def_tenant_app_name", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc, nameof(Name), OrderByType.Asc, true)]
[TenantTable]
[IncrementTable]
public sealed class TenantGameAppVersionDefinitionEntity : EntityBasic, IAppScoped, ISelectFilter
{
    /// <summary>
    /// 游戏应用Id。版本定义必须绑定具体应用，必须 &gt; 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "游戏应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [SugarColumn(ColumnDescription = "详细描述", Length = EntityStringLength.Length1024)]
    public string Description { get; set; } = null!;
}