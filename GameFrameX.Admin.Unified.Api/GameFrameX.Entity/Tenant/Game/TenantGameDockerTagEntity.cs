using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏区服
/// </summary>
[SugarTable(null, "游戏区服镜像标签")]
[TenantTable]
[IncrementTable]
public class TenantGameDockerTagEntity : EntityBasicSelect
{

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