namespace GameFrameX.Core.Service.TenantPackage.Dto;

/// <summary>
/// 租户最终生效配额
/// </summary>
public class EffectiveQuotaResponse
{
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
    /// 有效过渡期天数
    /// </summary>
    public int GracePeriodDays { get; set; } = 7;
}