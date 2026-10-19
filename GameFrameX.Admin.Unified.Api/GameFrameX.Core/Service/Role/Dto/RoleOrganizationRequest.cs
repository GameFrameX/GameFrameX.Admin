namespace GameFrameX.Core.Service.Role.Dto;

/// <summary>
/// 授权角色机构
/// </summary>
public class RoleOrganizationRequest : BaseIdRequest
{
    /// <summary>
    /// 数据范围
    /// </summary>
    public int DataScope { get; set; }

    /// <summary>
    /// 机构Id集合
    /// </summary>
    public List<long> OrganizationIdList { get; set; } = null!;
}