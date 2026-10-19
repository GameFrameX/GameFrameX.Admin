namespace GameFrameX.Core.Service.Role.Dto;

/// <summary>
/// 授权角色菜单
/// </summary>
public class RoleMenuRequest : BaseIdRequest
{
    /// <summary>
    /// 菜单Id集合
    /// </summary>
    public List<long> MenuIdList { get; set; } = null!;
}