namespace GameFrameX.Core.Service.Organization.Dto;

public class AddOrganizationRequest : TenantOrganizationEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "组织名称不能为空")]
    public override string Name { get; set; } = null!;
}