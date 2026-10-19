namespace GameFrameX.Core.Service.TenantPackage.Dto;

public class AddTenantPackageRequest
{
    /// <summary>
    /// 套餐名称
    /// </summary>
    [Required(ErrorMessage = "套餐名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 套餐编码
    /// </summary>
    [Required(ErrorMessage = "套餐编码不能为空")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 用户数上限
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "用户数上限必须大于0")]
    public int MaxUserCount { get; set; } = 10;

    /// <summary>
    /// 组织数上限
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "组织数上限必须大于0")]
    public int MaxOrganizationCount { get; set; } = 10;

    /// <summary>
    /// 组织层级深度上限
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "组织层级深度上限必须大于0")]
    public int MaxOrganizationLevel { get; set; } = 3;

    /// <summary>
    /// 角色数上限
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "角色数上限必须大于0")]
    public int MaxRoleCount { get; set; } = 5;

    /// <summary>
    /// 游戏应用数上限
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "游戏应用数上限必须大于0")]
    public int MaxGameAppCount { get; set; } = 1;

    /// <summary>
    /// 每应用分包数上限
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "每应用分包数上限必须大于0")]
    public int MaxPackageCountPerApp { get; set; } = 1;

    /// <summary>
    /// 默认过渡期天数
    /// </summary>
    [Range(0, 365, ErrorMessage = "过渡期天数必须在0-365之间")]
    public int GracePeriodDays { get; set; } = 7;

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 关联菜单Id列表
    /// </summary>
    public List<long> MenuIdList { get; set; } = new();
}