namespace GameFrameX.Core.Service.TenantPackage.Dto;

public class DeleteTenantPackageRequest
{
    /// <summary>
    /// Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    public long Id { get; set; }
}