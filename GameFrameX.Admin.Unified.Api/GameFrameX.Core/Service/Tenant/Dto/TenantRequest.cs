namespace GameFrameX.Core.Service.Tenant.Dto;

public class TenantRequest : BaseIdRequest
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}