namespace GameFrameX.Core.Service.Role.Dto;

public class PageRoleRequest : BasePageRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// 编码
    /// </summary>
    public virtual string Code { get; set; } = null!;
}