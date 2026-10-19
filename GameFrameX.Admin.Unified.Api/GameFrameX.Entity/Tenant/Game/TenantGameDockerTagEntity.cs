using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏区服
/// </summary>
[SugarTable(null, "游戏区服镜像标签")]
[SugarIndex("idx_tenant_game_docker_tag_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameDockerTagEntity : EntityBasicSelect, IAppScoped
{
    /// <summary>
    /// 应用Id。运营内容应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Admin 后台当前应用上下文写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 是否是默认
    /// </summary>
    [SugarColumn(ColumnDescription = "是否是默认", DefaultValue = "false")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// 镜像标签
    /// </summary>
    [SugarColumn(ColumnDescription = "镜像标签")]
    public string ServerVersion { get; set; } = null!;
}