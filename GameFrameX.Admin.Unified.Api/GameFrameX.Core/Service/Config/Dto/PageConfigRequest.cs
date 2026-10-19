namespace GameFrameX.Core.Service.Config.Dto;

public class PageConfigRequest : BasePageRequest
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
    /// 分组编码
    /// </summary>
    public string GroupCode { get; set; } = null!;
}