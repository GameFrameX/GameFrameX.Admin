namespace GameFrameX.Core.Service.TenantPackage.Dto;

public class PageTenantPackageRequest : BasePageRequest
{
    /// <summary>
    /// 套餐名称
    /// </summary>
    public string? Name { get; set; }
}