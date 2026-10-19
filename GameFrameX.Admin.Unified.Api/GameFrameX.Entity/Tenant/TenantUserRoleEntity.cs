using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 系统用户角色表
/// </summary>
[SugarTable(null, "租户库用户角色表")]
[TenantTable]
[IncrementTable]
public class TenantUserRoleEntity : EntityBase
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id", DefaultValue = "0")]
    public long UserId { get; set; }

    /// <summary>
    /// 用户
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public TenantUserEntity User { get; set; } = null!;

    /// <summary>
    /// 角色Id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id", DefaultValue = "0")]
    public long RoleId { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(RoleId))]
    public TenantRoleEntity TenantRoleEntity { get; set; } = null!;
}