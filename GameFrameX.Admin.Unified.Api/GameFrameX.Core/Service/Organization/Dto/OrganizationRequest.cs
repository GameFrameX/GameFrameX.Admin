namespace GameFrameX.Core.Service.Organization.Dto;

public class OrganizationRequest : BaseIdRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 组织类型
    /// </summary>
    public string Type { get; set; } = null!;
}