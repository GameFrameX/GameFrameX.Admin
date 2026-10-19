using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 系统用户扩展机构表
/// </summary>
[SugarTable(null, "租户库用户扩展机构表")]
[TenantTable]
[IncrementTable]
public class TenantUserExtensionOrganizationEntity : EntityBase
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
    /// 机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "机构Id", DefaultValue = "0")]
    public long OrganizationId { get; set; }

    /// <summary>
    /// 机构
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrganizationId))]
    public TenantOrganizationEntity TenantOrganizationEntity { get; set; } = null!;

    /// <summary>
    /// 职位Id
    /// </summary>
    [SugarColumn(ColumnDescription = "职位Id", DefaultValue = "0")]
    public long PosId { get; set; }

    /// <summary>
    /// 职位
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(PosId))]
    public TenantPositionEntity TenantPositionEntity { get; set; } = null!;

    /// <summary>
    /// 工号
    /// </summary>
    [SugarColumn(ColumnDescription = "工号", Length = EntityStringLength.Length256)]
    public string? JobNum { get; set; }

    /// <summary>
    /// 职级
    /// </summary>
    [SugarColumn(ColumnDescription = "职级", Length = EntityStringLength.Length256)]
    public string? PosLevel { get; set; }

    /// <summary>
    /// 入职日期
    /// </summary>
    [SugarColumn(ColumnDescription = "入职日期")]
    public long? JoinTime { get; set; }
}