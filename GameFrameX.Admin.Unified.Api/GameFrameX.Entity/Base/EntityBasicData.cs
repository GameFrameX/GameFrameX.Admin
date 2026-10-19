using GameFrameX.Foundation.Orm.Entity.Filter;

namespace GameFrameX.Entity.Base;

/// <summary>
/// 业务数据实体基类(数据权限)
/// </summary>
public abstract class EntityBasicData : EntityBasic, IOrganizationIdFilter
{
    /// <summary>
    /// 创建者部门Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者部门Id", IsOnlyIgnoreUpdate = true)]
    public virtual long? CreateOrganizationId { get; set; }

    /// <summary>
    /// 创建者部门
    /// </summary>
    [global::System.Text.Json.Serialization.JsonIgnore]
    [Navigate(NavigateType.OneToOne, nameof(CreateOrganizationId))]
    public virtual TenantOrganizationEntity CreateOrganization { get; set; } = null!;

    /// <summary>
    /// 创建者部门名称
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者部门名称", Length = EntityStringLength.Length256, IsOnlyIgnoreUpdate = true)]
    public virtual string CreateOrganizationName { get; set; } = null!;
}