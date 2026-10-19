namespace GameFrameX.Application.System.Tenant.Position.Dto;

public class AddPosRequest : TenantPositionEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "职位名称不能为空")]
    public override string Name { get; set; } = string.Empty;
}