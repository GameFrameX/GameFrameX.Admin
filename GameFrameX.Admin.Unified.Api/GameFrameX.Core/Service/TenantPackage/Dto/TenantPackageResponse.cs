namespace GameFrameX.Core.Service.TenantPackage.Dto;

/// <summary>
/// 系统租户套餐输出参数
/// </summary>
// 纯 POCO 投影（C228）：属性名与 GameFrameX.Entity.Global.GlobalPackageEntity 保持一致，
// 供 .Select(u => new TenantPackageResponse {...}) 投影与 Mapster 按名映射。
// MenuIdList 为关联菜单Id列表（来自 GlobalPackageMenuEntity 关系表），非实体列。
public class TenantPackageResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 套餐名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 套餐编码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 用户数上限
    /// </summary>
    public int MaxUserCount { get; set; } = 10;

    /// <summary>
    /// 组织数上限
    /// </summary>
    public int MaxOrganizationCount { get; set; } = 10;

    /// <summary>
    /// 组织层级深度上限
    /// </summary>
    public int MaxOrganizationLevel { get; set; } = 3;

    /// <summary>
    /// 角色数上限
    /// </summary>
    public int MaxRoleCount { get; set; } = 5;

    /// <summary>
    /// 游戏应用数上限
    /// </summary>
    public int MaxGameAppCount { get; set; } = 1;

    /// <summary>
    /// 每应用分包数上限
    /// </summary>
    public int MaxPackageCountPerApp { get; set; } = 1;

    /// <summary>
    /// 默认过渡期天数
    /// </summary>
    public int GracePeriodDays { get; set; } = 7;

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 关联菜单Id列表
    /// </summary>
    public List<long> MenuIdList { get; set; } = new();

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

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}