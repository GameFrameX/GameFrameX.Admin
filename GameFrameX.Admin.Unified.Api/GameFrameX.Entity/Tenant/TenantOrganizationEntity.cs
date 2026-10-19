using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 系统机构表
/// </summary>
[SugarTable(null, "租户库机构表")]
[TenantTable]
[IncrementTable]
public class TenantOrganizationEntity : EntityTenantBase
{
    /// <summary>
    /// 父Id
    /// </summary>
    [SugarColumn(ColumnDescription = "父Id", DefaultValue = "0")]
    public long Pid { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// 编码
    /// </summary>
    [SugarColumn(ColumnDescription = "编码", Length = EntityStringLength.Length256)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 级别
    /// </summary>
    [SugarColumn(ColumnDescription = "级别")]
    public int? Level { get; set; }

    /// <summary>
    /// 机构类型-数据字典
    /// </summary>
    [SugarColumn(ColumnDescription = "机构类型", Length = EntityStringLength.Length256)]
    public string Type { get; set; } = null!;

    /// <summary>
    /// 负责人Id
    /// </summary>
    [SugarColumn(ColumnDescription = "负责人Id", IsNullable = true)]
    public long? DirectorId { get; set; }

    /// <summary>
    /// 负责人
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(DirectorId))]
    public TenantUserEntity Director { get; set; } = null!;

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length256)]
    public string Remark { get; set; } = string.Empty; // DB 列 NOT NULL：补默认空串防 23502（Api#288）

    /// <summary>
    /// 机构子项
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<TenantOrganizationEntity> Children { get; set; } = null!;

    /// <summary>
    /// 是否禁止选中。命名豁免（GFX-707）：树形选择禁用态计算字段（非持久列，IsIgnore），
    /// 由 Service 按用户可选机构动态计算，禁用≠软删；且 disabled 为前端树组件标准节点字段名，改名破坏绑定。
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public bool Disabled { get; set; }
}