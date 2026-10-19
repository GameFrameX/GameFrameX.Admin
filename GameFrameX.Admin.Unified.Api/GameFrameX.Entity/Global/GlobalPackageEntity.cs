using GameFrameX.Entity.Routing;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Global;

/// <summary>
/// 系统租户套餐等级表
/// </summary>
[SugarTable(null, "系统租户套餐等级表")]
[GlobalTable]
[IncrementTable]
public class GlobalPackageEntity : EntityBase
{
    /// <summary>
    /// 套餐名称
    /// </summary>
    [SugarColumn(ColumnDescription = "套餐名称", Length = EntityStringLength.Length256)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 套餐编码
    /// </summary>
    [SugarColumn(ColumnDescription = "套餐编码", Length = EntityStringLength.Length256)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 用户数上限
    /// </summary>
    [SugarColumn(ColumnDescription = "用户数上限", DefaultValue = "0")]
    public int MaxUserCount { get; set; } = 10;

    /// <summary>
    /// 组织数上限
    /// </summary>
    [SugarColumn(ColumnDescription = "组织数上限", DefaultValue = "0")]
    public int MaxOrganizationCount { get; set; } = 10;

    /// <summary>
    /// 组织层级深度上限
    /// </summary>
    [SugarColumn(ColumnDescription = "组织层级深度上限", DefaultValue = "0")]
    public int MaxOrganizationLevel { get; set; } = 3;

    /// <summary>
    /// 角色数上限
    /// </summary>
    [SugarColumn(ColumnDescription = "角色数上限", DefaultValue = "0")]
    public int MaxRoleCount { get; set; } = 5;

    /// <summary>
    /// 游戏应用数上限
    /// </summary>
    [SugarColumn(ColumnDescription = "游戏应用数上限", DefaultValue = "0")]
    public int MaxGameAppCount { get; set; } = 1;

    /// <summary>
    /// 每应用分包数上限
    /// </summary>
    [SugarColumn(ColumnDescription = "每应用分包数上限", DefaultValue = "0")]
    public int MaxPackageCountPerApp { get; set; } = 1;

    /// <summary>
    /// 默认过渡期天数
    /// </summary>
    [SugarColumn(ColumnDescription = "默认过渡期天数", DefaultValue = "0")]
    public int GracePeriodDays { get; set; } = 7;

    /// <summary>
    /// 排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序", DefaultValue = "0")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length256)]
    public string? Remark { get; set; }
}