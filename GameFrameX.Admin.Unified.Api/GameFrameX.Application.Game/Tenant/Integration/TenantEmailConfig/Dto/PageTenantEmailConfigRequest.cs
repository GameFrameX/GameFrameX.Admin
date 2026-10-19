namespace GameFrameX.Application.Game.Tenant.Integration.TenantEmailConfig.Dto;

/// <summary>
/// 租户业务邮件配置分页查询输入参数
/// </summary>
public class PageTenantEmailConfigRequest : BaseAppActivePageRequest
{
    /// <summary>
    /// 租户业务邮件用途（精确过滤）。空表示不限。
    /// </summary>
    public int? Purpose { get; set; }
}