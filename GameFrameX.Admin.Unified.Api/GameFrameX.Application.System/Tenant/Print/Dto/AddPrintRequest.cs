namespace GameFrameX.Application.System.Tenant.Print.Dto;

public class AddPrintRequest : TenantPrintEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "模板名称不能为空")]
    public override string Name { get; set; } = string.Empty;
}