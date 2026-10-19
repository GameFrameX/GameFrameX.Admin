using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 系统角色机构表
/// </summary>
[SugarTable(null, "租户库角色机构表")]
[TenantTable]
[IncrementTable]
public class TenantRoleOrganizationEntity : EntityBase
{
    /// <summary>
    /// 角色Id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id", DefaultValue = "0")]
    public long RoleId { get; set; }

    /// <summary>
    /// 机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "机构Id", DefaultValue = "0")]
    public long OrganizationId { get; set; }

    /// <summary>
    /// 机构
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrganizationId))]
    public TenantOrganizationEntity TenantOrganizationEntity { get; set; } = null!;
}