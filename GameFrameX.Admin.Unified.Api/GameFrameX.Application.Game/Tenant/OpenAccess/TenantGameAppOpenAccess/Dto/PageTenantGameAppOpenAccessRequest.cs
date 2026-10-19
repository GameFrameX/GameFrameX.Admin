namespace GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess.Dto;

/// <summary>
/// 应用开放访问凭证分页查询输入。
/// </summary>
public class PageTenantGameAppOpenAccessRequest : BasePageRequest
{
    /// <summary>
    /// 当前应用 Id。凭证页只能在应用工作台内查询。
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 访问 Key 关键字。
    /// </summary>
    public string? AccessKey { get; set; }

    /// <summary>
    /// 责任场景编码。
    /// </summary>
    public string? Scenario { get; set; }

    /// <summary>
    /// 责任主体。
    /// </summary>
    public string? SubjectName { get; set; }

    /// <summary>
    /// 启用状态过滤（可空）。
    /// </summary>
    public bool? IsEnabled { get; set; }
}