namespace GameFrameX.Core.Service.Tenant.Dto;

public class AddTenantRequest : TenantResponse
{
    /// <summary>
    /// 租户名称
    /// </summary>
    [Required(ErrorMessage = "租户名称不能为空")]
    [MinLength(2, ErrorMessage = "租户名称不能少于2个字符")]
    public override string Name { get; set; } = null!;

    /// <summary>
    /// 租管账号
    /// </summary>
    [Required(ErrorMessage = "租管账号不能为空")]
    [MinLength(3, ErrorMessage = "租管账号不能少于3个字符")]
    public override string AdminAccount { get; set; } = null!;
}