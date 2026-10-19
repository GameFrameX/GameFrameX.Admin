namespace GameFrameX.Core.Service.Organization.Dto;

/// <summary>
/// 系统机构输出参数
/// </summary>
public class TenantOrganizationResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父Id
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 级别
    /// </summary>
    public int? Level { get; set; }

    /// <summary>
    /// 机构类型-数据字典
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// 负责人Id
    /// </summary>
    public long? DirectorId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;

    /// <summary>
    /// 机构子项
    /// </summary>
    public List<TenantOrganizationResponse> Children { get; set; } = null!;

    /// <summary>
    /// 是否禁止选中。命名豁免（GFX-707）：树形选择禁用态出站镜像（同实体侧说明，禁用≠软删，前端树组件标准字段名）。
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}