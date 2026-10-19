namespace GameFrameX.Core.Service.Role.Dto;

public class AddRoleRequest : TenantRoleEntity
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "角色名称不能为空")]
    public override string Name { get; set; } = null!;

    /// <summary>
    /// 菜单Id集合
    /// </summary>
    public List<long> MenuIdList { get; set; } = null!;
}