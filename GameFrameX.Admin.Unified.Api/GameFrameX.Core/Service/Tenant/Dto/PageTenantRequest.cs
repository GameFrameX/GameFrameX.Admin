namespace GameFrameX.Core.Service.Tenant.Dto;

public class PageTenantRequest : BasePageRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// 电话
    /// </summary>
    public virtual string Phone { get; set; } = null!;
}